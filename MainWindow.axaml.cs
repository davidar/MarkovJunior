using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace MarkovJuniorUI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    async Task<TimeSpan?> Run()
    {
        Stopwatch sw = Stopwatch.StartNew();
        Random meta = new();

        var xroot = XDocument.Load("resources/palette.xml").Root;
        if (xroot is null)
        {
            Debug.WriteLine("unable to load palette");
            return null;
        }
        Dictionary<char, int> palette = xroot.Elements("color").ToDictionary(x => x.Get<char>("symbol"), x => (255 << 24) + Convert.ToInt32(x.Get<string>("value"), 16));

        string name = "BasicSnake";
        int size = 23;

        var model = XDocument.Load($"models/{name}.xml", LoadOptions.SetLineInfo).Root;
        if (model is null)
        {
            Debug.WriteLine("unable to parse model");
            return null;
        }

        var interpreter = Interpreter.Load(model, size, size, 1);
        if (interpreter is null)
        {
            Debug.WriteLine("unable to load model");
            return null;
        }

        int pixelsize = 16;
        int steps = 100;
        int margin = 150;

        var fb = this.FindControl<WriteableBitmapPage>("Framebuffer");
        var status = this.FindControl<TextBlock>("TextBlock_Result");

        foreach (var (result, legend, FX, FY, FZ) in interpreter.Run(meta.Next(), steps, true))
        {
            var colors = legend.Select(ch => palette[ch]).ToArray();
            var (bitmap, width, height) = Graphics.Render(result, FX, FY, FZ, colors, pixelsize, margin);
            if (interpreter.root is null || interpreter.current is null)
            {
                Debug.WriteLine("interpreter failed");
                return null;
            }
            GUI.Draw(name, interpreter.root, interpreter.current, bitmap, width, height, palette);
            fb.Resize(width, height);
            fb.SetBitmap(bitmap);
            status.Text = ($"{interpreter.counter} / {steps}");
            await Task.Delay(1000 / 60);
        }

        return sw.Elapsed;
    }

    private async void Button_OnClick(object sender, RoutedEventArgs e)
    {
        var result = await Dispatcher.UIThread.InvokeAsync(Run, DispatcherPriority.Background);
        Debug.WriteLine($"Time elapsed: {result}");
    }
}
