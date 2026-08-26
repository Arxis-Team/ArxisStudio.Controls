using Avalonia;
using Avalonia.Controls.Primitives;

namespace ArxisStudio.Controls;

/// <summary>
/// Главный тулбар: три слота — слева, по центру, справа. Живёт внутри
/// <see cref="AxTitleBar"/>, поэтому фона и рамки не несёт: хром окна
/// рисует заголовок.
/// </summary>
public class AxToolBar : TemplatedControl
{
    /// <summary>Содержимое слева: логотип, имя проекта, меню.</summary>
    public static readonly StyledProperty<object?> LeftContentProperty =
        AvaloniaProperty.Register<AxToolBar, object?>(nameof(LeftContent));

    /// <summary>Содержимое по центру: виджет запуска.</summary>
    public static readonly StyledProperty<object?> CenterContentProperty =
        AvaloniaProperty.Register<AxToolBar, object?>(nameof(CenterContent));

    /// <summary>Содержимое справа: поиск, настройки, аватар.</summary>
    public static readonly StyledProperty<object?> RightContentProperty =
        AvaloniaProperty.Register<AxToolBar, object?>(nameof(RightContent));

    /// <inheritdoc cref="LeftContentProperty"/>
    public object? LeftContent
    {
        get => GetValue(LeftContentProperty);
        set => SetValue(LeftContentProperty, value);
    }

    /// <inheritdoc cref="CenterContentProperty"/>
    public object? CenterContent
    {
        get => GetValue(CenterContentProperty);
        set => SetValue(CenterContentProperty, value);
    }

    /// <inheritdoc cref="RightContentProperty"/>
    public object? RightContent
    {
        get => GetValue(RightContentProperty);
        set => SetValue(RightContentProperty, value);
    }
}
