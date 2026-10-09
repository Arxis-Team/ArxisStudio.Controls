using System.Globalization;
using Avalonia;
using Avalonia.Automation;
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

/// <summary>Что тянет ползунок цвета.</summary>
public enum AxColorChannel
{
    /// <summary>Оттенок, 0…360°.</summary>
    Hue,

    /// <summary>Насыщенность, 0…1.</summary>
    Saturation,

    /// <summary>Яркость, 0…1.</summary>
    Value,

    /// <summary>Непрозрачность, 0…1.</summary>
    Alpha,
}

/// <summary>
/// Ползунок одного канала цвета: оттенок, насыщенность, яркость или непрозрачность — по градиенту этого канала.
/// </summary>
/// <remarks>
/// <para>
/// Дорожка — градиент канала при остальных составляющих цвета: оттенок — радугой, непрозрачность — цветом от
/// прозрачного к плотному поверх шахматки, которую даёт тема (<c>:alpha</c>). Градиент ставит контрол: его
/// цвета — данные модели, а не тема.
/// </para>
/// <para>
/// События те же, что у спектра и выбора цвета: на ходу — <see cref="AxColorPicker.ColorChangingEvent"/>, при
/// отпускании — одна фиксация, Esc возвращает цвет начала. С клавиатуры ← и ↓ уменьшают, → и ↑ увеличивают —
/// на градус оттенка или сотую доли, с Shift вдесятеро; Home и End — края. Диктору ползунок — диапазон со
/// значением в единицах канала.
/// </para>
/// </remarks>
[TemplatePart(GradientPart, typeof(Border))]
[TemplatePart(ThumbPart, typeof(Control))]
[PseudoClasses(":pressed", ":alpha")]
public class AxColorSlider : TemplatedControl
{
    /// <summary>Какой канал тянет ползунок.</summary>
    public static readonly StyledProperty<AxColorChannel> ChannelProperty =
        AvaloniaProperty.Register<AxColorSlider, AxColorChannel>(nameof(Channel));

