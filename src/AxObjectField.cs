using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace ArxisStudio.Controls;

/// <summary>
/// Поле объекта, как ObjectField у Unity: миниатюра и имя выбранного, а у пустого — заглушка «Нет (Image)»; кнопка ⊙
/// просит выбор, крестик снимает значение, а цель броска поле подсвечивает.
/// </summary>
/// <remarks>
/// <para>
/// Само поле ничего не выбирает и не пишет: что значит «выбрать» и что можно бросить, знает хозяин. Поле поднимает
/// <see cref="PickRequestedEvent"/> — кнопкой ⊙ (<c>PART_Pick</c>) и с клавиатуры: Enter, пробел, Alt+↓ и F4, — и
/// <see cref="ClearedEvent"/> — крестиком (<c>PART_Clear</c>), Delete и Backspace. Хозяин ставит <see cref="Text"/> и
/// <see cref="Thumbnail"/>, а пока над полем несут то, что можно бросить, — <see cref="IsDropTarget"/>.
/// </para>
/// <para>
/// Поле — одна остановка Tab: кнопки внутри каретку не берут, как крестик у поля поиска. Разные значения у выбранных
/// (<see cref="IsMixed"/>) — прочерк; только для чтения (<see cref="IsReadOnly"/>) — ни выбора, ни снятия.
/// </para>
/// </remarks>
[TemplatePart(PickPart, typeof(Button))]
[TemplatePart(ClearPart, typeof(Button))]
[PseudoClasses(":empty", ":mixed", ":readonly", ":drop-target", ":compact")]
public class AxObjectField : TemplatedControl
{
    /// <summary>Миниатюра выбранного объекта; без неё поле показывает одно имя.</summary>
    public static readonly StyledProperty<IImage?> ThumbnailProperty =
        AvaloniaProperty.Register<AxObjectField, IImage?>(nameof(Thumbnail));

    /// <summary>Имя выбранного объекта; пусто — объекта нет, и стоит заглушка.</summary>
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<AxObjectField, string?>(nameof(Text));

    /// <summary>Заглушка пустого поля, как у Unity: «Нет (Image)».</summary>
    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<AxObjectField, string?>(nameof(PlaceholderText));

    /// <summary>Значения у выбранных расходятся: поле показывает прочерк.</summary>
    public static readonly StyledProperty<bool> IsMixedProperty =
        AvaloniaProperty.Register<AxObjectField, bool>(nameof(IsMixed));

    /// <summary>Значение только читается: ни выбора, ни снятия.</summary>
    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<AxObjectField, bool>(nameof(IsReadOnly));

    /// <summary>Над полем несут то, что на него можно бросить: поле подсвечено.</summary>
    public static readonly StyledProperty<bool> IsDropTargetProperty =
        AvaloniaProperty.Register<AxObjectField, bool>(nameof(IsDropTarget));

    /// <summary>Размер: обычный или тесный — для строк инспектора.</summary>
    public static readonly StyledProperty<AxControlSize> SizeProperty =
        AxButton.SizeProperty.AddOwner<AxObjectField>();

    /// <summary>Просят выбрать объект: кнопка ⊙, Enter, пробел, Alt+↓ или F4.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> PickRequestedEvent =
        RoutedEvent.Register<AxObjectField, RoutedEventArgs>(nameof(PickRequested), RoutingStrategies.Bubble);

    /// <summary>Просят снять значение: крестик, Delete или Backspace.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> ClearedEvent =
        RoutedEvent.Register<AxObjectField, RoutedEventArgs>(nameof(Cleared), RoutingStrategies.Bubble);

    private const string PickPart = "PART_Pick";
    private const string ClearPart = "PART_Clear";

    private Button? _pick;
    private Button? _clear;

    static AxObjectField()
    {
        FocusableProperty.OverrideDefaultValue<AxObjectField>(true);
        TextProperty.Changed.AddClassHandler<AxObjectField>((field, _) => field.Mark());
        IsMixedProperty.Changed.AddClassHandler<AxObjectField>((field, _) => field.Mark());
        IsReadOnlyProperty.Changed.AddClassHandler<AxObjectField>((field, _) => field.Mark());
        IsDropTargetProperty.Changed.AddClassHandler<AxObjectField>((field, change) =>
            field.PseudoClasses.Set(":drop-target", change.GetNewValue<bool>()));
        SizeProperty.Changed.AddClassHandler<AxObjectField>((field, change) =>
            AxLook.Mark(field.PseudoClasses, change.GetNewValue<AxControlSize>()));
    }

