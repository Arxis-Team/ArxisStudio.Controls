using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace ArxisStudio.Controls;

/// <summary>
/// Пипетка: берёт цвет любого пикселя экрана, как у Unity, — в студии и вне её.
/// </summary>
/// <remarks>
/// <para>
/// Экран снимается сразу и застывает: поверх каждого монитора встаёт окно со снимком, курсор — перекрестие, а рядом с
/// ним — лупа: пиксели вокруг крупно, средний обведён, под ними — его код. Щелчок берёт пиксель под курсором; Esc,
/// правая кнопка и закрытие окна — отмена. С клавиатуры: стрелки двигают точку на пиксель, с Shift — на десять, Enter
/// берёт.
/// </para>
/// <para>
/// На Windows снимается весь экран, все мониторы, каждый со своим масштабом. На других системах снимок экрана — дело
/// системы, и пипетка берёт цвет только с окна студии, из которого её позвали.
/// </para>
/// <para>
/// Пипетка одна на процесс: пока она открыта, второй вызов отвечает «ничего» сразу.
/// </para>
/// </remarks>
public static class AxEyedropper
{
    private static IReadOnlyList<AxEyedropperOverlay> _overlays = [];

    /// <summary>Открыта ли пипетка.</summary>
    public static bool IsOpen => _overlays.Count > 0;

    /// <summary>Шов тестов: откуда берутся снимки; пусто — с экрана.</summary>
    internal static Func<Visual, IReadOnlyList<AxScreenShot>>? Capture { get; set; }

    /// <summary>Окна поверх экранов, пока пипетка открыта.</summary>
    internal static IReadOnlyList<AxEyedropperOverlay> Overlays => _overlays;

    /// <summary>Берёт цвет пикселя, который укажет человек.</summary>
    /// <param name="anchor">Откуда позвали: его окно знает экраны, а без снимка экрана снимается оно само.</param>
    /// <param name="cancellationToken">Отмена — как Esc.</param>
    /// <returns>Цвет — непрозрачный, экран прозрачности не знает; null — отменили или снять было нечего.</returns>
    public static async Task<Color?> PickAsync(Visual anchor, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(anchor);
        Avalonia.Threading.Dispatcher.UIThread.VerifyAccess();

        if (IsOpen)
            return null;

        var shots = (Capture ?? AxScreenShots.Take)(anchor);

        if (shots.Count == 0)
            return null;

        var done = new TaskCompletionSource<Color?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var overlays = shots.Select(shot => new AxEyedropperOverlay(shot, done)).ToArray();

        _overlays = overlays;

        try
        {
            using var cancel = cancellationToken.Register(() => done.TrySetResult(null));

            foreach (var overlay in overlays)
                overlay.Show();

            overlays[0].Activate();

            return await done.Task.ConfigureAwait(true);
        }
        finally
        {
            _overlays = [];

            foreach (var overlay in overlays)
                overlay.Close();

            foreach (var shot in shots)
                shot.Dispose();
        }
    }
}

/// <summary>
/// Окно над одним монитором: застывший снимок, перекрестие и лупа; щелчок отдаёт цвет пикселя.
/// </summary>
internal sealed class AxEyedropperOverlay : Window
{
    private readonly AxScreenShot _shot;
    private readonly TaskCompletionSource<Color?> _done;
    private readonly AxEyedropperLoupe _loupe;
    private readonly Canvas _stage;
    private PixelPoint _pixel;

    public AxEyedropperOverlay(AxScreenShot shot, TaskCompletionSource<Color?> done)
    {
        _shot = shot;
        _done = done;
        _loupe = new AxEyedropperLoupe(shot);

        var picture = new Image { Source = shot.Image, Stretch = Stretch.Fill };

        RenderOptions.SetBitmapInterpolationMode(picture, Avalonia.Media.Imaging.BitmapInterpolationMode.None);

        _stage = new Canvas { Children = { _loupe } };

        WindowDecorations = WindowDecorations.None;
        Topmost = true;
        ShowInTaskbar = false;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.Manual;
        Position = shot.Bounds.Position;
        Width = shot.Bounds.Width / shot.Scaling;
        Height = shot.Bounds.Height / shot.Scaling;
        Cursor = new Cursor(StandardCursorType.Cross);
        Content = new Panel { Children = { picture, _stage } };

        this[!TitleProperty] = this.GetResourceObservable("AxTextEyedropperHint").ToBinding();
        this[!AutomationProperties.NameProperty] = this.GetResourceObservable("AxTextEyedropperHint").ToBinding();

        Move(new PixelPoint(shot.Bounds.Width / 2, shot.Bounds.Height / 2));
    }