    /// <summary>Цвет в модели HSV: ползунок меняет в нём свой канал.</summary>
    public static readonly StyledProperty<HsvColor> HsvProperty =
        AvaloniaProperty.Register<AxColorSlider, HsvColor>(
            nameof(Hsv), new HsvColor(1, 0, 0, 1), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Имя дорожки с градиентом канала.</summary>
    private const string GradientPart = "PART_Gradient";

    /// <summary>Имя ручки: контрол двигает её сдвигом, не трогая раскладки.</summary>
    private const string ThumbPart = "PART_Thumb";

    private readonly TranslateTransform _shift = new();
    private Border? _gradient;
    private Control? _thumb;
    private bool _dragging;
    private HsvColor _start;
    private IPointer? _pointer;

    static AxColorSlider()
    {
        FocusableProperty.OverrideDefaultValue<AxColorSlider>(true);
        ChannelProperty.Changed.AddClassHandler<AxColorSlider>((slider, change) =>
        {
            slider.PseudoClasses.Set(":alpha", change.GetNewValue<AxColorChannel>() == AxColorChannel.Alpha);
            slider.Paint();
            slider.Place();
        });
        HsvProperty.Changed.AddClassHandler<AxColorSlider>((slider, change) =>
        {
            slider.Paint();
            slider.Place();
            slider.Announce(change.GetOldValue<HsvColor>());
        });
    }

    /// <inheritdoc cref="ChannelProperty"/>
    public AxColorChannel Channel
    {
        get => GetValue(ChannelProperty);
        set => SetValue(ChannelProperty, value);
    }

    /// <inheritdoc cref="HsvProperty"/>
    public HsvColor Hsv
    {
        get => GetValue(HsvProperty);
        set => SetValue(HsvProperty, value);
    }

    /// <summary>Значение канала в его единицах: градусы оттенка или доля.</summary>
    public double Value => Read(Hsv, Channel);

    /// <summary>Верхний край канала: 360 у оттенка, 1 у остальных.</summary>
    public double Maximum => Channel == AxColorChannel.Hue ? 360 : 1;

    /// <summary>Идёт протяжка мышью.</summary>
    public bool IsDragging => _dragging;

    /// <summary>Чистый оттенок — при полной насыщенности и яркости.</summary>
    /// <param name="hue">Оттенок, 0…360°.</param>
    internal static Color PureHue(double hue) => new HsvColor(1, hue, 1, 1).ToRgb();

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnApplyTemplate(e);

        _gradient = e.NameScope.Find<Border>(GradientPart);
        _thumb = e.NameScope.Find<Control>(ThumbPart);

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

        if (_gradient is null || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        _start = Hsv;
        _dragging = true;
        _pointer = e.Pointer;
        PseudoClasses.Set(":pressed", true);
        e.Pointer.Capture(this);
        MoveTo(e.GetPosition(_gradient).X);
        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnPointerMoved(e);

        if (_dragging && _gradient is not null)
            MoveTo(e.GetPosition(_gradient).X);
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

        var step = (Channel == AxColorChannel.Hue ? 1 : 0.01) *
                   (e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? 10 : 1);
        var handled = e.Key switch
        {
            Key.Escape when _dragging => Finish(commit: false),
            Key.Left or Key.Down => Commit(Value - step),
            Key.Right or Key.Up => Commit(Value + step),
            Key.Home => Commit(0),
            Key.End => Commit(Maximum),
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
    protected override AutomationPeer OnCreateAutomationPeer() => new SliderPeer(this);

    /// <summary>Ставит канал по точке дорожки и показывает его на ходу.</summary>
    private void MoveTo(double x)
    {
        var width = _gradient!.Bounds.Width;

        if (width <= 0)
            return;

        var next = Write(Hsv, Channel, Math.Clamp(x / width, 0, 1) * Maximum);

        if (next.Equals(Hsv))
            return;

        SetCurrentValue(HsvProperty, next);
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

    /// <summary>Ставит канал и фиксирует его: шаг стрелкой, край, значение от диктора.</summary>
    private bool Commit(double value)
    {
        if (_dragging)
            return true;

        var next = Write(Hsv, Channel, Math.Clamp(value, 0, Maximum));

        if (next.Equals(Hsv))
            return true;

        SetCurrentValue(HsvProperty, next);
        RaiseEvent(new AxColorEventArgs(AxColorPicker.ColorCommittedEvent, Hsv.ToRgb()));

        return true;
    }

    /// <summary>Дорожка — градиент канала при остальных составляющих.</summary>
    private void Paint()
    {
        if (_gradient is null)
            return;

        var hsv = Hsv;
        var brush = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
        };

        if (Channel == AxColorChannel.Hue)
        {
            // Шесть граней круга и замыкание: красный, жёлтый, зелёный, голубой, синий, пурпурный, снова красный.
            for (var at = 0; at <= 6; at++)
                brush.GradientStops.Add(new GradientStop(PureHue(at * 60), at / 6d));
        }
        else
        {
            brush.GradientStops.Add(new GradientStop(Write(hsv, Channel, 0).ToRgb(), 0));
            brush.GradientStops.Add(new GradientStop(Write(hsv, Channel, Maximum).ToRgb(), 1));
        }

        _gradient.Background = brush;
    }

    /// <summary>Ручка — в точке значения канала.</summary>
    private void Place()
    {
        if (_gradient is null || _thumb is null)
            return;

        var width = _gradient.Bounds.Width;
        var thumb = _thumb.Bounds.Size;

        // Ручка — на середине контрола: дорожка тоньше ручки и стоит по центру его высоты.
        _shift.X = Value / Maximum * width - thumb.Width / 2;
        _shift.Y = (Bounds.Height - thumb.Height) / 2;
    }

    /// <summary>Говорит диктору, что значение канала сдвинулось.</summary>
    private void Announce(HsvColor previous)
    {
        if (ControlAutomationPeer.FromElement(this) is { } peer && !Read(previous, Channel).Equals(Value))
            peer.RaisePropertyChangedEvent(RangeValuePatternIdentifiers.ValueProperty, Read(previous, Channel), Value);
    }

    private static double Read(HsvColor hsv, AxColorChannel channel) => channel switch
    {
        AxColorChannel.Hue => hsv.H,
        AxColorChannel.Saturation => hsv.S,
        AxColorChannel.Value => hsv.V,
        _ => hsv.A,
    };

    /// <remarks>
    /// Оттенок у правого края не доходит до 360: модель сводит 360 к нулю, и ручка, дотянутая до края,
    /// прыгала бы в начало дорожки. Цвет у края тот же — красный.
    /// </remarks>
    private static HsvColor Write(HsvColor hsv, AxColorChannel channel, double value) => channel switch
    {
        AxColorChannel.Hue => new HsvColor(hsv.A, Math.Min(value, 360 - 1e-6), hsv.S, hsv.V),
        AxColorChannel.Saturation => new HsvColor(hsv.A, hsv.H, value, hsv.V),
        AxColorChannel.Value => new HsvColor(hsv.A, hsv.H, hsv.S, value),
        _ => new HsvColor(value, hsv.H, hsv.S, hsv.V),
    };

    /// <summary>Диктору ползунок — диапазон со значением в единицах канала: градусы оттенка или доля.</summary>
    private sealed class SliderPeer(AxColorSlider owner) : ControlAutomationPeer(owner), IRangeValueProvider
    {
        private AxColorSlider Slider => (AxColorSlider)Owner;

        public bool IsReadOnly => !Slider.IsEffectivelyEnabled;

        public double Minimum => 0;

        public double Maximum => Slider.Maximum;

        public double Value => Slider.Value;

        public double SmallChange => Slider.Channel == AxColorChannel.Hue ? 1 : 0.01;

        public double LargeChange => SmallChange * 10;

        public void SetValue(double value) => Slider.Commit(value);

        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Slider;

        protected override string? GetNameCore() =>
            base.GetNameCore() is { Length: > 0 } name
                ? name
                : Slider.Channel.ToString().ToLower(CultureInfo.InvariantCulture);
    }
}
