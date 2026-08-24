using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace ArxisStudio.Controls;

/// <summary>
/// Плитка с инициалами: аватар проекта, плагина или пользователя. Класс
/// <c>round</c> делает её круглой (пользователь, собеседник), без него — плитка
/// со скруглением, как у проектов в списке недавних.
/// </summary>
public class AxAvatar : TemplatedControl
{
    /// <summary>Инициалы: одна-две буквы.</summary>
    public static readonly StyledProperty<string?> InitialsProperty =
        AvaloniaProperty.Register<AxAvatar, string?>(nameof(Initials));

    /// <summary>Цвет плитки. По умолчанию — акцент студии.</summary>
    public static readonly StyledProperty<IBrush?> TileBrushProperty =
        AvaloniaProperty.Register<AxAvatar, IBrush?>(nameof(TileBrush));

    /// <inheritdoc cref="InitialsProperty"/>
    public string? Initials
    {
        get => GetValue(InitialsProperty);
        set => SetValue(InitialsProperty, value);
    }

    /// <inheritdoc cref="TileBrushProperty"/>
    public IBrush? TileBrush
    {
        get => GetValue(TileBrushProperty);
        set => SetValue(TileBrushProperty, value);
    }
}
