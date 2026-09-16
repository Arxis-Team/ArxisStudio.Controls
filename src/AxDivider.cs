using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace ArxisStudio.Controls;

/// <summary>
/// Разделительная линия. Ориентация задаётся свойством <see cref="Orientation"/>,
/// цвет — <see cref="Fill"/>.
/// </summary>
/// <remarks>
/// Линия рисует себя сама, без шаблона: разделителей в окне десятки, и у каждого
/// шаблон был бы одной рамкой с фоном — лишний элемент дерева на линию.
/// <para>
/// Толщину она тоже считает сама — один пиксель устройства при любом масштабе. Прибитый пиксель
/// раскладки при 150 % и 175 % округляется до двух, и линия, задуманная волосяной, становится
/// вдвое толще ровно там, где всё остальное остаётся на месте: рядом с ней это видно как рамку.
/// </para>
/// </remarks>
public class AxDivider : Control
{
    /// <summary>Ориентация линии.</summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<AxDivider, Orientation>(nameof(Orientation), Orientation.Horizontal);

    /// <summary>Цвет линии.</summary>
    public static readonly StyledProperty<IBrush?> FillProperty =
        AvaloniaProperty.Register<AxDivider, IBrush?>(nameof(Fill));

    private TopLevel? _root;

    static AxDivider()
    {
        AffectsRender<AxDivider>(FillProperty);
        AffectsMeasure<AxDivider>(OrientationProperty);
    }

    /// <inheritdoc cref="OrientationProperty"/>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <inheritdoc cref="FillProperty"/>
    public IBrush? Fill
    {
        get => GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    /// <summary>
    /// Один пиксель устройства в единицах раскладки при нынешнем масштабе окна.
    /// </summary>
    /// <param name="control">Элемент, чей масштаб спрашивают.</param>
    /// <remarks>
    /// При 150 % это 0,667: раскладка округляет такую длину ровно к одному пикселю, а прибитая
    /// единица — к двум. Этой же мерой меряет свои границы движок докинга.
    /// </remarks>
    public static double Hairline(Layoutable control)
    {
        ArgumentNullException.ThrowIfNull(control);

        return 1 / LayoutHelper.GetLayoutScale(control);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        var hairline = Hairline(this);

        return Orientation == Orientation.Horizontal ? new Size(0, hairline) : new Size(hairline, 0);
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _root = TopLevel.GetTopLevel(this);

        if (_root is not null)
            _root.ScalingChanged += OnScalingChanged;

        InvalidateMeasure();
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_root is not null)
            _root.ScalingChanged -= OnScalingChanged;

        _root = null;

        base.OnDetachedFromVisualTree(e);
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (Fill is { } fill)
            context.FillRectangle(fill, new Rect(Bounds.Size));
    }

    // Окно переехало на экран с другим масштабом: пиксель устройства стал другой длины.
    private void OnScalingChanged(object? sender, EventArgs e) => InvalidateMeasure();
}
