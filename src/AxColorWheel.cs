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
using Avalonia.Media.Immutable;
using Avalonia.Rendering;

namespace ArxisStudio.Controls;

/// <summary>
/// Кольцо оттенков, как у Unity: красный справа, оттенок растёт против часовой стрелки; внутри кольца стоит квадрат
/// насыщенности и яркости (<see cref="AxColorSpectrum"/>).
/// </summary>
/// <remarks>
/// <para>
/// Кольцо рисует себя само — отрезками по два градуса, каждый своим чистым оттенком, — и ручку на оттенке модели:
/// кольцо поперёк полосы оттенков, светлое (<see cref="ThumbBrush"/>, <see cref="ThumbThickness"/>) в тёмной кайме
/// снаружи (<see cref="ThumbOutlineBrush"/>, <see cref="ThumbOutlineThickness"/>), как ручка спектра. Нажимается только
/// кольцо: точка в середине уходит квадрату, который стоит поверх, а мимо кольца — никому.
/// </para>
/// <para>
/// События те же, что у спектра и ползунка: на ходу — <see cref="AxColorPicker.ColorChangingEvent"/>, при
/// отпускании — одна фиксация, Esc посреди протяжки возвращает оттенок начала. С клавиатуры ← и ↓ уменьшают
/// оттенок, → и ↑ увеличивают — на градус, с Shift на десять; круг замкнут. Диктору кольцо — диапазон 0…360°.
/// </para>
/// </remarks>
[PseudoClasses(":pressed")]
public class AxColorWheel : TemplatedControl, ICustomHitTest
{
    /// <summary>Выбранный цвет в модели HSV: кольцо правит оттенок.</summary>
    public static readonly StyledProperty<HsvColor> HsvProperty =
        AvaloniaProperty.Register<AxColorWheel, HsvColor>(
            nameof(Hsv), new HsvColor(1, 0, 0, 1), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Толщина кольца.</summary>
    public static readonly StyledProperty<double> RingThicknessProperty =
        AvaloniaProperty.Register<AxColorWheel, double>(nameof(RingThickness));

    /// <summary>Кисть ручки.</summary>
    public static readonly StyledProperty<IBrush?> ThumbBrushProperty =
        AvaloniaProperty.Register<AxColorWheel, IBrush?>(nameof(ThumbBrush));

    /// <summary>Кайма ручки — видна на любом оттенке под ней.</summary>
    public static readonly StyledProperty<IBrush?> ThumbOutlineBrushProperty =
        AvaloniaProperty.Register<AxColorWheel, IBrush?>(nameof(ThumbOutlineBrush));

    /// <summary>Толщина светлого кольца ручки.</summary>
    public static readonly StyledProperty<double> ThumbThicknessProperty =
        AvaloniaProperty.Register<AxColorWheel, double>(nameof(ThumbThickness));

    /// <summary>Толщина тёмной каймы ручки — снаружи светлого кольца.</summary>
    public static readonly StyledProperty<double> ThumbOutlineThicknessProperty =
        AvaloniaProperty.Register<AxColorWheel, double>(nameof(ThumbOutlineThickness));

    /// <summary>Шаг по кругу, в градусах: два — гладко на глаз и немного геометрии.</summary>
    private const double Segment = 2;

    /// <summary>Нахлёст соседних отрезков, в градусах: без него стыки светились бы щелью фона.</summary>
    private const double Overlap = 0.5;

    private bool _dragging;
    private HsvColor _start;
    private IPointer? _pointer;

    static AxColorWheel()
    {
        FocusableProperty.OverrideDefaultValue<AxColorWheel>(true);
        AffectsRender<AxColorWheel>(
            HsvProperty, RingThicknessProperty, ThumbBrushProperty, ThumbOutlineBrushProperty, ThumbThicknessProperty,
            ThumbOutlineThicknessProperty);
        HsvProperty.Changed.AddClassHandler<AxColorWheel>((wheel, change) => wheel.Announce(change.GetOldValue<HsvColor>()));
    }

    /// <inheritdoc cref="HsvProperty"/>
    public HsvColor Hsv
    {
        get => GetValue(HsvProperty);
        set => SetValue(HsvProperty, value);
    }

    /// <inheritdoc cref="RingThicknessProperty"/>
    public double RingThickness
    {
        get => GetValue(RingThicknessProperty);
        set => SetValue(RingThicknessProperty, value);
    }

    /// <inheritdoc cref="ThumbBrushProperty"/>
    public IBrush? ThumbBrush
    {
        get => GetValue(ThumbBrushProperty);
        set => SetValue(ThumbBrushProperty, value);
    }

    /// <inheritdoc cref="ThumbOutlineBrushProperty"/>
    public IBrush? ThumbOutlineBrush
    {
        get => GetValue(ThumbOutlineBrushProperty);
        set => SetValue(ThumbOutlineBrushProperty, value);
    }

    /// <inheritdoc cref="ThumbThicknessProperty"/>
    public double ThumbThickness
    {
        get => GetValue(ThumbThicknessProperty);
        set => SetValue(ThumbThicknessProperty, value);
    }

    /// <inheritdoc cref="ThumbOutlineThicknessProperty"/>
    public double ThumbOutlineThickness
    {
        get => GetValue(ThumbOutlineThicknessProperty);
        set => SetValue(ThumbOutlineThicknessProperty, value);
    }

    /// <summary>Оттенок, 0…360°.</summary>
    public double Hue => Hsv.H;

    /// <summary>Идёт протяжка мышью.</summary>
    public bool IsDragging => _dragging;

    /// <summary>Оттенок в точке: угол от правого края против часовой стрелки.</summary>
    /// <param name="point">Точка в координатах кольца.</param>
    public double HueAt(Point point)
    {
        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        var angle = Math.Atan2(center.Y - point.Y, point.X - center.X) * 180 / Math.PI;

        return angle < 0 ? angle + 360 : angle;
    }

    /// <summary>Точка кольца для оттенка — на середине его толщины.</summary>
    /// <param name="hue">Оттенок, 0…360°.</param>
    public Point PointAt(double hue)
    {
        var radius = Middle();
        var angle = hue * Math.PI / 180;

        return new Point(Bounds.Width / 2 + radius * Math.Cos(angle), Bounds.Height / 2 - radius * Math.Sin(angle));
    }

    /// <inheritdoc/>
    public bool HitTest(Point point)
    {
        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        var distance = Math.Sqrt(Math.Pow(point.X - center.X, 2) + Math.Pow(point.Y - center.Y, 2));
        var outer = Math.Min(Bounds.Width, Bounds.Height) / 2;

        return distance <= outer && distance >= outer - RingThickness;
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        base.Render(context);

        var outer = Math.Min(Bounds.Width, Bounds.Height) / 2;
        var thickness = Math.Min(RingThickness, outer);

        if (outer <= 0 || thickness <= 0)
            return;

        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        var inner = outer - thickness;

        // Отрезки чуть внахлёст: стыки соседних не светятся щелью фона.
        for (var from = 0d; from < 360; from += Segment)
        {
            var hue = from + Segment / 2;
            var geometry = new StreamGeometry();

            using (var figure = geometry.Open())
            {
                var start = from * Math.PI / 180;
                var end = (from + Segment + Overlap) * Math.PI / 180;

                figure.BeginFigure(Polar(center, outer, start), true);
                figure.LineTo(Polar(center, outer, end));
                figure.LineTo(Polar(center, inner, end));
                figure.LineTo(Polar(center, inner, start));
                figure.EndFigure(true);
            }

            context.DrawGeometry(new ImmutableSolidColorBrush(AxColorSlider.PureHue(hue)), null, geometry);
        }

        // Ручка — кольцо поперёк полосы оттенков: тёмная кайма снаружи, светлое кольцо внутри, как у ручки спектра, —
        // видна на любом оттенке. Перо рисует по середине своей толщины, и радиусы отступают на её половину.
        var thumb = PointAt(Hsv.H);
        var radius = thickness / 2;
        var rim = Math.Max(ThumbOutlineThickness, 0);
        var ring = Math.Max(ThumbThickness, 0);

        if (ThumbOutlineBrush is { } outline && rim > 0)
        {
            var at = radius - rim / 2;

            context.DrawEllipse(null, new ImmutablePen(outline.ToImmutable(), rim), thumb, at, at);
        }

        if (ThumbBrush is { } brush && ring > 0)
        {
            var at = radius - rim - ring / 2;

            context.DrawEllipse(null, new ImmutablePen(brush.ToImmutable(), ring), thumb, at, at);
        }
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnPointerPressed(e);

        var point = e.GetCurrentPoint(this);

        if (!point.Properties.IsLeftButtonPressed || !HitTest(point.Position))
            return;

        _dragging = true;
        _start = Hsv;
        _pointer = e.Pointer;
        PseudoClasses.Set(":pressed", true);
        e.Pointer.Capture(this);
        Focus(NavigationMethod.Pointer);
        MoveTo(point.Position);
        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnPointerMoved(e);

        if (_dragging)
            MoveTo(e.GetPosition(this));
    }

    /// <inheritdoc/>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnPointerReleased(e);

        if (_dragging)
        {
            Finish(commit: true);
            e.Handled = true;
        }
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

        var step = e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? 10 : 1;
        var handled = e.Key switch
        {
            Key.Escape when _dragging => Finish(commit: false),
            Key.Left or Key.Down => Commit(Hsv.H - step),
            Key.Right or Key.Up => Commit(Hsv.H + step),
            Key.Home => Commit(0),
            Key.End => Commit(359),
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
    protected override AutomationPeer OnCreateAutomationPeer() => new WheelPeer(this);

    private static Point Polar(Point center, double radius, double angle) =>
        new(center.X + radius * Math.Cos(angle), center.Y - radius * Math.Sin(angle));

    private double Middle() => Math.Min(Bounds.Width, Bounds.Height) / 2 - RingThickness / 2;

    /// <summary>Ставит оттенок по точке и показывает его на ходу.</summary>
    private void MoveTo(Point point)
    {
        var next = With(HueAt(point));

        if (next.Equals(Hsv))
            return;

        SetCurrentValue(HsvProperty, next);
        RaiseEvent(new AxColorEventArgs(AxColorPicker.ColorChangingEvent, Hsv.ToRgb()));
    }

    /// <summary>Кончает протяжку: одна фиксация — или возврат к оттенку начала — и мышь отпущена.</summary>
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

    /// <summary>Ставит оттенок и фиксирует его: шаг стрелкой, край, значение от диктора. Круг замкнут.</summary>
    private bool Commit(double hue)
    {
        if (_dragging)
            return true;

        var next = With(hue);

        if (next.Equals(Hsv))
            return true;

        SetCurrentValue(HsvProperty, next);
        RaiseEvent(new AxColorEventArgs(AxColorPicker.ColorCommittedEvent, Hsv.ToRgb()));

        return true;
    }

    /// <summary>Модель с другим оттенком: круг замкнут, и 360 — это снова ноль.</summary>
    private HsvColor With(double hue)
    {
        var turned = hue % 360;

        if (turned < 0)
            turned += 360;

        var hsv = Hsv;

        return new HsvColor(hsv.A, turned, hsv.S, hsv.V);
    }

    /// <summary>Говорит диктору, что оттенок сдвинулся.</summary>
    private void Announce(HsvColor previous)
    {
        if (ControlAutomationPeer.FromElement(this) is { } peer && !previous.H.Equals(Hsv.H))
            peer.RaisePropertyChangedEvent(RangeValuePatternIdentifiers.ValueProperty, previous.H, Hsv.H);
    }

    /// <summary>Диктору кольцо — диапазон оттенка в градусах.</summary>
    private sealed class WheelPeer(AxColorWheel owner) : ControlAutomationPeer(owner), IRangeValueProvider
    {
        private AxColorWheel Wheel => (AxColorWheel)Owner;

        public bool IsReadOnly => !Wheel.IsEffectivelyEnabled;

        public double Minimum => 0;

        public double Maximum => 360;

        public double Value => Wheel.Hsv.H;

        public double SmallChange => 1;

        public double LargeChange => 10;

        public void SetValue(double value) => Wheel.Commit(value);

        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Slider;

        protected override string? GetNameCore() =>
            base.GetNameCore() is { Length: > 0 } name ? name : nameof(Hue).ToLower(CultureInfo.InvariantCulture);
    }
}
