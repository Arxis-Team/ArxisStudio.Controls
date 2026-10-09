using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace ArxisStudio.Controls;

/// <summary>
/// Спектр выбора цвета: по горизонтали насыщенность, по вертикали яркость, при оттенке цвета.
/// </summary>
/// <remarks>
/// <para>
/// Квадрат — два градиента поверх друг друга: белый к чистому оттенку слева направо и прозрачный к чёрному
/// сверху вниз. Их наложение и есть насыщенность и яркость модели HSV — квадрат не рисуется по точкам. Цвета
/// градиентов — данные модели, а не тема: тема даёт рамку, скругление и ручку.
/// </para>
/// <para>
/// Тянут мышью: видно на ходу (<see cref="AxColorPicker.ColorChangingEvent"/>), фиксируется одно при
/// отпускании (<see cref="AxColorPicker.ColorCommittedEvent"/>), Esc возвращает цвет начала
/// (<see cref="AxColorPicker.ColorCancelledEvent"/>). С клавиатуры ← и → двигают насыщенность, ↑ и ↓ —
/// яркость, на сотую, с Shift — на десятую; каждый шаг фиксируется.
/// </para>
/// </remarks>
[TemplatePart(SaturationPart, typeof(Border))]
[TemplatePart(ShadePart, typeof(Border))]
[TemplatePart(ThumbPart, typeof(Control))]
[PseudoClasses(":pressed")]
public class AxColorSpectrum : TemplatedControl
{
    /// <summary>Цвет в модели HSV: тянут его насыщенность и яркость, а оттенок и прозрачность держат.</summary>
    public static readonly StyledProperty<HsvColor> HsvProperty =
        AvaloniaProperty.Register<AxColorSpectrum, HsvColor>(
            nameof(Hsv), new HsvColor(1, 0, 0, 1), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Шаг стрелки — сотая доли.</summary>
    public const double Step = 0.01;

    /// <summary>Шаг стрелки с Shift — десятая доли.</summary>
    public const double LargeStep = 0.1;

    /// <summary>Имя полотна насыщенности в теме: градиент к оттенку ставит контрол.</summary>
    private const string SaturationPart = "PART_Saturation";

    /// <summary>Имя полотна яркости поверх него.</summary>
    private const string ShadePart = "PART_Shade";

    /// <summary>Имя ручки: контрол двигает её сдвигом, не трогая раскладки.</summary>
    private const string ThumbPart = "PART_Thumb";

    private readonly TranslateTransform _shift = new();
    private Border? _saturation;
    private Control? _thumb;
    private bool _dragging;
    private HsvColor _start;
    private IPointer? _pointer;

    static AxColorSpectrum()
    {
        FocusableProperty.OverrideDefaultValue<AxColorSpectrum>(true);
        HsvProperty.Changed.AddClassHandler<AxColorSpectrum>((spectrum, _) =>
        {
            spectrum.Paint();
            spectrum.Place();
        });
    }

    /// <inheritdoc cref="HsvProperty"/>
    public HsvColor Hsv
    {
        get => GetValue(HsvProperty);
        set => SetValue(HsvProperty, value);
    }

    /// <summary>Идёт протяжка мышью.</summary>
    public bool IsDragging => _dragging;

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnApplyTemplate(e);

        _saturation = e.NameScope.Find<Border>(SaturationPart);
        _thumb = e.NameScope.Find<Control>(ThumbPart);

        if (e.NameScope.Find<Border>(ShadePart) is { } shade)
            shade.Background = Gradient(new RelativePoint(0, 1, RelativeUnit.Relative), Color.FromArgb(0, 0, 0, 0), Colors.Black);

        if (_thumb is not null)
            _thumb.RenderTransform = _shift;

        Paint();
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);

        Place();

        return size;
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnPointerPressed(e);

        if (_saturation is null || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        _start = Hsv;
        _dragging = true;
        _pointer = e.Pointer;
        PseudoClasses.Set(":pressed", true);
        e.Pointer.Capture(this);
        MoveTo(e.GetPosition(_saturation));
        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnPointerMoved(e);

        if (_dragging && _saturation is not null)
            MoveTo(e.GetPosition(_saturation));
    }

