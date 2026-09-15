using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace ArxisStudio.Controls;

/// <summary>
/// Разделительная линия. Ориентация задаётся свойством <see cref="Orientation"/>,
/// цвет — <see cref="Fill"/>.
/// </summary>
/// <remarks>
/// Линия рисует себя сама, без шаблона: разделителей в окне десятки, и у каждого
/// шаблон был бы одной рамкой с фоном — лишний элемент дерева на линию.
/// </remarks>
public class AxDivider : Control
{
    /// <summary>Ориентация линии.</summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<AxDivider, Orientation>(nameof(Orientation), Orientation.Horizontal);

    /// <summary>Цвет линии.</summary>
    public static readonly StyledProperty<IBrush?> FillProperty =
        AvaloniaProperty.Register<AxDivider, IBrush?>(nameof(Fill));

    static AxDivider() => AffectsRender<AxDivider>(FillProperty);

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

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (Fill is { } fill)
            context.FillRectangle(fill, new Rect(Bounds.Size));
    }
}
