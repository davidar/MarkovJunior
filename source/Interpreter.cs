// Copyright (C) 2022 Maxim Gumin, The MIT License (MIT)

using System;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Diagnostics;

class Interpreter
{
    public Branch? root, current;
    public Grid grid;
    readonly Grid startgrid;
    readonly bool origin;
    public Random random;

    public List<(int, int, int)> changes;
    public List<int> first;
    public int counter;
    
    public bool gif;

    private Interpreter(bool origin, Grid grid)
    {
        this.origin = origin;
        this.grid = grid;
        startgrid = grid;
        changes = new List<(int, int, int)>();
        first = new List<int>();
        random = new Random();
    }

    public static Interpreter? Load(XElement xelem, int MX, int MY, int MZ)
    {
        var grid = Grid.Load(xelem, MX, MY, MZ);
        if (grid == null)
        {
            Debug.WriteLine("failed to load grid");
            return null;
        }
        Interpreter ip = new(xelem.Get("origin", false), grid);

        var symmetryString = xelem.Get<string?>("symmetry", null);
        var symmetry = SymmetryHelper.GetSymmetry(ip.grid.MZ == 1, symmetryString, AH.Array1D(ip.grid.MZ == 1 ? 8 : 48, true));
        if (symmetry == null)
        {
            WriteLine($"unknown symmetry {symmetryString} at line {xelem.LineNumber()}");
            return null;
        }

        var topnode = Node.Factory(xelem, symmetry, ip, ip.grid);
        if (topnode == null) return null;
        ip.root = topnode is Branch ? topnode as Branch : new MarkovNode(topnode, ip);

        return ip;
    }

    public IEnumerable<(byte[], char[], int, int, int)> Run(int seed, int steps, bool gif)
    {
        random = new Random(seed);
        grid = startgrid;
        grid.Clear();
        if (origin) grid.state[grid.MX / 2 + (grid.MY / 2) * grid.MX + (grid.MZ / 2) * grid.MX * grid.MY] = 1;

        changes.Clear();
        first.Clear();
        first.Add(0);

        root?.Reset();
        current = root;

        this.gif = gif;
        counter = 0;
        while (current != null && (steps <= 0 || counter < steps))
        {
            if (gif)
            {
                Debug.WriteLine($"[{counter}]");
                yield return (grid.state, grid.characters, grid.MX, grid.MY, grid.MZ);
            }

            current.Go();
            counter++;
            first.Add(changes.Count);
        }

        yield return (grid.state, grid.characters, grid.MX, grid.MY, grid.MZ);
    }

    public static void WriteLine(string s) => Debug.WriteLine(s);
    public static void Write(string s) => Debug.Write(s);
}
