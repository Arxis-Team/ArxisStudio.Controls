using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace ArxisStudio.Controls;

/// <summary>Снимок одного монитора или окна для пипетки: пиксели BGRA построчно сверху и место на экране.</summary>
/// <param name="bounds">Где снимок лежит на экране — в пикселях.</param>
/// <param name="scaling">Масштаб монитора.</param>
/// <param name="pixels">Пиксели BGRA, строка за строкой сверху, без промежутков.</param>
internal sealed class AxScreenShot(PixelRect bounds, double scaling, byte[] pixels) : IDisposable
{
    private Bitmap? _image;

    /// <summary>Где снимок лежит на экране — в пикселях.</summary>
    public PixelRect Bounds { get; } = bounds;

    /// <summary>Масштаб монитора.</summary>
    public double Scaling { get; } = scaling;

    /// <summary>Картинка снимка — для окна пипетки.</summary>
    public Bitmap Image => _image ??= Picture();

    /// <summary>Цвет пикселя; за краем — ближайшего у края. Экран прозрачности не знает: цвет непрозрачный.</summary>
    /// <param name="pixel">Пиксель снимка.</param>
    public Color ColorAt(PixelPoint pixel)
    {
        var x = Math.Clamp(pixel.X, 0, Bounds.Width - 1);
        var y = Math.Clamp(pixel.Y, 0, Bounds.Height - 1);
        var at = (y * Bounds.Width + x) * 4;

        return Color.FromRgb(pixels[at + 2], pixels[at + 1], pixels[at]);
    }

    /// <inheritdoc/>
    public void Dispose() => _image?.Dispose();

    private Bitmap Picture()
    {
        var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);

        try
        {
            return new Bitmap(PixelFormat.Bgra8888, AlphaFormat.Opaque, handle.AddrOfPinnedObject(),
                Bounds.Size, new Vector(96, 96), Bounds.Width * 4);
        }
        finally
        {
            handle.Free();
        }
    }
}

/// <summary>Откуда пипетка берёт снимки: на Windows — со всех мониторов, иначе — с окна, из которого её позвали.</summary>
internal static class AxScreenShots
{
    private const uint SourceCopy = 0x00CC0020;
    private const uint CaptureLayered = 0x40000000;

    /// <summary>Размер заголовка BITMAPINFOHEADER.</summary>
    private const uint HeaderSize = 40;

    /// <summary>Снимает экран.</summary>
    /// <param name="anchor">Откуда позвали.</param>
    public static IReadOnlyList<AxScreenShot> Take(Visual anchor)
    {
        if (OperatingSystem.IsWindows() && TopLevel.GetTopLevel(anchor)?.Screens?.All is { Count: > 0 } screens)
        {
            try
            {
                return [.. screens.Select(screen => Screen(screen.Bounds, screen.Scaling)).OfType<AxScreenShot>()];
            }
            catch (Exception e) when (e is DllNotFoundException or EntryPointNotFoundException or ExternalException)
            {
                // Снять экран не дали — остаётся окно студии.
            }
        }

        return Window(anchor);
    }

    /// <summary>Снимок монитора через GDI: экран в пикселях, слои окон — тоже.</summary>
    private static AxScreenShot? Screen(PixelRect bounds, double scaling)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return null;

        var screen = GetDC(IntPtr.Zero);
        var memory = CreateCompatibleDC(screen);
        var bitmap = CreateCompatibleBitmap(screen, bounds.Width, bounds.Height);
        var previous = SelectObject(memory, bitmap);

        try
        {
            if (!BitBlt(memory, 0, 0, bounds.Width, bounds.Height, screen, bounds.X, bounds.Y, SourceCopy | CaptureLayered))
                return null;

            // Высота со знаком минус — строки сверху вниз, как их ждёт снимок.
            var info = new BitmapInfo
            {
                Size = HeaderSize,
                Width = bounds.Width,
                Height = -bounds.Height,
                Planes = 1,
                BitCount = 32,
                Compression = 0,
                SizeImage = 0,
                XPelsPerMeter = 0,
                YPelsPerMeter = 0,
                ColorsUsed = 0,
                ColorsImportant = 0,
                Colors = 0,
            };
            var pixels = new byte[bounds.Width * bounds.Height * 4];

            return GetDIBits(memory, bitmap, 0, (uint)bounds.Height, pixels, ref info, 0) == bounds.Height
                ? new AxScreenShot(bounds, scaling, pixels)
                : null;
        }
        finally
        {
            SelectObject(memory, previous);
            DeleteObject(bitmap);
            DeleteDC(memory);
            _ = ReleaseDC(IntPtr.Zero, screen);
        }
    }

    /// <summary>Снимок окна, из которого позвали: чем снять экран, система не дала.</summary>
    private static IReadOnlyList<AxScreenShot> Window(Visual anchor)
    {
        if (TopLevel.GetTopLevel(anchor) is not { } top || top.ClientSize.Width <= 0 || top.ClientSize.Height <= 0)
            return [];

        var scaling = top.RenderScaling;
        var size = PixelSize.FromSize(top.ClientSize, scaling);

        using var target = new RenderTargetBitmap(size, new Vector(96 * scaling, 96 * scaling));

        target.Render(top);

        var pixels = new byte[size.Width * size.Height * 4];
        var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);

        try
        {
            target.CopyPixels(new PixelRect(size), handle.AddrOfPinnedObject(), pixels.Length, size.Width * 4);
        }
        finally
        {
            handle.Free();
        }

        return [new AxScreenShot(new PixelRect(top.PointToScreen(default), size), scaling, pixels)];
    }

    /// <summary>BITMAPINFO: заголовок и одна запись палитры, которой у 32 бит нет.</summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapInfo
    {
        public uint Size;
        public int Width;
        public int Height;
        public ushort Planes;
        public ushort BitCount;
        public uint Compression;
        public uint SizeImage;
        public int XPelsPerMeter;
        public int YPelsPerMeter;
        public uint ColorsUsed;
        public uint ColorsImportant;
        public uint Colors;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetDC(IntPtr window);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int ReleaseDC(IntPtr window, IntPtr context);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern IntPtr CreateCompatibleDC(IntPtr context);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern IntPtr CreateCompatibleBitmap(IntPtr context, int width, int height);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern IntPtr SelectObject(IntPtr context, IntPtr item);

    [DllImport("gdi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool BitBlt(IntPtr target, int x, int y, int width, int height, IntPtr source, int sourceX, int sourceY, uint operation);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern int GetDIBits(IntPtr context, IntPtr bitmap, uint start, uint lines, byte[] bits, ref BitmapInfo info, uint usage);

    [DllImport("gdi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject(IntPtr item);

    [DllImport("gdi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteDC(IntPtr context);
}
