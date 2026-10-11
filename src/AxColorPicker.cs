using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace ArxisStudio.Controls;

/// <summary>Цвет из выбора цвета и, если его набрали, как набрали.</summary>
/// <param name="routedEvent">Событие.</param>
/// <param name="color">Цвет.</param>
/// <param name="text">Набранная запись; пусто — цвет выбран не текстом.</param>
public sealed class AxColorEventArgs(RoutedEvent routedEvent, Color color, string? text = null)
    : RoutedEventArgs(routedEvent)
{
    /// <summary>Цвет.</summary>
    public Color Color { get; } = color;

    /// <summary>
    /// Как цвет набрали текстом — «Red», «#F80»; пусто — выбрали мышью или стрелками, и как его записать,
    /// решает хозяин. Выбор цвета отдаёт набранное, только если разметка прочтёт его тем же цветом.
    /// </summary>
    public string? Text { get; } = text;
}

/// <summary>Как выбор цвета показывает каналы — как переключатель окна цвета у Unity.</summary>
public enum AxColorPickerMode
{
    /// <summary>Красный, зелёный, синий — целыми 0…255.</summary>
    Rgb255,

    /// <summary>Красный, зелёный, синий — долями 0…1.</summary>
    Rgb1,

    /// <summary>Оттенок 0…360°, насыщенность и яркость 0…100.</summary>
    Hsv,
}

