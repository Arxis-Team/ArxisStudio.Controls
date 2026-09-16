using Avalonia;
using System.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace ArxisStudio.Controls;

/// <summary>
/// Попап поиска: строка ввода, список результатов и подвал с подсказками
/// клавиш. Сам контрол — карточка; открыть её попапом и наполнить
/// результатами — забота приложения: у каждого поиска свои источники.
/// </summary>
/// <remarks>
/// Клавиатуру карточка разбирает сама: стрелки водят выбор по кругу, Enter говорит «это», Esc —
/// «передумал». Прежде это лежало у палитры команд, единственного её потребителя, и второй поиск
/// в студии начинался бы с переписывания тех же сорока строк — а поиск без клавиатуры не поиск:
/// набирают в нём всегда, и рука на стрелках, а не на мыши.
/// <para>
/// Что делать с выбранным, карточка не знает: <see cref="Accepted"/> и <see cref="Cancelled"/> —
/// события, и хозяин на них закрывает попап, выполняет команду или открывает файл.
/// </para>
/// </remarks>
[TemplatePart("PART_List", typeof(ListBox))]
[TemplatePart("PART_Search", typeof(AxTextBox))]
public class AxQuickSearch : TemplatedControl
{
    private ListBox? _list;

    /// <summary>Выбранное принято: Enter или щелчок по строке.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> AcceptedEvent =
        RoutedEvent.Register<AxQuickSearch, RoutedEventArgs>(nameof(Accepted), RoutingStrategies.Bubble);

    /// <summary>Поиск брошен: Esc.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> CancelledEvent =
        RoutedEvent.Register<AxQuickSearch, RoutedEventArgs>(nameof(Cancelled), RoutingStrategies.Bubble);

    /// <inheritdoc cref="AcceptedEvent"/>
    public event EventHandler<RoutedEventArgs>? Accepted
    {
        add => AddHandler(AcceptedEvent, value);
        remove => RemoveHandler(AcceptedEvent, value);
    }

    /// <inheritdoc cref="CancelledEvent"/>
    public event EventHandler<RoutedEventArgs>? Cancelled
    {
        add => AddHandler(CancelledEvent, value);
        remove => RemoveHandler(CancelledEvent, value);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnApplyTemplate(e);

        if (_list is not null)
            _list.RemoveHandler(InputElement.PointerReleasedEvent, OnListReleased);

        _list = e.NameScope.Find<ListBox>("PART_List");

        if (_list is not null)
        {
            // Список внутри попапа — не своя область: клавиатура стоит в строке запроса, и своей
            // областью список гас бы всегда. Значение он берёт у попапа по наследству.
            AxSelectionScope.Release(_list);

            // Щелчок по строке — это выбор и «да» сразу: искали, нашли, нажали. Ждать после
            // щелчка ещё и Enter человека заставляет только список, в котором выбирают, а не
            // открывают.
            _list.AddHandler(InputElement.PointerReleasedEvent, OnListReleased, handledEventsToo: true);
        }
    }

    /// <summary>Заводит попап и объявляет его областью выделения.</summary>
    /// <remarks>
    /// Фокус в попапе стоит в строке запроса, а выбранная строка живёт в списке под ней: со стилем
    /// по <c>:focus-within</c> список не загорался никогда, и выбор в палитре команд всегда
    /// выглядел погашенным. Область — весь попап целиком: и поле, и список внутри него.
    /// <para>
    /// Клавиши ловятся на пути вниз, до поля ввода: выбор в списке — дело карточки, и решение о
    /// стрелках и Enter принадлежит ей, что бы поле ни делало с ними завтра. Сегодня однострочное
    /// поле пропускает их дальше, и всплытия хватило бы тоже.
    /// </para>
    /// </remarks>
    public AxQuickSearch()
    {
        AxSelectionScope.Track(this);
        AddHandler(InputElement.KeyDownEvent, OnKey, RoutingStrategies.Tunnel);
    }

    /// <summary>Набранный запрос.</summary>
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<AxQuickSearch, string?>(nameof(Text), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Подсказка в пустой строке запроса.</summary>
    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<AxQuickSearch, string?>(nameof(PlaceholderText));

    /// <summary>Результаты поиска.</summary>
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<AxQuickSearch, IEnumerable?>(nameof(ItemsSource));

    /// <summary>Шаблон строки результата.</summary>
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<AxQuickSearch, IDataTemplate?>(nameof(ItemTemplate));

    /// <summary>Выбранный результат.</summary>
    public static readonly StyledProperty<object?> SelectedItemProperty =
        AvaloniaProperty.Register<AxQuickSearch, object?>(nameof(SelectedItem), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Подсказки клавиш в подвале: «↑↓ выбор · ↵ открыть».</summary>
    public static readonly StyledProperty<string?> HintsProperty =
        AvaloniaProperty.Register<AxQuickSearch, string?>(nameof(Hints));

    /// <inheritdoc cref="TextProperty"/>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <inheritdoc cref="PlaceholderTextProperty"/>
    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    /// <inheritdoc cref="ItemsSourceProperty"/>
    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <inheritdoc cref="ItemTemplateProperty"/>
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <inheritdoc cref="SelectedItemProperty"/>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <inheritdoc cref="HintsProperty"/>
    public string? Hints
    {
        get => GetValue(HintsProperty);
        set => SetValue(HintsProperty, value);
    }

    private void OnKey(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Down:
                e.Handled = Step(1);
                break;

            case Key.Up:
                e.Handled = Step(-1);
                break;

            case Key.Enter:
                e.Handled = true;
                RaiseEvent(new RoutedEventArgs(AcceptedEvent));
                break;

            case Key.Escape:
                e.Handled = true;
                RaiseEvent(new RoutedEventArgs(CancelledEvent));
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Двигает выбор по кругу и показывает выбранное.
    /// </summary>
    /// <param name="by">Шаг: вниз или вверх.</param>
    /// <returns>Было ли что двигать.</returns>
    /// <remarks>
    /// По кругу, а не до упора: список короткий, и человек, дошедший стрелкой до низа, ждёт
    /// начала, а не тишины. Выбранное подтягивается в видимую часть — стрелкой можно уйти за край
    /// списка, и выбор, которого не видно, ничем не отличается от его отсутствия.
    /// </remarks>
    private bool Step(int by)
    {
        if (_list is null || _list.ItemCount == 0)
            return false;

        var at = _list.SelectedIndex;
        var count = _list.ItemCount;

        _list.SelectedIndex = at < 0
            ? (by > 0 ? 0 : count - 1)
            : (((at + by) % count) + count) % count;

        _list.ScrollIntoView(_list.SelectedIndex);

        return true;
    }

    private void OnListReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton != MouseButton.Left)
            return;

        // Щелчок мимо строк — по пустоте под списком — не выбор: выбранное там прежнее, и
        // принимать его человек не просил.
        if (e.Source is Visual source && source.FindAncestorOfType<ListBoxItem>(includeSelf: true) is not null)
            RaiseEvent(new RoutedEventArgs(AcceptedEvent));
    }
}