    /// <summary>Пиксель под точкой: в пикселях снимка.</summary>
    public PixelPoint Pixel => _pixel;

    /// <summary>Лупа.</summary>
    public AxEyedropperLoupe Loupe => _loupe;

    /// <inheritdoc/>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnPointerMoved(e);
        Move(PixelAt(e.GetPosition(this)));
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnPointerPressed(e);

        var point = e.GetCurrentPoint(this);

        if (point.Properties.IsRightButtonPressed)
        {
            _done.TrySetResult(null);
        }
        else if (point.Properties.IsLeftButtonPressed)
        {
            Move(PixelAt(point.Position));
            _done.TrySetResult(_shot.ColorAt(_pixel));
        }

        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        var step = e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? 10 : 1;

        switch (e.Key)
        {
            case Key.Escape:
                _done.TrySetResult(null);
                break;
            case Key.Enter or Key.Space:
                _done.TrySetResult(_shot.ColorAt(_pixel));
                break;
            case Key.Left:
                Move(new PixelPoint(_pixel.X - step, _pixel.Y));
                break;
            case Key.Right:
                Move(new PixelPoint(_pixel.X + step, _pixel.Y));
                break;
            case Key.Up:
                Move(new PixelPoint(_pixel.X, _pixel.Y - step));
                break;
            case Key.Down:
                Move(new PixelPoint(_pixel.X, _pixel.Y + step));
                break;
            default:
                base.OnKeyDown(e);
                return;
        }

        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnClosed(EventArgs e)
    {
        // Закрыли не пипеткой — Alt+F4, система: это отмена.
        _done.TrySetResult(null);
        base.OnClosed(e);
    }

    /// <summary>Пиксель снимка под точкой окна: снимок растянут на всё окно, каким бы ни был масштаб.</summary>
    private PixelPoint PixelAt(Point point)
    {
        var size = ClientSize;

        if (size.Width <= 0 || size.Height <= 0)
            return _pixel;

        return new PixelPoint(
            (int)Math.Floor(point.X * _shot.Bounds.Width / size.Width),
            (int)Math.Floor(point.Y * _shot.Bounds.Height / size.Height));
    }

    /// <summary>Ставит точку и лупу рядом с ней — внутри окна, по другую сторону, если у края ей нет места.</summary>
    private void Move(PixelPoint pixel)
    {
        _pixel = new PixelPoint(
            Math.Clamp(pixel.X, 0, _shot.Bounds.Width - 1),
            Math.Clamp(pixel.Y, 0, _shot.Bounds.Height - 1));
        _loupe.Center = _pixel;

        var size = ClientSize.Width > 0 ? ClientSize : new Size(Width, Height);
        var at = new Point(
            (_pixel.X + 0.5) * size.Width / _shot.Bounds.Width,
            (_pixel.Y + 0.5) * size.Height / _shot.Bounds.Height);
        var offset = this.TryFindResource("AxEyedropperLoupeOffset", out var found) && found is double value ? value : 0;
        var loupe = _loupe.DesiredSize.Width > 0 ? _loupe.DesiredSize : Measured(_loupe);
        var left = at.X + offset + loupe.Width <= size.Width ? at.X + offset : at.X - offset - loupe.Width;
        var top = at.Y + offset + loupe.Height <= size.Height ? at.Y + offset : at.Y - offset - loupe.Height;

        Canvas.SetLeft(_loupe, Math.Max(0, left));
        Canvas.SetTop(_loupe, Math.Max(0, top));
    }

    private static Size Measured(Control control)
    {
        control.Measure(Size.Infinity);

        return control.DesiredSize;
    }
}

