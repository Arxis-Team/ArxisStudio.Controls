using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace ArxisStudio.Controls;

/// <summary>
/// Диалог студии: окно без системной рамки на тени <c>AxShadowModal</c> —
/// заголовок с крестиком, содержимое и полоса кнопок. Основное действие
/// стоит крайним справа, отмена — левее, деструктивное — кнопка вида
/// <see cref="AxButtonAppearance.Danger"/>.
/// </summary>
/// <remarks>
/// Уйти, не ответив, можно двумя способами — крестиком и Esc, — и оба слушают
/// одно свойство <see cref="IsCloseVisible"/>. Снятое, оно означает
/// обязательный выбор: ни крестика, ни клавиши, решать придётся кнопкой.
/// Держать эти два пути на разных условиях значило бы завести чёрный ход мимо
/// принятого решения.
/// </remarks>
[TemplatePart("PART_Close", typeof(Button))]
[PseudoClasses(":alert")]
public class AxDialog : Window
{
    private Button? _close;

    /// <summary>Кнопки диалога; кладутся в полосу внизу справа.</summary>
    public static readonly StyledProperty<object?> ButtonsProperty =
        AvaloniaProperty.Register<AxDialog, object?>(nameof(Buttons));

    /// <summary>Содержимое слева от кнопок: флажок «Больше не спрашивать».</summary>
    public static readonly StyledProperty<object?> FooterContentProperty =
        AvaloniaProperty.Register<AxDialog, object?>(nameof(FooterContent));

    /// <summary>
    /// Значок слева от заголовка: диалог становится алертом.
    /// </summary>
    /// <remarks>
    /// Имя своё, не Icon: у окна такое свойство уже есть — значок в панели
    /// задач, и подменять его собой нельзя.
    /// </remarks>
    /// <remarks>
    /// Со значком шапка не нужна: заголовок и текст встают колонкой рядом с
    /// ним. Вместе с шапкой уходит и крестик — но это
    /// вёрстка, а не смысл: обязательность выбора объявляет
    /// <see cref="IsCloseVisible"/>, и алерт с обычным значением этого свойства
    /// Esc отпускает. Так и устроен вопрос «вы уверены?»: у него есть «Отмена»,
    /// и Esc делает ровно то же.
    /// </remarks>
    public static readonly StyledProperty<object?> AlertIconProperty =
        AvaloniaProperty.Register<AxDialog, object?>(nameof(AlertIcon));

    /// <summary>
    /// Из диалога можно уйти, не ответив: крестик в заголовке и Esc.
    /// </summary>
    /// <remarks>
    /// Имя осталось прежним — свойство и заводилось как «показывать крестик», —
    /// а значит оно с самого начала: есть ли у человека выход помимо кнопок.
    /// Esc слушает его же, потому что второй выход на других условиях был бы
    /// не вторым выходом, а дырой.
    /// </remarks>
    public static readonly StyledProperty<bool> IsCloseVisibleProperty =
        AvaloniaProperty.Register<AxDialog, bool>(nameof(IsCloseVisible), true);

    /// <summary>Создаёт диалог: без системной рамки, поверх владельца, по центру.</summary>
    public AxDialog()
    {
        WindowDecorations = WindowDecorations.None;
        // Тень лежит за краем карточки, поэтому окну нужен прозрачный фон.
        TransparencyLevelHint = [WindowTransparencyLevel.Transparent];
        Background = null;
        SizeToContent = SizeToContent.WidthAndHeight;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ShowInTaskbar = false;
        CanResize = false;
    }

    /// <inheritdoc cref="ButtonsProperty"/>
    public object? Buttons
    {
        get => GetValue(ButtonsProperty);
        set => SetValue(ButtonsProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        // Значок поднимает состояние, а не тема смотрит на свойство: по
        // состоянию тема переключает и шапку, и отбивку тела разом, а
        // селектора «свойство не пусто» в Avalonia нет.
        if (change.Property == AlertIconProperty)
            PseudoClasses.Set(":alert", change.NewValue is not null);
    }

    /// <inheritdoc cref="AlertIconProperty"/>
    public object? AlertIcon
    {
        get => GetValue(AlertIconProperty);
        set => SetValue(AlertIconProperty, value);
    }

    /// <inheritdoc cref="FooterContentProperty"/>
    public object? FooterContent
    {
        get => GetValue(FooterContentProperty);
        set => SetValue(FooterContentProperty, value);
    }

    /// <summary>
    /// Ключ стиля — свой.
    /// </summary>
    /// <remarks>
    /// Иначе диалог берёт шаблон базового <see cref="Window"/>: тот подменяет
    /// ключ собой, чтобы наследники окон не оставались без чрома. Нам это как
    /// раз мешает — шаблон диалога рисует карточку с шапкой и полосой кнопок.
    /// </remarks>
    protected override Type StyleKeyOverride => typeof(AxDialog);

    /// <inheritdoc cref="IsCloseVisibleProperty"/>
    public bool IsCloseVisible
    {
        get => GetValue(IsCloseVisibleProperty);
        set => SetValue(IsCloseVisibleProperty, value);
    }

    /// <summary>
    /// Esc закрывает диалог — тот, из которого вообще можно уйти не ответив.
    /// </summary>
    /// <remarks>
    /// Обещание это стояло в описании класса с первого дня и не выполнялось ни
    /// разу: обработчика клавиши здесь не было вовсе, и сказанное словами
    /// расходилось с тем, что делает код.
    /// </remarks>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape && IsCloseVisible)
        {
            e.Handled = true;

            Close();
        }

        base.OnKeyDown(e);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnApplyTemplate(e);

        // Шаблон переприменяется при смене темы, и подписка на прежний крестик
        // держала бы в памяти его дерево вместе с окном.
        if (_close is not null)
            _close.Click -= OnCloseClick;

        _close = e.NameScope.Find<Button>("PART_Close");

        if (_close is not null)
            _close.Click += OnCloseClick;
    }

    private void OnCloseClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close();
}
