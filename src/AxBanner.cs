using Avalonia;
using Avalonia.Controls;

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
/// положить действия через <see cref="Actions"/>.
/// </summary>
public class AxBanner : ContentControl
{
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
}
