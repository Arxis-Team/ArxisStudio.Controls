using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace ArxisStudio.Controls;

/// <summary>
/// Вкладка документа или панели: значок, заголовок и кнопка закрытия. Активная
/// вкладка подчёркнута акцентной полосой снизу, как в IntelliJ.
/// </summary>
/// <remarks>
/// <para>
/// Вид вкладки панели приходит от полосы псевдоклассом <c>:tool-window</c> — см.
/// <see cref="AxTabStrip.Kind"/>.
/// </para>
/// <para>
/// Выбранная вкладка говорит и о фокусе: <c>:selection-active</c>, пока клавиатура внутри её
/// области — панели, в шапке которой она стоит, — и ничего, когда клавиатура ушла к соседке. Так
/// же устроена строка списка (<see cref="AxListBoxItem"/>): признак приходит от области выделения,
/// а не от селектора с предком, и потому не гаснет, пока открыто меню, позванное из панели.
/// </para>
/// </remarks>
[TemplatePart("PART_Close", typeof(Control))]
[PseudoClasses(":tool-window", ":selection-active", ":solo")]
public class AxTabItem : ListBoxItem
{
    static AxTabItem() =>
        AxSelectionScope.IsActiveProperty.Changed.AddClassHandler<AxTabItem>(
            (tab, change) => tab.PseudoClasses.Set(":selection-active", change.GetNewValue<bool>()));

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

    /// <summary>Отмечает вкладку вкладкой панели; зовёт полоса.</summary>
    internal void MarkToolWindow(bool on) => PseudoClasses.Set(":tool-window", on);

    /// <summary>
    /// Отмечает вкладку единственной в полосе; зовёт полоса.
    /// </summary>
    /// <remarks>
    /// Одинокой вкладке полоса выбора не нужна: выбирать не из чего, и линия под ней говорит о
    /// том, чего человек и так не спрашивает. Считает это сама полоса — вкладка своего ряда не
    /// видит.
    /// </remarks>
    internal void MarkSolo(bool on) => PseudoClasses.Set(":solo", on);

    /// <inheritdoc/>
    /// <remarks>
    /// Не шире места своей полосы — почему, сказано у <see cref="AxTabStrip"/>.
    /// Вкладка вне полосы мерится как прежде.
    /// </remarks>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (this.FindAncestorOfType<AxTabStrip>() is { } strip && strip.Room < availableSize.Width)
            availableSize = availableSize.WithWidth(strip.Room);

        return base.MeasureOverride(availableSize);
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

    /// <inheritdoc/>
    /// <remarks>
    /// Вкладка устроена строкой списка, и экранный диктор читал её «элементом списка, 2 из 5».
    /// Роль у неё своя — вкладка, — а выбор остаётся от строки: вкладку выбирают, как строку.
    /// </remarks>
    protected override AutomationPeer OnCreateAutomationPeer() => new TabPeer(this);

    /// <summary>Вкладка для экранного диктора.</summary>
    private sealed class TabPeer(AxTabItem owner) : ListItemAutomationPeer(owner)
    {
        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.TabItem;
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
