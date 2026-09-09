using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace ArxisStudio.Controls;

/// <summary>
/// Граница между двумя областями, за которую можно взяться мышью.
/// </summary>
/// <remarks>
/// Линия в пиксель, как у <see cref="AxDivider"/>, но с полосой захвата вокруг:
/// однопиксельная линия — то, что человек видит, а попасть в неё курсором он не
/// обязан. Ставится в <c>Grid</c> между двумя долями и тянет их.
/// <para>
/// Сторону задаёт <see cref="Orientation"/>, и она же значит то же самое, что у
/// разделителя: <c>Horizontal</c> — линия поперёк, тянет строки; <c>Vertical</c>
/// — линия вдоль, тянет столбцы. Направление изменения размера контрол
/// выставляет по ней сам, и «угадывания» по выравниванию, которое умеет базовый
/// класс, здесь нет намеренно: угаданное направление меняется от того, растянут
/// ли контрол, — а это свойство соседей, и однажды оно меняется само.
/// </para>
/// </remarks>
public class AxSplitter : GridSplitter
{
    /// <summary>Сторона разделителя.</summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<AxSplitter, Orientation>(nameof(Orientation), Orientation.Horizontal);

    /// <summary>Заводит разделитель строк — стороной по умолчанию.</summary>
    public AxSplitter() => ResizeDirection = Direction(Orientation);

    /// <inheritdoc cref="OrientationProperty"/>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        ArgumentNullException.ThrowIfNull(change);

        base.OnPropertyChanged(change);

        if (change.Property == OrientationProperty)
            ResizeDirection = Direction(change.GetNewValue<Orientation>());
    }

    private static GridResizeDirection Direction(Orientation orientation) =>
        orientation == Orientation.Horizontal ? GridResizeDirection.Rows : GridResizeDirection.Columns;
}