/// <summary>Лупа пипетки: пиксели вокруг точки крупно, средний обведён, под ними — его код.</summary>
internal sealed class AxEyedropperLoupe : Border
{
    private readonly AxEyedropperPixels _pixels;
    private readonly TextBlock _code;

    public AxEyedropperLoupe(AxScreenShot shot)
    {
        _pixels = new AxEyedropperPixels(shot);
        _code = new TextBlock { HorizontalAlignment = HorizontalAlignment.Center };

        this[!BackgroundProperty] = this.GetResourceObservable("AxSurfaceOverlayBrush").ToBinding();
        this[!BorderBrushProperty] = this.GetResourceObservable("AxStrokeStrongBrush").ToBinding();
        this[!BorderThicknessProperty] = this.GetResourceObservable("AxHairlineThickness").ToBinding();
        this[!CornerRadiusProperty] = this.GetResourceObservable("AxCornerRadius").ToBinding();
        this[!PaddingProperty] = this.GetResourceObservable("AxSpaceTightThickness").ToBinding();
        _code[!TextBlock.FontFamilyProperty] = _code.GetResourceObservable("AxFontFamilyMono").ToBinding();
        _code[!TextBlock.ForegroundProperty] = _code.GetResourceObservable("AxTextPrimaryBrush").ToBinding();

        Child = new StackPanel { Children = { _pixels, _code } };
        IsHitTestVisible = false;
        Show();
    }

    /// <summary>Средний пиксель лупы — в пикселях снимка.</summary>
    public PixelPoint Center
    {
        get => _pixels.Center;
        set
        {
            _pixels.Center = value;
            Show();
        }
    }

    /// <summary>Код среднего пикселя, как его показывает лупа.</summary>
    public string? Code => _code.Text;

    private void Show() => _code.Text = AxColorField.Format(_pixels.Color, alpha: false);
}

/// <summary>Пиксели лупы: квадрат клеток вокруг среднего, каждая — клетка увеличения.</summary>
internal sealed class AxEyedropperPixels : Control
{
    private readonly AxScreenShot _shot;
    private PixelPoint _center;

    public AxEyedropperPixels(AxScreenShot shot) => _shot = shot;

    /// <summary>Средний пиксель.</summary>
    public PixelPoint Center
    {
        get => _center;
        set
        {
            _center = value;
            InvalidateVisual();
        }
    }

    /// <summary>Цвет среднего пикселя.</summary>
    public Color Color => _shot.ColorAt(_center);

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        var side = Cells() * Zoom();

        return new Size(side, side);
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var cells = Cells();
        var zoom = Zoom();
        var half = cells / 2;

        for (var row = 0; row < cells; row++)
        {
            for (var column = 0; column < cells; column++)
            {
                var color = _shot.ColorAt(new PixelPoint(_center.X - half + column, _center.Y - half + row));

                context.FillRectangle(new Avalonia.Media.Immutable.ImmutableSolidColorBrush(color),
                    new Rect(column * zoom, row * zoom, zoom, zoom));
            }
        }

        // Средний — двойной обводкой: тёмная и светлая видны на любом цвете под ней.
        var middle = new Rect(half * zoom, half * zoom, zoom, zoom);

        if (Brush("AxColorAlphaEmptyBrush") is { } dark)
            context.DrawRectangle(new Avalonia.Media.Immutable.ImmutablePen(dark.ToImmutable()), middle);

        if (Brush("AxColorAlphaFillBrush") is { } light)
            context.DrawRectangle(new Avalonia.Media.Immutable.ImmutablePen(light.ToImmutable()), middle.Deflate(1));
    }

    private IBrush? Brush(string key) =>
        this.TryFindResource(key, ActualThemeVariant, out var found) ? found as IBrush : null;

    private int Cells() => this.TryFindResource("AxEyedropperLoupeCells", out var found) && found is double value ? Odd((int)value) : 1;

    private double Zoom() => this.TryFindResource("AxEyedropperLoupeZoom", out var found) && found is double value ? value : 1;

    /// <summary>Клеток нечётно: у среднего пикселя есть своя клетка.</summary>
    private static int Odd(int cells) => cells % 2 == 0 ? cells + 1 : Math.Max(1, cells);
}
