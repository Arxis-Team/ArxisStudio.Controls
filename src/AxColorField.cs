using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.VisualTree;

namespace ArxisStudio.Controls;

/// <summary>
/// Поле цвета: образец и запись <c>#AARRGGBB</c>; щелчок по образцу открывает выбор цвета.
/// </summary>
/// <remarks>
/// <para>
/// Запись правит текст разметки: поле пишет <c>#RRGGBB</c>, а прозрачный — <c>#AARRGGBB</c>, и читает то, что
/// читает Avalonia, — имена («Red»), <c>#RGB</c>, <c>#ARGB</c>, восьмизначную запись, а без решётки —
/// шестнадцатеричные цифры. Набранное фиксируется на Enter и уходе каретки — <see cref="ColorCommitted"/> с
/// набранным текстом: хозяин сам решает, писать ли «Red» или запись. Непрочитанное остаётся с отметкой ошибки.
/// </para>
/// <para>
/// Выбор цвета (<see cref="AxColorPicker"/>) открывается под полем; его события идут сквозь поле: на ходу
/// <see cref="ColorChanging"/>, одна фиксация на жест, Esc посреди протяжки — <see cref="ColorCancelled"/>.
/// Esc вне протяжки и щелчок мимо закрывают выбор, и каретка возвращается к образцу.
/// </para>
/// <para>
/// Разные значения у выбранных (<see cref="IsMixed"/>) — пустая запись с прочерком и образец без цвета.
/// </para>
/// <para>
/// Образец — кнопка <c>PART_Swatch</c> в шаблоне или содержимое слева у поля записи (<c>InnerLeftContent</c>):
/// там он стоит в рамке поля, но живёт своей областью имён, и шаблон его по имени не найдёт.
/// </para>
/// </remarks>
[TemplatePart(TextPart, typeof(TextBox))]
[TemplatePart(SwatchPart, typeof(Button))]
[TemplatePart(PopupPart, typeof(Popup))]
[TemplatePart(PickerPart, typeof(AxColorPicker))]
[PseudoClasses(":mixed", ":open", ":compact")]
public class AxColorField : TemplatedControl
{
    /// <summary>Цвет поля.</summary>
    public static readonly StyledProperty<Color> ColorProperty =
        AvaloniaProperty.Register<AxColorField, Color>(nameof(Color), Colors.White, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Значения у выбранных расходятся: запись пуста, образец без цвета.</summary>
    public static readonly StyledProperty<bool> IsMixedProperty =
        AvaloniaProperty.Register<AxColorField, bool>(nameof(IsMixed));

    /// <summary>Цвет бывает прозрачным.</summary>
    public static readonly StyledProperty<bool> AllowsAlphaProperty =
        AxColorPicker.AllowsAlphaProperty.AddOwner<AxColorField>();

    /// <summary>Цвет только читается: запись выделяется, выбор не открывается.</summary>
    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<AxColorField, bool>(nameof(IsReadOnly));

    /// <summary>Открыт ли выбор цвета.</summary>
    public static readonly StyledProperty<bool> IsPickerOpenProperty =
        AvaloniaProperty.Register<AxColorField, bool>(nameof(IsPickerOpen), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Размер: обычный или тесный — для строк инспектора.</summary>
    public static readonly StyledProperty<AxControlSize> SizeProperty =
        AxButton.SizeProperty.AddOwner<AxColorField>();

    /// <summary>Кисть образца — сплошная цветом поля; у разных значений её нет.</summary>
    public static readonly DirectProperty<AxColorField, IBrush?> SwatchBrushProperty =
        AvaloniaProperty.RegisterDirect<AxColorField, IBrush?>(nameof(SwatchBrush), field => field.SwatchBrush);

    private const string TextPart = "PART_Text";
    private const string SwatchPart = "PART_Swatch";
    private const string PopupPart = "PART_Popup";
    private const string PickerPart = "PART_Picker";

    private TextBox? _text;
    private Button? _swatch;
    private Popup? _popup;
    private AxColorPicker? _picker;
    private IBrush? _swatchBrush;
    private bool _dirty;
    private bool _invalid;
    private bool _byKeyboard;

    static AxColorField()
    {
        ColorProperty.Changed.AddClassHandler<AxColorField>((field, _) => field.Show());
        AllowsAlphaProperty.Changed.AddClassHandler<AxColorField>((field, _) => field.Show());
        IsMixedProperty.Changed.AddClassHandler<AxColorField>((field, change) =>
        {
            field.PseudoClasses.Set(":mixed", change.GetNewValue<bool>());
            field.Show();
        });
        IsPickerOpenProperty.Changed.AddClassHandler<AxColorField>((field, change) => field.OnPickerOpen(change.GetNewValue<bool>()));
        SizeProperty.Changed.AddClassHandler<AxColorField>((field, change) =>
            AxLook.Mark(field.PseudoClasses, change.GetNewValue<AxControlSize>()));
    }

    /// <summary>Заводит поле: события выбора цвета, идущие сквозь него, ставят его цвет.</summary>
    public AxColorField()
    {
        AddHandler(AxColorPicker.ColorChangingEvent, OnPickerColor);
        AddHandler(AxColorPicker.ColorCommittedEvent, OnPickerColor);
        AddHandler(AxColorPicker.ColorCancelledEvent, OnPickerCancelled);
        Show();
    }

    /// <inheritdoc cref="AxColorPicker.ColorChangingEvent"/>
    public event EventHandler<AxColorEventArgs>? ColorChanging
    {
        add => AddHandler(AxColorPicker.ColorChangingEvent, value);
        remove => RemoveHandler(AxColorPicker.ColorChangingEvent, value);
    }

    /// <inheritdoc cref="AxColorPicker.ColorCommittedEvent"/>
    public event EventHandler<AxColorEventArgs>? ColorCommitted
    {
        add => AddHandler(AxColorPicker.ColorCommittedEvent, value);
        remove => RemoveHandler(AxColorPicker.ColorCommittedEvent, value);
    }

    /// <inheritdoc cref="AxColorPicker.ColorCancelledEvent"/>
    public event EventHandler<RoutedEventArgs>? ColorCancelled
    {
        add => AddHandler(AxColorPicker.ColorCancelledEvent, value);
        remove => RemoveHandler(AxColorPicker.ColorCancelledEvent, value);
    }

    /// <inheritdoc cref="ColorProperty"/>
    public Color Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <inheritdoc cref="IsMixedProperty"/>
    public bool IsMixed
    {
        get => GetValue(IsMixedProperty);
        set => SetValue(IsMixedProperty, value);
    }

    /// <inheritdoc cref="AllowsAlphaProperty"/>
    public bool AllowsAlpha
    {
        get => GetValue(AllowsAlphaProperty);
        set => SetValue(AllowsAlphaProperty, value);
    }

    /// <inheritdoc cref="IsReadOnlyProperty"/>
    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <inheritdoc cref="IsPickerOpenProperty"/>
    public bool IsPickerOpen
    {
        get => GetValue(IsPickerOpenProperty);
        set => SetValue(IsPickerOpenProperty, value);
    }

    /// <inheritdoc cref="SizeProperty"/>
    public AxControlSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <inheritdoc cref="SwatchBrushProperty"/>
    public IBrush? SwatchBrush
    {
        get => _swatchBrush;
        private set => SetAndRaise(SwatchBrushProperty, ref _swatchBrush, value);
    }

    /// <summary>Запись поля: куда ставить каретку.</summary>
    public TextBox? Input => _text;

    /// <summary>Выбор цвета под полем — когда шаблон поднят.</summary>
    public AxColorPicker? Picker => _picker;

    /// <summary>Цвет записью, которой его пишет разметка: <c>#RRGGBB</c>, а прозрачный — <c>#AARRGGBB</c>.</summary>
    /// <param name="color">Цвет.</param>
    /// <param name="alpha">Писать ли прозрачность; без неё цвет пишется плотным.</param>
    public static string Format(Color color, bool alpha = true) =>
        alpha && color.A != byte.MaxValue
            ? string.Create(CultureInfo.InvariantCulture, $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}")
            : string.Create(CultureInfo.InvariantCulture, $"#{color.R:X2}{color.G:X2}{color.B:X2}");

    /// <summary>
    /// Читает цвет так, как его читает Avalonia: имя, <c>#RGB</c>, <c>#ARGB</c>, <c>#RRGGBB</c>, <c>#AARRGGBB</c>;
    /// шестнадцатеричные цифры без решётки — тоже.
    /// </summary>
    /// <param name="text">Текст.</param>
    /// <param name="color">Цвет; если не прочлось — прозрачный.</param>
    /// <returns><c>true</c>, если прочлось.</returns>
    public static bool TryParse(string? text, out Color color)
    {
        color = default;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        var trimmed = text.Trim();

        if (Color.TryParse(trimmed, out color))
            return true;

        return !trimmed.StartsWith('#') && trimmed.All(char.IsAsciiHexDigit) && Color.TryParse("#" + trimmed, out color);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnApplyTemplate(e);

        if (_text is not null)
        {
            _text.RemoveHandler(KeyDownEvent, OnTextKey);
            _text.LostFocus -= OnTextBlur;
            _text.PropertyChanged -= OnTextProperty;
        }

        if (_swatch is not null)
            _swatch.Click -= OnSwatchClick;

        if (_popup is not null)
            _popup.Closed -= OnPopupClosed;

        _text = e.NameScope.Find<TextBox>(TextPart);
        _popup = e.NameScope.Find<Popup>(PopupPart);
        _picker = e.NameScope.Find<AxColorPicker>(PickerPart);

        // Образец, положенный содержимым слева у поля записи, живёт в своей области имён, и по имени шаблона
        // его не найти: тогда он и есть это содержимое.
        _swatch = e.NameScope.Find<Button>(SwatchPart) ?? _text?.InnerLeftContent as Button;

        if (_text is not null)
        {
            _text.AddHandler(KeyDownEvent, OnTextKey, RoutingStrategies.Tunnel);
            _text.LostFocus += OnTextBlur;
            _text.PropertyChanged += OnTextProperty;
        }

        if (_swatch is not null)
            _swatch.Click += OnSwatchClick;

        if (_popup is not null)
            _popup.Closed += OnPopupClosed;

        Show(force: true);
    }

    /// <inheritdoc/>
    /// <remarks>Esc вне протяжки закрывает выбор цвета: протяжку Esc отменяет у самой части, и сюда он не доходит.</remarks>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        if (e.Key == Key.Escape && IsPickerOpen)
        {
            SetCurrentValue(IsPickerOpenProperty, false);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Down && e.KeyModifiers == KeyModifiers.Alt && !IsReadOnly)
        {
            _byKeyboard = true;
            SetCurrentValue(IsPickerOpenProperty, true);
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    /// <summary>Показывает цвет: запись — если в ней не набрано своё — и образец.</summary>
    private void Show(bool force = false)
    {
        SwatchBrush = IsMixed ? null : new ImmutableSolidColorBrush(Color);

        if (_text is null || (!force && _dirty && _text.IsKeyboardFocusWithin))
            return;

        // Показ меняет текст и сам же снимает отметку набранного: уведомление о тексте приходит сразу.
        _text.Text = IsMixed ? string.Empty : Format(Color, AllowsAlpha);
        _dirty = false;

        if (_invalid)
        {
            _invalid = false;
            AxValidation.SetState(_text, AxValidationState.None);
        }
    }

    /// <summary>Текст записи сменился — набрано; показ снимает отметку сам.</summary>
    /// <remarks>
    /// По свойству, а не по <see cref="TextBox.TextChanged"/>: то событие Avalonia поднимает позже, когда показ
    /// уже снял отметку, и показанное полем считалось бы набранным — уход каретки фиксировал бы его.
    /// </remarks>
    private void OnTextProperty(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == TextBox.TextProperty)
            _dirty = true;
    }

    private void OnTextKey(object? sender, KeyEventArgs e)
    {
        // Туннель записи проходят и клавиши образца, который лежит внутри неё: Enter на образце — его щелчок.
        if (e.KeyModifiers != KeyModifiers.None || !ReferenceEquals(e.Source, _text))
            return;

        if (e.Key == Key.Enter)
        {
            Commit();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && _dirty)
        {
            Show(force: true);
            e.Handled = true;
        }
    }

    /// <summary>Каретка ушла из записи — набранное фиксируется.</summary>
    /// <remarks>Уход каретки с образца, лежащего внутри записи, всплывает сюда же — он записью не считается.</remarks>
    private void OnTextBlur(object? sender, FocusChangedEventArgs e)
    {
        if (ReferenceEquals(e.Source, _text))
            Commit();
    }

    /// <summary>Набранная запись — в цвет поля; непрочитанная остаётся с отметкой ошибки.</summary>
    private void Commit()
    {
        if (_text is null || !_dirty || IsReadOnly)
            return;

        var text = (_text.Text ?? string.Empty).Trim();

        if (!TryParse(text, out var color))
        {
            _invalid = true;
            AxValidation.SetState(_text, AxValidationState.Error);
            return;
        }

        if (!AllowsAlpha)
            color = Color.FromArgb(byte.MaxValue, color.R, color.G, color.B);

        SetCurrentValue(IsMixedProperty, false);
        SetCurrentValue(ColorProperty, color);
        Show(force: true);
        RaiseEvent(new AxColorEventArgs(AxColorPicker.ColorCommittedEvent, color, text));
    }

    private void OnSwatchClick(object? sender, RoutedEventArgs e)
    {
        if (IsReadOnly)
            return;

        _byKeyboard = _swatch?.IsKeyboardFocusWithin == true && _swatch.Classes.Contains(":focus-visible");
        SetCurrentValue(IsPickerOpenProperty, !IsPickerOpen);
    }

    /// <summary>Выбор открылся — с цветом поля и кареткой в спектре; закрылся — каретка к образцу.</summary>
    private void OnPickerOpen(bool open)
    {
        PseudoClasses.Set(":open", open);

        if (!open || _picker is null)
            return;

        _picker.AllowsAlpha = AllowsAlpha;
        _picker.Color = Color;

        // Каретка — в спектр, когда карточка уже на экране: до показа ставить её некуда.
        Avalonia.Threading.Dispatcher.UIThread.Post(
            () => _picker?.FocusSpectrum(_byKeyboard ? NavigationMethod.Tab : NavigationMethod.Pointer),
            Avalonia.Threading.DispatcherPriority.Loaded);
    }

    /// <summary>Выбор закрылся: каретка, оставшаяся в закрытой карточке, возвращается к образцу.</summary>
    /// <remarks>Каретку, ушедшую к соседу, не трогают: её туда увёл человек.</remarks>
    private void OnPopupClosed(object? sender, EventArgs e)
    {
        SetCurrentValue(IsPickerOpenProperty, false);

        var focused = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as Visual;

        if (focused is null || !focused.IsAttachedToVisualTree() ||
            (_picker is not null && focused.GetSelfAndVisualAncestors().Contains(_picker)))
        {
            _swatch?.Focus(_byKeyboard ? NavigationMethod.Tab : NavigationMethod.Pointer);
        }

        _byKeyboard = false;
    }

    /// <summary>Цвет выбора — цвет поля; событие идёт дальше как есть, к хозяину.</summary>
    private void OnPickerColor(object? sender, AxColorEventArgs e)
    {
        if (ReferenceEquals(e.Source, this) || _picker is null || !IsFrom(e))
            return;

        SetCurrentValue(IsMixedProperty, false);
        SetCurrentValue(ColorProperty, e.Color);
    }

    private void OnPickerCancelled(object? sender, RoutedEventArgs e)
    {
        if (_picker is not null && IsFrom(e))
            SetCurrentValue(ColorProperty, _picker.Color);
    }

    /// <summary>Событие пришло из выбора цвета этого поля.</summary>
    private bool IsFrom(RoutedEventArgs e) =>
        e.Source is Visual source && _picker is not null &&
        (ReferenceEquals(source, _picker) || source.GetVisualAncestors().Contains(_picker));
}
