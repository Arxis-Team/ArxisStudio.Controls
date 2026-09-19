using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
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

    /// <summary>
    /// Ход границы кончился: тяга мышью отпущена или стрелка сделала шаг вдоль оси.
    /// </summary>
    /// <remarks>
    /// Событие для того, кто записывает положение границы. Тяговых событий разделителя,
    /// перенесённого из WPF, для этого мало: <c>DragCompleted</c> приходит только от мыши, и
    /// хозяин, записывавший границу по нему, хода стрелкой не видел — граница возвращалась на место
    /// при первой же перекладке. Придумывать стрелке конец тяги нельзя: тот, кто ждёт пары начала и
    /// конца или смещения указателя, получил бы конец без начала и нулевое смещение.
    /// <para>
    /// Ход — не обязательно сдвиг: граница, упёршаяся в предел, стоит на месте, и хозяин, записав
    /// её положение, запишет прежнее. Так и у мыши: щелчок без тяги тоже кончает ход.
    /// </para>
    /// </remarks>
    public static readonly RoutedEvent<RoutedEventArgs> MovedEvent =
        RoutedEvent.Register<AxSplitter, RoutedEventArgs>(nameof(Moved), RoutingStrategies.Bubble);

    /// <summary>Заводит разделитель строк — стороной по умолчанию.</summary>
    public AxSplitter() => ResizeDirection = Direction(Orientation);

    /// <inheritdoc cref="MovedEvent"/>
    public event EventHandler<RoutedEventArgs>? Moved
    {
        add => AddHandler(MovedEvent, value);
        remove => RemoveHandler(MovedEvent, value);
    }

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

    /// <inheritdoc/>
    protected override void OnDragCompleted(VectorEventArgs e)
    {
        base.OnDragCompleted(e);

        RaiseEvent(new RoutedEventArgs(MovedEvent));
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Базовый контрол отмечает обработанной любую стрелку, и поперечную тоже, хотя поперёк своей
    /// оси граница не ходит. Ход объявляется только о стрелке вдоль оси: иначе хозяин, записывающий
    /// границу, переписывал бы её на каждое нажатие мимо — а переписанная, она уже не совпадает с
    /// записанной на долю пикселя округления.
    /// </remarks>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnKeyDown(e);

        if (e.Handled && Along(e.Key))
            RaiseEvent(new RoutedEventArgs(MovedEvent));
    }

    /// <summary>Стрелка идёт вдоль оси, по которой граница ходит.</summary>
    private bool Along(Key key) =>
        Orientation == Orientation.Horizontal ? key is Key.Up or Key.Down : key is Key.Left or Key.Right;

    private static GridResizeDirection Direction(Orientation orientation) =>
        orientation == Orientation.Horizontal ? GridResizeDirection.Rows : GridResizeDirection.Columns;
}
