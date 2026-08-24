using Avalonia;
using Avalonia.Controls;

namespace ArxisStudio.Controls;

/// <summary>
/// Панель инструментов студии — то, из чего собран её интерфейс: иерархия,
/// палитра, инспектор, консоль, панель проекта. По Int UI это «tool window»:
/// шапка с заголовком и действиями плюс содержимое.
/// </summary>
/// <remarks>
/// Шапка бывает трёх видов, и все три задаются свойствами, а не разными
/// контролами: только заголовок; заголовок с линией-разделителем
/// (<see cref="ShowHeaderSeparator"/>); заголовок с вкладками
/// (<see cref="Tabs"/>) — так устроена нижняя панель IDE, где рядом живут
/// «Проект», «Консоль» и «Проблемы».
/// </remarks>
public class AxToolWindow : ContentControl
{
    /// <summary>Заголовок панели.</summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<AxToolWindow, string?>(nameof(Title));

    /// <summary>Вкладки в шапке; обычно <see cref="AxTabStrip"/> с классом <c>compact</c>.</summary>
    public static readonly StyledProperty<object?> TabsProperty =
        AvaloniaProperty.Register<AxToolWindow, object?>(nameof(Tabs));

    /// <summary>Действия справа в шапке: поиск, меню, свернуть.</summary>
    public static readonly StyledProperty<object?> ActionsProperty =
        AvaloniaProperty.Register<AxToolWindow, object?>(nameof(Actions));

    /// <summary>Рисовать линию под шапкой.</summary>
    public static readonly StyledProperty<bool> ShowHeaderSeparatorProperty =
        AvaloniaProperty.Register<AxToolWindow, bool>(nameof(ShowHeaderSeparator), true);

    /// <summary>Показывать шапку. Панель без шапки — просто область содержимого.</summary>
    public static readonly StyledProperty<bool> ShowHeaderProperty =
        AvaloniaProperty.Register<AxToolWindow, bool>(nameof(ShowHeader), true);

    /// <inheritdoc cref="TitleProperty"/>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <inheritdoc cref="TabsProperty"/>
    public object? Tabs
    {
        get => GetValue(TabsProperty);
        set => SetValue(TabsProperty, value);
    }

    /// <inheritdoc cref="ActionsProperty"/>
    public object? Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }

    /// <inheritdoc cref="ShowHeaderSeparatorProperty"/>
    public bool ShowHeaderSeparator
    {
        get => GetValue(ShowHeaderSeparatorProperty);
        set => SetValue(ShowHeaderSeparatorProperty, value);
    }

    /// <inheritdoc cref="ShowHeaderProperty"/>
    public bool ShowHeader
    {
        get => GetValue(ShowHeaderProperty);
        set => SetValue(ShowHeaderProperty, value);
    }
}
