using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ArxisStudio.Controls;

/// <summary>Смысл сообщения баннера: он же определяет цвет и значок.</summary>
public enum AxBannerSeverity
{
    /// <summary>Сообщение к сведению.</summary>
    Information,

    /// <summary>Операция удалась.</summary>
    Success,

    /// <summary>Что-то требует внимания, но работать можно.</summary>
    Warning,

    /// <summary>Ошибка: действие не выполнено.</summary>
    Error,
}

/// <summary>
/// Полоса сообщения над содержимым: результат операции, предупреждение,
/// ошибка. Значок и цвет выбираются по <see cref="Severity"/>, справа можно
/// положить действия через <see cref="Actions"/>, а закрывается баннер
/// крестиком.
/// </summary>
/// <remarks>
/// Закрыть баннер можно всегда: сообщение о состоянии перестаёт быть нужным
/// раньше, чем состояние меняется, и человек вправе убрать его с глаз. Кто
/// показал баннер, узнаёт об этом из <see cref="Closed"/> — чтобы не показать
/// то же самое снова.
/// </remarks>
public class AxBanner : ContentControl
{
    /// <summary>Имя кнопки закрытия в теме.</summary>
    private const string ClosePart = "PART_Close";

    /// <summary>Баннер закрыли крестиком.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> ClosedEvent =
        RoutedEvent.Register<AxBanner, RoutedEventArgs>(nameof(Closed), RoutingStrategies.Bubble);

    /// <summary>
    /// Слушает нажатие по маршруту события: кнопка живёт в шаблоне, но её
    /// нажатие всплывает до баннера в любом случае.
    /// </summary>
    public AxBanner() =>
        AddHandler(Button.ClickEvent, OnClick, RoutingStrategies.Bubble);

    /// <inheritdoc cref="ClosedEvent"/>
    public event EventHandler<RoutedEventArgs>? Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    /// <summary>Смысл сообщения.</summary>
    public static readonly StyledProperty<AxBannerSeverity> SeverityProperty =
        AvaloniaProperty.Register<AxBanner, AxBannerSeverity>(nameof(Severity));

    /// <summary>Действия справа от текста: кнопки или ссылки.</summary>
    public static readonly StyledProperty<object?> ActionsProperty =
        AvaloniaProperty.Register<AxBanner, object?>(nameof(Actions));

    /// <inheritdoc cref="SeverityProperty"/>
    public AxBannerSeverity Severity
    {
        get => GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
    }

    /// <inheritdoc cref="ActionsProperty"/>
    public object? Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }

    private void OnClick(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not Button { Name: ClosePart })
        {
            return;
        }

        // Баннер убирает себя сам: тот, кто его показал, узнает об этом
        // событием и решит, показывать ли снова.
        IsVisible = false;
        e.Handled = true;

        RaiseEvent(new RoutedEventArgs(ClosedEvent));
    }
}
