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

    /// <summary>Показывать крестик закрытия в заголовке.</summary>
    public static readonly StyledProperty<bool> IsCloseVisibleProperty =
        AvaloniaProperty.Register<AxDialog, bool>(nameof(IsCloseVisible), true);

    /// <summary>Создаёт диалог: без системной рамки, поверх владельца, по центру.</summary>
    public AxDialog()
    {
        WindowDecorations = WindowDecorations.None;
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

    /// <inheritdoc cref="FooterContentProperty"/>
    public object? FooterContent
    {
        get => GetValue(FooterContentProperty);
        set => SetValue(FooterContentProperty, value);
    }

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