    /// <inheritdoc/>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        Finish(commit: true);
    }

    /// <inheritdoc/>
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);

        Finish(commit: true);
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        var step = e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? LargeStep : Step;
        var handled = e.Key switch
        {
            Key.Escape when _dragging => Finish(commit: false),
            Key.Left => Nudge(-step, 0),
            Key.Right => Nudge(step, 0),
            Key.Up => Nudge(0, step),
            Key.Down => Nudge(0, -step),
            _ => false,
        };

        if (handled)
        {
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    /// <inheritdoc/>
    protected override AutomationPeer OnCreateAutomationPeer() => new SpectrumPeer(this);

    /// <summary>Ставит насыщенность и яркость по точке полотна и показывает их на ходу.</summary>
    private void MoveTo(Point point)
    {
        var size = _saturation!.Bounds.Size;

        if (size.Width <= 0 || size.Height <= 0)
            return;

        var saturation = Math.Clamp(point.X / size.Width, 0, 1);
        var value = Math.Clamp(1 - point.Y / size.Height, 0, 1);

        if (Hsv.S.Equals(saturation) && Hsv.V.Equals(value))
            return;

        SetCurrentValue(HsvProperty, new HsvColor(Hsv.A, Hsv.H, saturation, value));
        RaiseEvent(new AxColorEventArgs(AxColorPicker.ColorChangingEvent, Hsv.ToRgb()));
    }

    /// <summary>Кончает протяжку: одна фиксация — или возврат к цвету начала — и мышь отпущена.</summary>
    private bool Finish(bool commit)
    {
        if (!_dragging)
            return false;

        var pointer = _pointer;

        _dragging = false;
        _pointer = null;
        PseudoClasses.Set(":pressed", false);

        if (commit)
        {
            if (!Hsv.Equals(_start))
                RaiseEvent(new AxColorEventArgs(AxColorPicker.ColorCommittedEvent, Hsv.ToRgb()));
        }
        else
        {
            SetCurrentValue(HsvProperty, _start);
            RaiseEvent(new RoutedEventArgs(AxColorPicker.ColorCancelledEvent));
        }

        if (pointer is not null && ReferenceEquals(pointer.Captured, this))
            pointer.Capture(null);

        return true;
    }

    /// <summary>Шаг стрелкой: насыщенность и яркость сдвигаются, и шаг фиксируется.</summary>
    private bool Nudge(double saturation, double value)
    {
        if (_dragging)
            return true;

        var next = new HsvColor(
            Hsv.A, Hsv.H, Math.Clamp(Hsv.S + saturation, 0, 1), Math.Clamp(Hsv.V + value, 0, 1));

        if (next.Equals(Hsv))
            return true;

        SetCurrentValue(HsvProperty, next);
        RaiseEvent(new AxColorEventArgs(AxColorPicker.ColorCommittedEvent, Hsv.ToRgb()));

        return true;
    }

    /// <summary>Полотно насыщенности — от белого к чистому оттенку.</summary>
    private void Paint()
    {
        if (_saturation is not null)
            _saturation.Background = Gradient(new RelativePoint(1, 0, RelativeUnit.Relative), Colors.White, AxColorSlider.PureHue(Hsv.H));
    }

    /// <summary>Ручка — в точке насыщенности и яркости.</summary>
    private void Place()
    {
        if (_saturation is null || _thumb is null)
            return;

        var size = _saturation.Bounds.Size;
        var thumb = _thumb.Bounds.Size;

        _shift.X = Hsv.S * size.Width - thumb.Width / 2;
        _shift.Y = (1 - Hsv.V) * size.Height - thumb.Height / 2;
    }

    /// <summary>Градиент от левого верхнего угла к точке <paramref name="end"/>.</summary>
    private static LinearGradientBrush Gradient(RelativePoint end, Color from, Color to) => new()
    {
        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
        EndPoint = end,
        GradientStops = { new GradientStop(from, 0), new GradientStop(to, 1) },
    };

    /// <summary>Диктору спектр — значение цвета записью, которой его пишет разметка.</summary>
    private sealed class SpectrumPeer(AxColorSpectrum owner) : ControlAutomationPeer(owner), IValueProvider
    {
        public bool IsReadOnly => true;

        public string? Value => AxColorField.Format(((AxColorSpectrum)Owner).Hsv.ToRgb());

        public void SetValue(string? value) =>
            throw new InvalidOperationException("Цвет спектра ставят мышью и стрелками, а текстом — поле цвета.");

        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Custom;
    }
}
