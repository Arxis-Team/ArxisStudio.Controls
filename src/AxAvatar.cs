using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace ArxisStudio.Controls;

/// <summary>Форма плитки аватара.</summary>
public enum AxAvatarShape
{
    /// <summary>Плитка со скруглением: проект, плагин.</summary>
    Tile,

    /// <summary>Круг: пользователь, собеседник.</summary>
    Circle,
}

/// <summary>
/// Цвет плитки монограммы.
/// </summary>
/// <remarks>
/// Цвета подобраны темой так, что белые инициалы читаются на каждом; поэтому цвет
/// называют из списка, а не кистью на месте.
/// </remarks>
public enum AxAvatarTint
{
    /// <summary>Акцент студии.</summary>
    Accent,

    /// <summary>Оранжевый.</summary>
    Orange,

    /// <summary>Зелёный.</summary>
    Green,

    /// <summary>Фиолетовый.</summary>
    Purple,

    /// <summary>Красный.</summary>
    Red,
}

/// <summary>
/// Плитка с инициалами: аватар проекта, плагина или пользователя.
/// </summary>
/// <remarks>
/// Форма и цвет — перечисления, а не классы <c>round</c> и <c>purple</c>: тема видит их
/// псевдоклассами, и опечатка в имени не оставит плитку молча акцентной.
/// </remarks>
[PseudoClasses(":circle", ":orange", ":green", ":purple", ":red")]
public class AxAvatar : TemplatedControl
{
    /// <summary>Инициалы: одна-две буквы.</summary>
    public static readonly StyledProperty<string?> InitialsProperty =
        AvaloniaProperty.Register<AxAvatar, string?>(nameof(Initials));

    /// <summary>Цвет плитки своей кистью; обычно цвет называют <see cref="Tint"/>.</summary>
    public static readonly StyledProperty<IBrush?> TileBrushProperty =
        AvaloniaProperty.Register<AxAvatar, IBrush?>(nameof(TileBrush));

    /// <summary>Форма плитки.</summary>
    public static readonly StyledProperty<AxAvatarShape> ShapeProperty =
        AvaloniaProperty.Register<AxAvatar, AxAvatarShape>(nameof(Shape));

    /// <summary>Цвет плитки из списка темы.</summary>
    public static readonly StyledProperty<AxAvatarTint> TintProperty =
        AvaloniaProperty.Register<AxAvatar, AxAvatarTint>(nameof(Tint));

    static AxAvatar()
    {
        ShapeProperty.Changed.AddClassHandler<AxAvatar>((avatar, change) =>
            avatar.PseudoClasses.Set(":circle", change.GetNewValue<AxAvatarShape>() == AxAvatarShape.Circle));
        TintProperty.Changed.AddClassHandler<AxAvatar>((avatar, change) =>
        {
            var tint = change.GetNewValue<AxAvatarTint>();

            avatar.PseudoClasses.Set(":orange", tint == AxAvatarTint.Orange);
            avatar.PseudoClasses.Set(":green", tint == AxAvatarTint.Green);
            avatar.PseudoClasses.Set(":purple", tint == AxAvatarTint.Purple);
            avatar.PseudoClasses.Set(":red", tint == AxAvatarTint.Red);
        });
    }

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

    /// <inheritdoc cref="ShapeProperty"/>
    public AxAvatarShape Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    /// <inheritdoc cref="TintProperty"/>
    public AxAvatarTint Tint
    {
        get => GetValue(TintProperty);
        set => SetValue(TintProperty, value);
    }
}
