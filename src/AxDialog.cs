using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace ArxisStudio.Controls;

/// <summary>
/// Диалог студии: окно без системной рамки на тени <c>AxAbShadow</c> —
/// заголовок с крестиком, содержимое и полоса кнопок. Основное действие
/// стоит крайним справа, отмена — левее, деструктивное несёт класс
/// <c>danger</c>. Esc закрывает диалог; крестик прячется свойством
/// <see cref="IsCloseVisible"/> — например, у обязательного выбора.
/// </summary>
public class AxDialog : Window
{
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
    /// ним, как в карточке «Диалоги». Крестика у алерта тоже нет — уйти из
    /// него можно только кнопкой, и это часть смысла: решение обязательно.
    /// </remarks>
    public static readonly StyledProperty<object?> AlertIconProperty =
        AvaloniaProperty.Register<AxDialog, object?>(nameof(AlertIcon));

    /// <summary>Показывать крестик закрытия в заголовке.</summary>
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

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (e.NameScope.Find<Button>("PART_Close") is { } close)
            close.Click += (_, _) => Close();
    }
}