    /// <summary>Заводит поле — пустым.</summary>
    public AxObjectField() => Mark();

    /// <inheritdoc cref="PickRequestedEvent"/>
    public event EventHandler<RoutedEventArgs>? PickRequested
    {
        add => AddHandler(PickRequestedEvent, value);
        remove => RemoveHandler(PickRequestedEvent, value);
    }

    /// <inheritdoc cref="ClearedEvent"/>
    public event EventHandler<RoutedEventArgs>? Cleared
    {
        add => AddHandler(ClearedEvent, value);
        remove => RemoveHandler(ClearedEvent, value);
    }

    /// <inheritdoc cref="ThumbnailProperty"/>
    public IImage? Thumbnail
    {
        get => GetValue(ThumbnailProperty);
        set => SetValue(ThumbnailProperty, value);
    }

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

    /// <inheritdoc cref="IsMixedProperty"/>
    public bool IsMixed
    {
        get => GetValue(IsMixedProperty);
        set => SetValue(IsMixedProperty, value);
    }

    /// <inheritdoc cref="IsReadOnlyProperty"/>
    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <inheritdoc cref="IsDropTargetProperty"/>
    public bool IsDropTarget
    {
        get => GetValue(IsDropTargetProperty);
        set => SetValue(IsDropTargetProperty, value);
    }

    /// <inheritdoc cref="SizeProperty"/>
    public AxControlSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Объекта нет, и разных значений тоже: поле показывает заглушку.</summary>
    public bool IsEmpty => !IsMixed && string.IsNullOrEmpty(Text);

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnApplyTemplate(e);

        if (_pick is not null)
            _pick.Click -= OnPickClick;

        if (_clear is not null)
            _clear.Click -= OnClearClick;

        _pick = e.NameScope.Find<Button>(PickPart);
        _clear = e.NameScope.Find<Button>(ClearPart);

        if (_pick is not null)
            _pick.Click += OnPickClick;

        if (_clear is not null)
            _clear.Click += OnClearClick;
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        if (!IsReadOnly && (e is { Key: Key.Down, KeyModifiers: KeyModifiers.Alt }
                            || e is { KeyModifiers: KeyModifiers.None, Key: Key.Enter or Key.Space or Key.F4 }))
        {
            Pick();
            e.Handled = true;
            return;
        }

        if (!IsReadOnly && !IsEmpty && e is { KeyModifiers: KeyModifiers.None, Key: Key.Delete or Key.Back })
        {
            Clear();
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    /// <inheritdoc/>
    /// <remarks>Нажатие ставит каретку в поле: оно остановка Tab, и клавиши выбора и снятия — его.</remarks>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnPointerPressed(e);

        if (!e.Handled && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            Focus(NavigationMethod.Pointer);
    }

    /// <inheritdoc/>
    protected override AutomationPeer OnCreateAutomationPeer() => new FieldPeer(this);

    private void OnPickClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;

        if (!IsReadOnly)
            Pick();
    }

    private void OnClearClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;

        if (!IsReadOnly && !IsEmpty)
            Clear();
    }

    private void Pick() => RaiseEvent(new RoutedEventArgs(PickRequestedEvent, this));

    private void Clear() => RaiseEvent(new RoutedEventArgs(ClearedEvent, this));

    private void Mark()
    {
        PseudoClasses.Set(":empty", IsEmpty);
        PseudoClasses.Set(":mixed", IsMixed);
        PseudoClasses.Set(":readonly", IsReadOnly);
    }

    /// <summary>
    /// Диктору поле — выбор со значением: имя выбранного, заглушка у пустого, прочерк у разных; вызов просит выбор.
    /// </summary>
    private sealed class FieldPeer(AxObjectField owner) : ControlAutomationPeer(owner), IValueProvider, IInvokeProvider
    {
        private AxObjectField Field => (AxObjectField)Owner;

        public bool IsReadOnly => true;

        public string? Value => Field.IsMixed ? "—" : Field.Text is { Length: > 0 } text ? text : Field.PlaceholderText;

        public void SetValue(string? value) =>
            throw new InvalidOperationException("Объект поля выбирают, а не набирают: вызовите выбор.");

        public void Invoke()
        {
            if (!Field.IsReadOnly)
                Field.Pick();
        }

        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.ComboBox;
    }
}
