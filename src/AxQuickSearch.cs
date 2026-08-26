using Avalonia;
using System.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;

namespace ArxisStudio.Controls;

/// <summary>
/// Попап поиска: строка ввода, список результатов и подвал с подсказками
/// клавиш. Сам контрол — карточка; открыть её попапом и наполнить
/// результатами — забота приложения: у каждого поиска свои источники.
/// </summary>
public class AxQuickSearch : TemplatedControl
{
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
}
