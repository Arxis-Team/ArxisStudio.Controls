using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace ArxisStudio.Controls;

/// <summary>
/// Вкладка документа: значок, заголовок и кнопка закрытия. Активная вкладка
/// подчёркнута акцентной полосой снизу, как в IntelliJ.
/// </summary>
public class AxTabItem : ListBoxItem
{
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
}