/// <summary>
/// Выбор цвета, как окно Color у Unity: кольцо оттенков с квадратом насыщенности и яркости внутри, каналы ползунками
/// с числами в трёх видах, шестнадцатеричный код, пипетка и образцы «стало» и «было».
/// </summary>
/// <remarks>
/// <para>
/// <b>Источник — HSV.</b> Цвет держится моделью HSV (<see cref="Hsv"/>), а <see cref="Color"/> из неё выводится:
/// иначе оттенок пропадал бы, стоит цвету стать серым или чёрным, и ручка оттенка прыгала бы в начало. Цвет,
/// поставленный снаружи, оттенок серого не трогает.
/// </para>
/// <para>
/// <b>Один жест — одна фиксация.</b> Части — кольцо (<see cref="AxColorWheel"/>), квадрат (<see cref="AxColorSpectrum"/>),
/// ползунки (<see cref="AxColorSlider"/>), числа, код, пипетка и образец «было» — поднимают одни и те же события: на
/// ходу <see cref="ColorChangingEvent"/>, при отпускании, шаге стрелкой или наборе — <see cref="ColorCommittedEvent"/>,
/// при Esc посреди протяжки — <see cref="ColorCancelledEvent"/>. Хозяин показывает цвет на ходу предпросмотром, а пишет
/// одно значение; окно цвета поля (<see cref="AxColorField"/>) пишет одно значение на весь сеанс.
/// </para>
/// <para>
/// <b>Вид каналов</b> (<see cref="Mode"/>) — как у Unity: RGB целыми 0…255, RGB долями 0…1 или HSV. Ползунок канала —
/// градиент канала при остальных. Код — шесть цифр без прозрачности, как у Unity, а прозрачность — своим ползунком;
/// набранная восьмизначная запись ставит и её. Читается то, что читает Avalonia, — и имена; набранное уходит
/// хозяину текстом (<see cref="AxColorEventArgs.Text"/>), только если разметка прочтёт его тем же цветом: «Navy» —
/// да, шесть цифр без решётки — нет.
/// </para>
/// <para>
/// <b>«Было»</b> (<see cref="OriginalColor"/>) — цвет, с которого начали: щелчок по его образцу возвращает его. Пусто —
/// цвета не было вовсе, и вернуть его выбор не может.
/// </para>
/// <para>
/// Без прозрачности (<see cref="AllowsAlpha"/> выключено) цвет всегда плотный, и ползунка прозрачности нет.
/// </para>
/// </remarks>
[TemplatePart(SpectrumPart, typeof(AxColorSpectrum))]
[TemplatePart(WheelPart, typeof(AxColorWheel))]
[TemplatePart(HuePart, typeof(AxColorSlider))]
[TemplatePart(AlphaPart, typeof(AxColorSlider))]
[TemplatePart(HexPart, typeof(TextBox))]
[TemplatePart(ChannelsPart, typeof(AxVectorField))]
[TemplatePart(ModePart, typeof(SelectingItemsControl))]
[TemplatePart(EyedropperPart, typeof(Button))]
[TemplatePart(NewPart, typeof(AxColorSwatch))]
[TemplatePart(OriginalPart, typeof(Button))]
[TemplatePart(OriginalSwatchPart, typeof(AxColorSwatch))]
[PseudoClasses(":rgb255", ":rgb1", ":hsv")]
public class AxColorPicker : TemplatedControl
{
    /// <summary>Выбранный цвет.</summary>
    public static readonly StyledProperty<Color> ColorProperty =
        AvaloniaProperty.Register<AxColorPicker, Color>(nameof(Color), Colors.White, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Выбранный цвет в модели HSV — источник, из которого выводится <see cref="Color"/>.</summary>
    public static readonly StyledProperty<HsvColor> HsvProperty =
        AvaloniaProperty.Register<AxColorPicker, HsvColor>(
            nameof(Hsv), new HsvColor(1, 0, 0, 1), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Цвет бывает прозрачным: ползунок непрозрачности и восьмизначная запись.</summary>
    public static readonly StyledProperty<bool> AllowsAlphaProperty =
        AvaloniaProperty.Register<AxColorPicker, bool>(nameof(AllowsAlpha), true);

    /// <summary>Как показаны каналы: RGB 0…255, RGB 0…1 или HSV.</summary>
    public static readonly StyledProperty<AxColorPickerMode> ModeProperty =
        AvaloniaProperty.Register<AxColorPicker, AxColorPickerMode>(nameof(Mode), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Цвет, с которого начали, — образец «было»; пусто — цвета не было.</summary>
    public static readonly StyledProperty<Color?> OriginalColorProperty =
        AvaloniaProperty.Register<AxColorPicker, Color?>(nameof(OriginalColor));

    /// <summary>Цвет меняется протяжкой: его видно, но он ещё не зафиксирован.</summary>
    public static readonly RoutedEvent<AxColorEventArgs> ColorChangingEvent =
        RoutedEvent.Register<AxColorPicker, AxColorEventArgs>(nameof(ColorChanging), RoutingStrategies.Bubble);

    /// <summary>Цвет зафиксирован: конец протяжки, шаг стрелкой, набранная запись.</summary>
    public static readonly RoutedEvent<AxColorEventArgs> ColorCommittedEvent =
        RoutedEvent.Register<AxColorPicker, AxColorEventArgs>(nameof(ColorCommitted), RoutingStrategies.Bubble);

    /// <summary>Протяжку отменили: цвет вернулся к тому, что был до неё.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> ColorCancelledEvent =
        RoutedEvent.Register<AxColorPicker, RoutedEventArgs>(nameof(ColorCancelled), RoutingStrategies.Bubble);

    private const string SpectrumPart = "PART_Spectrum";
    private const string WheelPart = "PART_Wheel";
    private const string HuePart = "PART_Hue";
    private const string AlphaPart = "PART_Alpha";
    private const string HexPart = "PART_Hex";
    private const string ChannelsPart = "PART_Channels";
    private const string ModePart = "PART_Mode";
    private const string EyedropperPart = "PART_Eyedropper";
    private const string NewPart = "PART_New";
    private const string OriginalPart = "PART_Original";
    private const string OriginalSwatchPart = "PART_OriginalSwatch";

    /// <summary>Строки каналов: ползунок, число и подпись — 0…2 цветом, A — прозрачностью.</summary>
    private static readonly string[] Rows = ["0", "1", "2", "A"];

    private readonly AxColorSlider?[] _sliders = new AxColorSlider?[4];
    private readonly AxNumberField?[] _numbers = new AxNumberField?[4];
    private readonly TextBlock?[] _labels = new TextBlock?[4];

    private AxColorSpectrum? _spectrum;
    private AxColorWheel? _wheel;
    private AxColorSlider? _hue;
    private AxColorSlider? _alpha;
    private TextBox? _hex;
    private AxVectorField? _channels;
    private SelectingItemsControl? _mode;
    private Button? _eyedropper;
    private AxColorSwatch? _new;
    private Button? _original;
    private AxColorSwatch? _originalSwatch;
    private bool _syncing;
    private bool _picking;
    private HsvColor? _scrubStart;

    static AxColorPicker()
    {
        ColorProperty.Changed.AddClassHandler<AxColorPicker>((picker, change) => picker.Adopt(change.GetNewValue<Color>()));
        HsvProperty.Changed.AddClassHandler<AxColorPicker>((picker, _) => picker.Spread());
        AllowsAlphaProperty.Changed.AddClassHandler<AxColorPicker>((picker, _) =>
        {
            picker.Label();
            picker.Adopt(picker.Color);
            picker.Spread();
        });
        ModeProperty.Changed.AddClassHandler<AxColorPicker>((picker, _) =>
        {
            picker.Label();
            picker.Spread(force: true);
        });
        OriginalColorProperty.Changed.AddClassHandler<AxColorPicker>((picker, _) => picker.ShowOriginal());
    }

    /// <summary>Заводит выбор цвета: события частей слушаются здесь.</summary>
    public AxColorPicker()
    {
        AddHandler(ColorChangingEvent, OnPartColor);
        AddHandler(ColorCommittedEvent, OnPartColor);
        AddHandler(ColorCancelledEvent, OnPartCancelled);
        AddHandler(AxVectorField.ValuesChangingEvent, OnChannelsChanging);
        AddHandler(AxVectorField.ValuesCommittedEvent, OnChannelsCommitted);
        AddHandler(AxVectorField.ScrubCancelledEvent, OnChannelsCancelled);
        AddHandler(AxNumberField.ValueChangingEvent, OnNumberChanging);
        AddHandler(AxNumberField.ValueCommittedEvent, OnNumberCommitted);
        AddHandler(AxNumberField.ScrubCancelledEvent, OnNumberCancelled);
        Label();
    }

    /// <inheritdoc cref="ColorChangingEvent"/>
    public event EventHandler<AxColorEventArgs>? ColorChanging
    {
        add => AddHandler(ColorChangingEvent, value);
        remove => RemoveHandler(ColorChangingEvent, value);
    }

    /// <inheritdoc cref="ColorCommittedEvent"/>
    public event EventHandler<AxColorEventArgs>? ColorCommitted
    {
        add => AddHandler(ColorCommittedEvent, value);
        remove => RemoveHandler(ColorCommittedEvent, value);
    }

    /// <inheritdoc cref="ColorCancelledEvent"/>
    public event EventHandler<RoutedEventArgs>? ColorCancelled
    {
        add => AddHandler(ColorCancelledEvent, value);
        remove => RemoveHandler(ColorCancelledEvent, value);
    }

    /// <inheritdoc cref="ColorProperty"/>
    public Color Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <inheritdoc cref="HsvProperty"/>
    public HsvColor Hsv
    {
        get => GetValue(HsvProperty);
        set => SetValue(HsvProperty, value);
    }

    /// <inheritdoc cref="AllowsAlphaProperty"/>
    public bool AllowsAlpha
    {
        get => GetValue(AllowsAlphaProperty);
        set => SetValue(AllowsAlphaProperty, value);
    }

    /// <inheritdoc cref="ModeProperty"/>
    public AxColorPickerMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    /// <inheritdoc cref="OriginalColorProperty"/>
    public Color? OriginalColor
    {
        get => GetValue(OriginalColorProperty);
        set => SetValue(OriginalColorProperty, value);
    }

    /// <summary>Шестнадцатеричный код цвета, как его показывает Unity: шесть цифр без решётки и прозрачности.</summary>
    /// <param name="color">Цвет.</param>
    public static string Hex(Color color) =>
        string.Create(CultureInfo.InvariantCulture, $"{color.R:X2}{color.G:X2}{color.B:X2}");

    /// <summary>Ставит каретку в квадрат насыщенности и яркости — оттуда выбор цвета ведут с клавиатуры.</summary>
    /// <param name="method">Как пришла каретка: с клавиатуры квадрат показывает кольцо.</param>
    /// <returns>Встала ли каретка.</returns>
    public bool FocusSpectrum(NavigationMethod method = NavigationMethod.Unspecified) =>
        _spectrum?.Focus(method) == true;

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnApplyTemplate(e);

        if (_hex is not null)
        {
            _hex.RemoveHandler(KeyDownEvent, OnHexKey);
            _hex.LostFocus -= OnHexBlur;
        }

        if (_mode is not null)
            _mode.SelectionChanged -= OnModeChosen;

        if (_eyedropper is not null)
            _eyedropper.Click -= OnEyedropperClick;

        if (_original is not null)
            _original.Click -= OnOriginalClick;

        _spectrum = e.NameScope.Find<AxColorSpectrum>(SpectrumPart);
        _wheel = e.NameScope.Find<AxColorWheel>(WheelPart);
        _hue = e.NameScope.Find<AxColorSlider>(HuePart);
        _alpha = e.NameScope.Find<AxColorSlider>(AlphaPart);
        _hex = e.NameScope.Find<TextBox>(HexPart);
        _channels = e.NameScope.Find<AxVectorField>(ChannelsPart);
        _mode = e.NameScope.Find<SelectingItemsControl>(ModePart);
        _eyedropper = e.NameScope.Find<Button>(EyedropperPart);
        _new = e.NameScope.Find<AxColorSwatch>(NewPart);
        _original = e.NameScope.Find<Button>(OriginalPart);
        _originalSwatch = e.NameScope.Find<AxColorSwatch>(OriginalSwatchPart);

        for (var at = 0; at < Rows.Length; at++)
        {
            _sliders[at] = e.NameScope.Find<AxColorSlider>("PART_Slider" + Rows[at]);
            _numbers[at] = e.NameScope.Find<AxNumberField>("PART_Number" + Rows[at]);
            _labels[at] = e.NameScope.Find<TextBlock>("PART_Label" + Rows[at]);
        }

        if (_hex is not null)
        {
            _hex.AddHandler(KeyDownEvent, OnHexKey, RoutingStrategies.Tunnel);
            _hex.LostFocus += OnHexBlur;
        }

        if (_mode is not null)
            _mode.SelectionChanged += OnModeChosen;

        if (_eyedropper is not null)
            _eyedropper.Click += OnEyedropperClick;

        if (_original is not null)
            _original.Click += OnOriginalClick;

        Label();
        ShowOriginal();
        Spread(force: true);
    }

    /// <summary>Цвет, поставленный снаружи, — в модель HSV; у серого и чёрного оттенок остаётся прежним.</summary>
    private void Adopt(Color color)
    {
        if (_syncing)
            return;

        if (!AllowsAlpha)
            color = Color.FromArgb(byte.MaxValue, color.R, color.G, color.B);

        var current = Hsv;

        if (current.ToRgb() == color)
        {
            Spread();
            return;
        }

        SetCurrentValue(HsvProperty, AxColorSlider.Keep(current, color.ToHsv(), color.A / (double)byte.MaxValue));
    }

    /// <summary>Модель HSV — в цвет и во все части.</summary>
    /// <param name="force">Обновить и поля, в которых стоит каретка: так при подъёме шаблона и смене вида.</param>
    private void Spread(bool force = false)
    {
        if (_syncing)
            return;

        _syncing = true;

        try
        {
            var hsv = Hsv;
            var color = hsv.ToRgb();

            SetCurrentValue(ColorProperty, color);
            _spectrum?.SetCurrentValue(AxColorSpectrum.HsvProperty, hsv);
            _wheel?.SetCurrentValue(AxColorWheel.HsvProperty, hsv);
            _hue?.SetCurrentValue(AxColorSlider.HsvProperty, hsv);
            _alpha?.SetCurrentValue(AxColorSlider.HsvProperty, hsv);
            _channels?.SetCurrentValue(AxVectorField.ValuesProperty, Channels(color));

            if (_new is not null)
                _new.Color = color;

            for (var at = 0; at < Rows.Length; at++)
            {
                _sliders[at]?.SetCurrentValue(AxColorSlider.HsvProperty, hsv);

                // Число, которое человек набирает, не затирается: оно станет цветом на Enter или уходе каретки.
                if (_numbers[at] is { } number && (force || !number.IsKeyboardFocusWithin))
                    number.Value = NumberOf(at, hsv, color);
            }

            // Код, который человек набирает, не затирается: он станет цветом на Enter или уходе каретки.
            if (_hex is not null && (force || !_hex.IsKeyboardFocusWithin))
            {
                _hex.Text = Hex(color);
                AxValidation.SetState(_hex, AxValidationState.None);
            }
        }
        finally
        {
            _syncing = false;
        }
    }

    /// <summary>Подписи, каналы ползунков и границы чисел — по виду каналов и прозрачности.</summary>
    private void Label()
    {
        var mode = Mode;

        PseudoClasses.Set(":rgb255", mode == AxColorPickerMode.Rgb255);
        PseudoClasses.Set(":rgb1", mode == AxColorPickerMode.Rgb1);
        PseudoClasses.Set(":hsv", mode == AxColorPickerMode.Hsv);

        if (_mode is not null && _mode.SelectedIndex != (int)mode)
            _mode.SelectedIndex = (int)mode;

        string[] names = mode == AxColorPickerMode.Hsv ? ["H", "S", "V", "A"] : ["R", "G", "B", "A"];
        AxColorChannel[] channels = mode == AxColorPickerMode.Hsv
            ? [AxColorChannel.Hue, AxColorChannel.Saturation, AxColorChannel.Value, AxColorChannel.Alpha]
            : [AxColorChannel.Red, AxColorChannel.Green, AxColorChannel.Blue, AxColorChannel.Alpha];

        for (var at = 0; at < Rows.Length; at++)
        {
            if (_labels[at] is { } label)
                label.Text = names[at];

            if (_sliders[at] is { } slider)
                slider.Channel = channels[at];

            if (_numbers[at] is { } number)
            {
                var (maximum, decimals) = RangeOf(at);

                number.Minimum = 0;
                number.Maximum = maximum;
                number.Decimals = decimals;
                number.Step = decimals == 0 ? 1 : 0.01;
            }
        }

        if (_channels is not null)
        {
            _channels.Labels = AllowsAlpha ? ["R", "G", "B", "A"] : ["R", "G", "B"];
            _channels.Minimum = 0;
            _channels.Maximum = byte.MaxValue;
            _channels.Decimals = 0;
        }
    }

    /// <summary>Образец «было»: цвет начала, а без него — перечёркнутый, и вернуть его нечем.</summary>
    private void ShowOriginal()
    {
        if (_originalSwatch is not null)
        {
            _originalSwatch.IsNone = OriginalColor is null;
            _originalSwatch.Color = OriginalColor ?? Colors.Transparent;
        }

        if (_original is not null)
            _original.IsEnabled = OriginalColor is not null;
    }

    /// <summary>Верх числа строки и знаков после точки — по виду каналов.</summary>
    private (double Maximum, int Decimals) RangeOf(int row) => (Mode, row) switch
    {
        (AxColorPickerMode.Rgb1, _) => (1, 3),
        (AxColorPickerMode.Hsv, 0) => (360, 0),
        (AxColorPickerMode.Hsv, 1 or 2) => (100, 0),
        _ => (byte.MaxValue, 0),
    };

    /// <summary>Число строки в единицах вида каналов.</summary>
    private double NumberOf(int row, HsvColor hsv, Color color) => (Mode, row) switch
    {
        (_, 3) when Mode == AxColorPickerMode.Rgb1 => Math.Round(hsv.A, 3),
        (_, 3) => Math.Round(hsv.A * byte.MaxValue),
        (AxColorPickerMode.Hsv, 0) => Math.Round(hsv.H),
        (AxColorPickerMode.Hsv, 1) => Math.Round(hsv.S * 100),
        (AxColorPickerMode.Hsv, 2) => Math.Round(hsv.V * 100),
        (AxColorPickerMode.Rgb1, _) => Math.Round(Channel(color, row) / (double)byte.MaxValue, 3),
        _ => Channel(color, row),
    };

    /// <summary>Модель с числом строки, набранным в единицах вида каналов.</summary>
    private HsvColor WithNumber(int row, double value)
    {
        var hsv = Hsv;

        if (row == 3)
        {
            var alpha = Mode == AxColorPickerMode.Rgb1 ? value : value / byte.MaxValue;

            return new HsvColor(Math.Clamp(alpha, 0, 1), hsv.H, hsv.S, hsv.V);
        }

        if (Mode == AxColorPickerMode.Hsv)
        {
            return row switch
            {
                0 => new HsvColor(hsv.A, Math.Clamp(value, 0, 360 - 1e-6), hsv.S, hsv.V),
                1 => new HsvColor(hsv.A, hsv.H, Math.Clamp(value / 100, 0, 1), hsv.V),
                _ => new HsvColor(hsv.A, hsv.H, hsv.S, Math.Clamp(value / 100, 0, 1)),
            };
        }

        var level = Mode == AxColorPickerMode.Rgb1 ? value : value / byte.MaxValue;
        AxColorChannel[] channels = [AxColorChannel.Red, AxColorChannel.Green, AxColorChannel.Blue];

        return AxColorSlider.WriteRgb(hsv, channels[row], Math.Clamp(level, 0, 1));
    }

    private static byte Channel(Color color, int row) => row switch
    {
        0 => color.R,
        1 => color.G,
        _ => color.B,
    };

    /// <summary>Цвет части — кольца, квадрата или ползунка — становится цветом выбора; событие идёт дальше как есть.</summary>
    private void OnPartColor(object? sender, AxColorEventArgs e)
    {
        if (Part(e) is { } hsv)
            SetCurrentValue(HsvProperty, hsv);
    }

    private void OnPartCancelled(object? sender, RoutedEventArgs e)
    {
        if (Part(e) is { } hsv)
            SetCurrentValue(HsvProperty, hsv);
    }

    /// <summary>Модель HSV части, из которой пришло событие; пусто — событие не от части.</summary>
    private HsvColor? Part(RoutedEventArgs e) => e.Source switch
    {
        AxColorSpectrum spectrum when ReferenceEquals(spectrum, _spectrum) => spectrum.Hsv,
        AxColorWheel wheel when ReferenceEquals(wheel, _wheel) => wheel.Hsv,
        AxColorSlider slider when ReferenceEquals(slider, _hue) || ReferenceEquals(slider, _alpha) ||
                                  _sliders.Contains(slider) => slider.Hsv,
        _ => null,
    };

    private void OnModeChosen(object? sender, SelectionChangedEventArgs e)
    {
        if (_mode is { SelectedIndex: >= 0 and <= 2 } mode && mode.SelectedIndex != (int)Mode)
            SetCurrentValue(ModeProperty, (AxColorPickerMode)mode.SelectedIndex);
    }

    private void OnNumberChanging(object? sender, AxNumberEventArgs e)
    {
        if (RowOf(e.Source) is not { } row)
            return;

        e.Handled = true;
        _scrubStart ??= Hsv;
        SetCurrentValue(HsvProperty, WithNumber(row, e.Value));
        RaiseEvent(new AxColorEventArgs(ColorChangingEvent, Color));
    }

    private void OnNumberCommitted(object? sender, AxNumberEventArgs e)
    {
        if (RowOf(e.Source) is not { } row)
            return;

        e.Handled = true;
        _scrubStart = null;

        var next = WithNumber(row, e.Value);

        if (next.Equals(Hsv))
        {
            Spread(force: true);
            return;
        }

        SetCurrentValue(HsvProperty, next);
        Spread(force: true);
        RaiseEvent(new AxColorEventArgs(ColorCommittedEvent, Color));
    }

    private void OnNumberCancelled(object? sender, RoutedEventArgs e)
    {
        if (RowOf(e.Source) is null)
            return;

        e.Handled = true;

        if (_scrubStart is { } start)
        {
            _scrubStart = null;
            SetCurrentValue(HsvProperty, start);
        }

        RaiseEvent(new RoutedEventArgs(ColorCancelledEvent));
    }

    /// <summary>Строка числа, из которого пришло событие; пусто — не наше число.</summary>
    private int? RowOf(object? source)
    {
        for (var at = 0; at < _numbers.Length; at++)
        {
            if (_numbers[at] is { } number && ReferenceEquals(number, source))
                return at;
        }

        return null;
    }

    private void OnChannelsChanging(object? sender, AxVectorEventArgs e)
    {
        if (!ReferenceEquals(e.Source, _channels))
            return;

        e.Handled = true;
        _scrubStart ??= Hsv;
        Adopt(FromChannels(e.Values));
        RaiseEvent(new AxColorEventArgs(ColorChangingEvent, Color));
    }

    private void OnChannelsCommitted(object? sender, AxVectorEventArgs e)
    {
        if (!ReferenceEquals(e.Source, _channels))
            return;

        e.Handled = true;
        _scrubStart = null;
        Adopt(FromChannels(e.Values));
        RaiseEvent(new AxColorEventArgs(ColorCommittedEvent, Color));
    }

    private void OnChannelsCancelled(object? sender, RoutedEventArgs e)
    {
        if (!ReferenceEquals(e.Source, _channels))
            return;

        e.Handled = true;

        if (_scrubStart is { } start)
        {
            _scrubStart = null;
            SetCurrentValue(HsvProperty, start);
        }

        RaiseEvent(new RoutedEventArgs(ColorCancelledEvent));
    }

    private void OnHexKey(object? sender, KeyEventArgs e)
    {
        if (_hex is null || e.KeyModifiers != KeyModifiers.None)
            return;

        if (e.Key == Key.Enter)
        {
            CommitHex();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && _hex.Text != Hex(Color))
        {
            Spread(force: true);
            e.Handled = true;
        }
    }

    private void OnHexBlur(object? sender, FocusChangedEventArgs e) => CommitHex();

    /// <summary>
    /// Набранный код — в цвет: шесть цифр и имена ставят цвет, а прозрачность остаётся прежней; восьмизначная запись
    /// ставит и её. Непрочитанное остаётся с отметкой ошибки.
    /// </summary>
    private void CommitHex()
    {
        if (_hex is null)
            return;

        var text = (_hex.Text ?? string.Empty).Trim();

        if (string.Equals(text.TrimStart('#'), Hex(Color), StringComparison.OrdinalIgnoreCase))
        {
            AxValidation.SetState(_hex, AxValidationState.None);
            return;
        }

        if (!AxColorField.TryParse(text, out var color))
        {
            AxValidation.SetState(_hex, AxValidationState.Error);
            return;
        }

        var digits = text.TrimStart('#');
        var carriesAlpha = digits.All(char.IsAsciiHexDigit) && digits.Length is 4 or 8;

        if (!carriesAlpha)
            color = Color.FromArgb(Color.A, color.R, color.G, color.B);

        Adopt(color);
        Spread(force: true);

        // Набранное — хозяину, только если разметка прочтёт его тем же цветом: «Navy» — да, а шесть цифр без решётки
        // и запись без прозрачности поверх прозрачного цвета — нет, их хозяин запишет сам.
        var spelled = Color.TryParse(text, out var read) && read == Color ? text : null;

        RaiseEvent(new AxColorEventArgs(ColorCommittedEvent, Color, spelled));
    }

    private void OnEyedropperClick(object? sender, RoutedEventArgs e) => _ = PickFromScreenAsync();

    /// <summary>Пипетка: цвет пикселя экрана — в выбор; прозрачность остаётся прежней, экран её не знает.</summary>
    private async Task PickFromScreenAsync()
    {
        if (_picking)
            return;

        _picking = true;

        try
        {
            if (await AxEyedropper.PickAsync(this).ConfigureAwait(true) is not { } picked)
                return;

            Adopt(Color.FromArgb(Color.A, picked.R, picked.G, picked.B));
            Spread(force: true);
            RaiseEvent(new AxColorEventArgs(ColorCommittedEvent, Color));
        }
        finally
        {
            _picking = false;
        }
    }

    /// <summary>Образец «было» возвращает цвет начала.</summary>
    private void OnOriginalClick(object? sender, RoutedEventArgs e)
    {
        if (OriginalColor is not { } original || original == Color)
            return;

        Adopt(original);
        Spread(force: true);
        RaiseEvent(new AxColorEventArgs(ColorCommittedEvent, Color));
    }

    private double[] Channels(Color color) =>
        AllowsAlpha ? [color.R, color.G, color.B, color.A] : [color.R, color.G, color.B];

    private Color FromChannels(IReadOnlyList<double> values)
    {
        static byte Byte(IReadOnlyList<double> values, int at) =>
            at < values.Count ? (byte)Math.Clamp(Math.Round(values[at]), 0, byte.MaxValue) : byte.MaxValue;

        return Color.FromArgb(AllowsAlpha ? Byte(values, 3) : byte.MaxValue, Byte(values, 0), Byte(values, 1), Byte(values, 2));
    }
}
