using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace ArxisStudio.Controls;

/// <summary>
/// Разделительная линия толщиной в пиксель. Ориентация задаётся
/// свойством <see cref="Orientation"/>.
/// </summary>
public class AxDivider : TemplatedControl
{
    /// <summary>Ориентация линии.</summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<AxDivider, Orientation>(nameof(Orientation), Orientation.Horizontal);

    /// <inheritdoc cref="OrientationProperty"/>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
}
