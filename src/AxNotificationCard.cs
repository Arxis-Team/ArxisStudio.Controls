using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace ArxisStudio.Controls;

/// <summary>
/// Карточка уведомления: событие, которое исчезает, — в отличие от
/// <see cref="AxBanner"/>, который описывает состояние контекста и живёт в
/// панели. Значок берётся по <see cref="Severity"/>, действия — ссылки внизу.
/// </summary>
public class AxNotificationCard : ContentControl
{
    /// <summary>Смысл уведомления: значок и его цвет.</summary>
    public static readonly StyledProperty<AxBannerSeverity> SeverityProperty =
        AvaloniaProperty.Register<AxNotificationCard, AxBannerSeverity>(nameof(Severity));

    /// <summary>Заголовок уведомления.</summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<AxNotificationCard, string?>(nameof(Title));

    /// <summary>Действия внизу: ссылки «Запустить», «Показать журнал».</summary>
    public static readonly StyledProperty<object?> ActionsProperty =
        AvaloniaProperty.Register<AxNotificationCard, object?>(nameof(Actions));

    /// <summary>Крестик закрытия нажат.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> CloseRequestedEvent =
        RoutedEvent.Register<AxNotificationCard, RoutedEventArgs>(nameof(CloseRequested), RoutingStrategies.Bubble);

    /// <inheritdoc cref="SeverityProperty"/>
    public AxBannerSeverity Severity
    {
        get => GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
    }

    /// <inheritdoc cref="TitleProperty"/>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <inheritdoc cref="ActionsProperty"/>
    public object? Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }

    /// <inheritdoc cref="CloseRequestedEvent"/>
    public event EventHandler<RoutedEventArgs>? CloseRequested
    {
        add => AddHandler(CloseRequestedEvent, value);
        remove => RemoveHandler(CloseRequestedEvent, value);
    }

    /// <summary>Поднимает <see cref="CloseRequested"/>; зовёт крестик шаблона.</summary>
    public void RequestClose() => RaiseEvent(new RoutedEventArgs(CloseRequestedEvent));

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (e.NameScope.Find<Button>("PART_Close") is { } close)
            close.Click += (_, _) => RequestClose();
    }
}
