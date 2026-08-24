using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace ArxisStudio.Controls;

/// <summary>
/// Полоса заголовка окна, совмещённая с главным тулбаром: слева содержимое
/// приложения, справа кнопки окна. По пустому месту полосы окно перетаскивают,
/// двойным щелчком разворачивают.
/// </summary>
/// <remarks>
/// В Int UI у главного окна нет отдельного системного заголовка: строка с
/// проектом, веткой и кнопками запуска и есть заголовок. Поэтому окно
/// выключает системную рамку, а роль заголовка берёт этот контрол.
/// </remarks>
public class AxTitleBar : ContentControl
{
    /// <summary>Показывать кнопки окна справа.</summary>
    public static readonly StyledProperty<bool> ShowWindowControlsProperty =
        AvaloniaProperty.Register<AxTitleBar, bool>(nameof(ShowWindowControls), true);

    /// <summary>
    /// Отступ слева под системные кнопки. На macOS система рисует свои кнопки
    /// поверх клиентской области, и содержимому нужно место, чтобы не попасть
    /// под них.
    /// </summary>
    public static readonly StyledProperty<double> SystemControlsInsetProperty =
        AvaloniaProperty.Register<AxTitleBar, double>(nameof(SystemControlsInset));

    /// <inheritdoc cref="ShowWindowControlsProperty"/>
    public bool ShowWindowControls
    {
        get => GetValue(ShowWindowControlsProperty);
        set => SetValue(ShowWindowControlsProperty, value);
    }

    /// <inheritdoc cref="SystemControlsInsetProperty"/>
    public double SystemControlsInset
    {
        get => GetValue(SystemControlsInsetProperty);
        set => SetValue(SystemControlsInsetProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // Место под кнопки-светофоры macOS появляется само: приложение о них
        // не думает, а на других платформах отступа нет.
        if (OperatingSystem.IsMacOS() && SystemControlsInset == 0)
            SystemControlsInset = 78;
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (e.Handled || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        // Перетаскивание начинается только с самой полосы: щелчок по кнопке
        // или полю внутри неё должен работать как щелчок по этому контролу.
        if (e.Source is Visual source && source != this && source.FindAncestorOfType<Button>() is not null)
            return;

        if (this.FindAncestorOfType<Window>() is not { } window)
            return;

        if (e.ClickCount == 2)
        {
            window.WindowState = window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;

            e.Handled = true;
            return;
        }

        window.BeginMoveDrag(e);
    }
}
