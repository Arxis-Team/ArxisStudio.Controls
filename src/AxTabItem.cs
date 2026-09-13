using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace ArxisStudio.Controls;

/// <summary>
/// Вкладка документа: значок, заголовок и кнопка закрытия. Активная вкладка
/// подчёркнута акцентной полосой снизу, как в IntelliJ.
/// </summary>
public class AxTabItem : ListBoxItem
{
    /// <summary>
    /// Человек попросил закрыть вкладку.
    /// </summary>
    /// <remarks>
    /// Именно попросил, а не закрыл: закрывает хозяин вкладки. У документа
    /// могут быть несохранённые правки, и спросить о них — не дело контрола.
    /// </remarks>
    public static readonly RoutedEvent<RoutedEventArgs> CloseRequestedEvent =
        RoutedEvent.Register<AxTabItem, RoutedEventArgs>(nameof(CloseRequested), RoutingStrategies.Bubble);

    /// <summary>Значок типа документа.</summary>
    public static readonly StyledProperty<Geometry?> IconProperty =
        AvaloniaProperty.Register<AxTabItem, Geometry?>(nameof(Icon));

    /// <summary>Цвет значка.</summary>
    public static readonly StyledProperty<IBrush?> IconBrushProperty =
        AvaloniaProperty.Register<AxTabItem, IBrush?>(nameof(IconBrush));

    /// <summary>Показывать кнопку закрытия.</summary>
    public static readonly StyledProperty<bool> IsClosableProperty =
        AvaloniaProperty.Register<AxTabItem, bool>(nameof(IsClosable), true);

    /// <summary>В документе есть несохранённые правки.</summary>
    public static readonly StyledProperty<bool> IsModifiedProperty =
        AvaloniaProperty.Register<AxTabItem, bool>(nameof(IsModified));

    private Control? _close;

    /// <inheritdoc cref="CloseRequestedEvent"/>
    public event EventHandler<RoutedEventArgs>? CloseRequested
    {
        add => AddHandler(CloseRequestedEvent, value);
        remove => RemoveHandler(CloseRequestedEvent, value);
    }

    /// <inheritdoc cref="IconProperty"/>
    public Geometry? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <inheritdoc cref="IconBrushProperty"/>
    public IBrush? IconBrush
    {
        get => GetValue(IconBrushProperty);
        set => SetValue(IconBrushProperty, value);
    }

    /// <inheritdoc cref="IsClosableProperty"/>
    public bool IsClosable
    {
        get => GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    /// <inheritdoc cref="IsModifiedProperty"/>
    public bool IsModified
    {
        get => GetValue(IsModifiedProperty);
        set => SetValue(IsModifiedProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_close is not null)
        {
            _close.PointerPressed -= OnClosePressed;
            _close.PointerReleased -= OnCloseReleased;
        }

        _close = e.NameScope.Find<Control>("PART_Close");

        if (_close is not null)
        {
            _close.PointerPressed += OnClosePressed;
            _close.PointerReleased += OnCloseReleased;
        }
    }

    /// <summary>
    /// Delete закрывает вкладку, на которой стоит фокус.
    /// </summary>
    /// <remarks>
    /// Крестик ловит мышь площадкой шестнадцать на шестнадцать, и клавиатуре
    /// эта площадка не помогает никак: своим местом в обходе крестик не стал
    /// нарочно — в полосе из десяти вкладок это двадцать остановок вместо
    /// десяти, и половина из них ведёт к необратимому действию. Вместо этого
    /// закрывает клавиша, и закрывает ту вкладку, на которой человек стоит.
    /// <para>
    /// Delete, а не Ctrl+W: сочетание принадлежит окну, а не вкладке — оно
    /// закрывает показанный документ, где бы ни был фокус, и объявлять его
    /// здесь значило бы завести второго хозяина одному жесту.
    /// </para>
    /// </remarks>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Delete && IsClosable)
        {
            e.Handled = true;

            RaiseEvent(new RoutedEventArgs(CloseRequestedEvent));
        }

        base.OnKeyDown(e);
    }

    /// <summary>
    /// Нажатие на крестик дальше не идёт.
    /// </summary>
    /// <remarks>
    /// Иначе вкладка сперва станет выбранной и поедет за мышью, если хозяин
    /// умеет её перетаскивать, — а закроется уже потом.
    /// </remarks>
    private void OnClosePressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            e.Handled = true;
    }

    /// <summary>
    /// Просит закрыть — по отпусканию, а не по нажатию.
    /// </summary>
    /// <remarks>
    /// Так нажатый по ошибке крестик ещё можно отменить, уведя мышь в сторону:
    /// отпускание тогда придётся не на него. Закрытие необратимо, и права на
    /// эту секунду человека лишать незачем.
    /// </remarks>
    private void OnCloseReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton != MouseButton.Left)
            return;

        e.Handled = true;

        RaiseEvent(new RoutedEventArgs(CloseRequestedEvent));
    }
}
