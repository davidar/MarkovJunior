using System.Runtime.InteropServices;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;

namespace MarkovJuniorUI;

public class WriteableBitmapPage : Control
{
    private WriteableBitmap? Bitmap;

    public void Resize(int width, int height)
    {
        if (Bitmap is null || Bitmap.Size.Width != width || Bitmap.Size.Height != height)
        {
            Bitmap?.Dispose();
            Bitmap = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
        }

        Width = width;
        Height = height;

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow.Width = width;
            desktop.MainWindow.Height = height + 30;
        }
    }

    public void SetBitmap(int[] data)
    {
        if (Bitmap is not null)
        {
            using var fb = Bitmap.Lock();
            Marshal.Copy(data, 0, fb.Address, fb.Size.Width * fb.Size.Height);
        }
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);

        Bitmap?.Dispose();
        Bitmap = null;
    }

    public override void Render(DrawingContext context)
    {
        if (Bitmap is not null)
        {
            context.DrawImage(Bitmap,
                new Rect(0, 0, Bitmap.Size.Width, Bitmap.Size.Height),
                new Rect(0, 0, Width, Height));
        }

        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
    }
}
