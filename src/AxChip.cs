using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace ArxisStudio.Controls;

/// <summary>Вид чипа.</summary>
public enum AxChipKind
{
    /// <summary>Метка: тег шаблона, версия, состояние.</summary>
    Default,

    /// <summary>Выделенная метка.</summary>
    Accent,

    /// <summary>Клавиша или сочетание: «Ctrl+Shift+P».</summary>
    Key,
}

/// <summary>
/// Чип-метка: тег шаблона, версия, состояние плагина, сочетание клавиш, жетон в поле жетонов.
/// </summary>
/// <remarks>
/// <para>
/// Вид — перечисление <see cref="Kind"/>, а не классы стиля: тема видит его
/// псевдоклассами <c>:accent</c> и <c>:key</c>.
/// </para>
/// <para>
/// Снимаемый чип (<see cref="IsRemovable"/>) несёт крестик и просит снять себя —
/// <see cref="RemoveRequestedEvent"/>, — а снимает его хозяин: чип не знает, откуда он. Крестик каретку не
/// берёт: с клавиатуры чип снимают Delete или Backspace, когда каретка на самом чипе.
/// </para>
/// </remarks>
[PseudoClasses(":accent", ":key", ":removable")]
[TemplatePart(RemovePart, typeof(Button))]
public class AxChip : ContentControl
{
    /// <summary>Вид чипа.</summary>
    public static readonly StyledProperty<AxChipKind> KindProperty =
        AvaloniaProperty.Register<AxChip, AxChipKind>(nameof(Kind));

    /// <summary>Чип можно снять: крестик и Delete.</summary>
    public static readonly StyledProperty<bool> IsRemovableProperty =
        AvaloniaProperty.Register<AxChip, bool>(nameof(IsRemovable));

    /// <summary>Чип просят снять: крестиком или клавишей.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> RemoveRequestedEvent =
        RoutedEvent.Register<AxChip, RoutedEventArgs>(nameof(RemoveRequested), RoutingStrategies.Bubble);

    /// <summary>Имя крестика в теме.</summary>
    private const string RemovePart = "PART_Remove";

    private Button? _remove;

    static AxChip()
    {
        KindProperty.Changed.AddClassHandler<AxChip>((chip, change) =>
        {
            chip.PseudoClasses.Set(":accent", change.GetNewValue<AxChipKind>() == AxChipKind.Accent);
            chip.PseudoClasses.Set(":key", change.GetNewValue<AxChipKind>() == AxChipKind.Key);
        });
        IsRemovableProperty.Changed.AddClassHandler<AxChip>((chip, change) =>
            chip.PseudoClasses.Set(":removable", change.GetNewValue<bool>()));
    }

    /// <inheritdoc cref="RemoveRequestedEvent"/>
    public event EventHandler<RoutedEventArgs>? RemoveRequested
    {
        add => AddHandler(RemoveRequestedEvent, value);
        remove => RemoveHandler(RemoveRequestedEvent, value);
    }

    /// <inheritdoc cref="KindProperty"/>
    public AxChipKind Kind
    {
        get => GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <inheritdoc cref="IsRemovableProperty"/>
    public bool IsRemovable
    {
        get => GetValue(IsRemovableProperty);
        set => SetValue(IsRemovableProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnApplyTemplate(e);

        if (_remove is not null)
            _remove.Click -= OnRemoveClick;

        _remove = e.NameScope.Find<Button>(RemovePart);

        if (_remove is not null)
            _remove.Click += OnRemoveClick;
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        if (IsRemovable && e.Key is Key.Delete or Key.Back && e.KeyModifiers == KeyModifiers.None)
        {
            RaiseEvent(new RoutedEventArgs(RemoveRequestedEvent));
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    private void OnRemoveClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;

        if (IsRemovable)
            RaiseEvent(new RoutedEventArgs(RemoveRequestedEvent));
    }
}
