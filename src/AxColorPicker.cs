using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace ArxisStudio.Controls;

/// <summary>Цвет — в событиях выбора цвета: на ходу и при фиксации.</summary>
/// <param name="routedEvent">Событие.</param>
/// <param name="color">Цвет.</param>
/// <param name="text">Как цвет набрали текстом; пусто — выбрали мышью или стрелками.</param>
public sealed class AxColorEventArgs(RoutedEvent routedEvent, Color color, string? text = null)
    : RoutedEventArgs(routedEvent)
{
    /// <summary>Цвет.</summary>
    public Color Color { get; } = color;

    /// <summary>
    /// Как цвет набрали текстом — «Red», «#F80»; пусто — выбрали мышью или стрелками, и как его записать,
    /// решает хозяин.
    /// </summary>
    public string? Text { get; } = text;
}

/// <summary>
/// Выбор цвета: спектр насыщенности и яркости, оттенок, непрозрачность, запись <c>#AARRGGBB</c> и каналы RGB.
/// </summary>
/// <remarks>
/// <para>
/// <b>Источник — HSV.</b> Цвет держится моделью HSV (<see cref="Hsv"/>), а <see cref="Color"/> из неё выводится:
/// иначе оттенок пропадал бы, стоит цвету стать серым или чёрным, и ручка оттенка прыгала бы в начало. Цвет,
/// поставленный снаружи, оттенок серого не трогает.
/// </para>
/// <para>
/// <b>Один жест — одна фиксация.</b> Части — <see cref="AxColorSpectrum"/>, <see cref="AxColorSlider"/>, поле
/// записи и каналы — поднимают одни и те же события: на ходу <see cref="ColorChangingEvent"/>, при отпускании
/// или шаге стрелкой — <see cref="ColorCommittedEvent"/>, при Esc посреди протяжки —
/// <see cref="ColorCancelledEvent"/>. Хозяин показывает цвет на ходу предпросмотром, а пишет одно значение.
/// </para>
/// <para>
/// Без прозрачности (<see cref="AllowsAlpha"/> выключено) цвет всегда плотный, а запись — <c>#RRGGBB</c>.
/// </para>
/// </remarks>
[TemplatePart(SpectrumPart, typeof(AxColorSpectrum))]
[TemplatePart(HuePart, typeof(AxColorSlider))]
[TemplatePart(AlphaPart, typeof(AxColorSlider))]
[TemplatePart(HexPart, typeof(TextBox))]
[TemplatePart(ChannelsPart, typeof(AxVectorField))]
public class AxColorPicker : TemplatedControl
{
    /// <summary>Выбранный цвет.</summary>
    public static readonly StyledProperty<Color> ColorProperty =
        AvaloniaProperty.Register<AxColorPicker, Color>(nameof(Color), Colors.White, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Выбранный цвет в модели HSV — источник, из которого выводится <see cref="Color"/>.</summary>
    public static readonly StyledProperty<HsvColor> HsvProperty =
        AvaloniaProperty.Register<AxColorPicker, HsvColor>(
            nameof(Hsv), new HsvColor(1, 0, 0, 1), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Цвет бывает прозрачным: ползунок непрозрачности, канал A и запись <c>#AARRGGBB</c>.</summary>
    public static readonly StyledProperty<bool> AllowsAlphaProperty =
        AvaloniaProperty.Register<AxColorPicker, bool>(nameof(AllowsAlpha), true);

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
    private const string HuePart = "PART_Hue";
    private const string AlphaPart = "PART_Alpha";
    private const string HexPart = "PART_Hex";
    private const string ChannelsPart = "PART_Channels";

    private AxColorSpectrum? _spectrum;
    private AxColorSlider? _hue;
    private AxColorSlider? _alpha;
    private TextBox? _hex;
    private AxVectorField? _channels;
    private bool _syncing;
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

    /// <summary>Ставит каретку в спектр — оттуда выбор цвета ведут с клавиатуры.</summary>
    /// <param name="method">Как пришла каретка: с клавиатуры спектр показывает кольцо.</param>
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

        _spectrum = e.NameScope.Find<AxColorSpectrum>(SpectrumPart);
        _hue = e.NameScope.Find<AxColorSlider>(HuePart);
        _alpha = e.NameScope.Find<AxColorSlider>(AlphaPart);
        _hex = e.NameScope.Find<TextBox>(HexPart);
        _channels = e.NameScope.Find<AxVectorField>(ChannelsPart);

        if (_hex is not null)
        {
            _hex.AddHandler(KeyDownEvent, OnHexKey, RoutingStrategies.Tunnel);
            _hex.LostFocus += OnHexBlur;
        }

        Label();
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

        var next = color.ToHsv();

        if (next.V <= 0)
            next = new HsvColor(next.A, current.H, current.S, 0);
        else if (next.S <= 0)
            next = new HsvColor(next.A, current.H, 0, next.V);

        SetCurrentValue(HsvProperty, next);
    }

    /// <summary>Модель HSV — в цвет и во все части.</summary>
    /// <param name="force">Обновить и запись, в которой стоит каретка: так при подъёме шаблона.</param>
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
            _hue?.SetCurrentValue(AxColorSlider.HsvProperty, hsv);
            _alpha?.SetCurrentValue(AxColorSlider.HsvProperty, hsv);
            _channels?.SetCurrentValue(AxVectorField.ValuesProperty, Channels(color));

            // Запись, которую человек набирает, не затирается: она станет цветом на Enter или уходе каретки.
            if (_hex is not null && (force || !_hex.IsKeyboardFocusWithin))
            {
                _hex.Text = AxColorField.Format(color, AllowsAlpha);
                AxValidation.SetState(_hex, AxValidationState.None);
            }
        }
        finally
        {
            _syncing = false;
        }
    }

    /// <summary>Каналы RGB — подписями и границами по прозрачности.</summary>
    private void Label()
    {
        if (_channels is null)
            return;

        _channels.Labels = AllowsAlpha ? ["R", "G", "B", "A"] : ["R", "G", "B"];
        _channels.Minimum = 0;
        _channels.Maximum = byte.MaxValue;
        _channels.Decimals = 0;
    }

    /// <summary>Цвет части — спектра или ползунка — становится цветом выбора; событие идёт дальше как есть.</summary>
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
        AxColorSlider slider when ReferenceEquals(slider, _hue) || ReferenceEquals(slider, _alpha) => slider.Hsv,
        _ => null,
    };

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
        else if (e.Key == Key.Escape && _hex.Text != AxColorField.Format(Color, AllowsAlpha))
        {
            Spread(force: true);
            e.Handled = true;
        }
    }

    private void OnHexBlur(object? sender, FocusChangedEventArgs e) => CommitHex();

    /// <summary>Набранная запись — в цвет; непрочитанная остаётся с отметкой ошибки.</summary>
    private void CommitHex()
    {
        if (_hex is null)
            return;

        var text = (_hex.Text ?? string.Empty).Trim();

        if (text == AxColorField.Format(Color, AllowsAlpha))
        {
            AxValidation.SetState(_hex, AxValidationState.None);
            return;
        }

        if (!AxColorField.TryParse(text, out var color))
        {
            AxValidation.SetState(_hex, AxValidationState.Error);
            return;
        }

        Adopt(color);
        Spread(force: true);
        RaiseEvent(new AxColorEventArgs(ColorCommittedEvent, Color, text));
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
