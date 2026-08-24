using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace ArxisStudio.Controls;

/// <summary>
/// Контурная иконка студии: SVG-путь в системе координат 16×16, обведённый
/// <see cref="TemplatedControl.Foreground"/>. Пути лежат в наборе студии; плагин
/// может передать свой.
/// </summary>
public class AxIcon : TemplatedControl
{
    /// <summary>Геометрия пути в координатах 16×16.</summary>
    public static readonly StyledProperty<Geometry?> DataProperty =
        AvaloniaProperty.Register<AxIcon, Geometry?>(nameof(Data));

    /// <summary>Толщина обводки; по умолчанию 1.2 — как в дизайн-спецификации.</summary>
    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<AxIcon, double>(nameof(StrokeThickness), 1.2);

    /// <summary>Заливать путь вместо обводки (play, stop, пауза).</summary>
    public static readonly StyledProperty<bool> IsFilledProperty =
        AvaloniaProperty.Register<AxIcon, bool>(nameof(IsFilled));

    /// <inheritdoc cref="DataProperty"/>
    public Geometry? Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    /// <inheritdoc cref="StrokeThicknessProperty"/>
    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    /// <inheritdoc cref="IsFilledProperty"/>
    public bool IsFilled
    {
        get => GetValue(IsFilledProperty);
        set => SetValue(IsFilledProperty, value);
    }
}
