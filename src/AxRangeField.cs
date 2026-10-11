using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace ArxisStudio.Controls;

/// <summary>
/// Число в границах — ползунком и полем рядом, как <c>Range</c> у Unity: ползунок тянут и щёлкают по дорожке, а поле
/// правят, как <see cref="AxNumberField"/>, — выражениями, шагом стрелкой и протяжкой за подпись.
/// </summary>
/// <remarks>
/// <para>
/// События — те же, что у числового поля, и поднимают их части; поле только сводит их: протянутое ползунком видно в
/// поле, набранное в поле встаёт ползунком. На ходу — <see cref="AxNumberField.ValueChangingEvent"/>, а фиксируется одно
/// значение — при отпускании ползунка, шаге клавишей или наборе (<see cref="AxNumberField.ValueCommittedEvent"/>); Esc
/// посреди протяжки возвращает прежнее (<see cref="AxNumberField.ScrubCancelledEvent"/>), и разные значения у
/// выбранных — тоже.
/// </para>
/// <para>
/// Ползунок идёт делениями шага (<see cref="Step"/>): протяжка не пишет шум долей, а клавиши шагают на шаг, PageUp и
/// PageDown — на десять. Разные значения (<see cref="IsMixed"/>) — прочерк в поле, а ползунок стоит у значения
/// первого из выбранных, как у Unity.
/// </para>
/// <para>
/// Части поле заводит само и кладёт в панель шаблона: поле числа есть с самого начала — подпись строки свойства тянет
/// его (<see cref="NumberField"/>) раньше, чем поле встанет на экран.
/// </para>
/// </remarks>
[TemplatePart(HostPart, typeof(Panel))]
[PseudoClasses(":compact")]
public class AxRangeField : TemplatedControl
{
    /// <summary>Значение.</summary>
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<AxRangeField, double>(nameof(Value));

    /// <summary>Нижняя граница.</summary>
    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<AxRangeField, double>(nameof(Minimum));

    /// <summary>Верхняя граница.</summary>
    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<AxRangeField, double>(nameof(Maximum), 100d);

    /// <summary>Шаг: деление ползунка, шаг клавиши и мера протяжки поля.</summary>
    public static readonly StyledProperty<double> StepProperty =
        AxNumberField.StepProperty.AddOwner<AxRangeField>();

    /// <summary>Сколько знаков после точки показывает поле; пусто — сколько нужно, не больше трёх.</summary>
    public static readonly StyledProperty<int?> DecimalsProperty =
        AxNumberField.DecimalsProperty.AddOwner<AxRangeField>();

    /// <summary>Значения у выбранных расходятся: в поле прочерк, ползунок — у значения первого.</summary>
    public static readonly StyledProperty<bool> IsMixedProperty =
        AxNumberField.IsMixedProperty.AddOwner<AxRangeField>();

    /// <summary>Значение только читается: ползунок выключен, поле — выделяемый текст.</summary>
    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<AxRangeField, bool>(nameof(IsReadOnly));

    /// <summary>Размер поля: обычный или тесный — для строк инспектора.</summary>
    public static readonly StyledProperty<AxControlSize> SizeProperty =
        AxButton.SizeProperty.AddOwner<AxRangeField>();

    /// <summary>Имя панели частей в теме.</summary>
    private const string HostPart = "PART_Host";

    private readonly AxSlider _slider = new();
    private readonly AxNumberField _number = new();
    private Panel? _host;
    private (double Value, bool Mixed)? _before;
    private bool _syncing;

    static AxRangeField()
    {
        ValueProperty.Changed.AddClassHandler<AxRangeField>((field, _) => field.Sync());
        MinimumProperty.Changed.AddClassHandler<AxRangeField>((field, _) => field.Sync());
        MaximumProperty.Changed.AddClassHandler<AxRangeField>((field, _) => field.Sync());
        StepProperty.Changed.AddClassHandler<AxRangeField>((field, _) => field.Sync());
        DecimalsProperty.Changed.AddClassHandler<AxRangeField>((field, _) => field.Sync());
        IsMixedProperty.Changed.AddClassHandler<AxRangeField>((field, _) => field.Sync());
        IsReadOnlyProperty.Changed.AddClassHandler<AxRangeField>((field, _) => field.Sync());
        SizeProperty.Changed.AddClassHandler<AxRangeField>((field, change) =>
        {
            AxLook.Mark(field.PseudoClasses, change.GetNewValue<AxControlSize>());
            field.Sync();
        });
        AutomationProperties.NameProperty.Changed.AddClassHandler<AxRangeField>((field, _) => field.Announce());
    }

    /// <summary>Заводит поле с его частями: события частей слушаются здесь и выходят наружу как есть.</summary>
    public AxRangeField()
    {
        // Поле числа справа, ползунок — во всё остальное: порядок в панели — порядок стыковки.
        DockPanel.SetDock(_number, Dock.Right);

        AddHandler(AxNumberField.ValueChangingEvent, OnPartValue);
        AddHandler(AxNumberField.ValueCommittedEvent, OnPartValue);
        AddHandler(AxNumberField.ScrubCancelledEvent, OnPartCancelled);

        // Протяжка, вернувшаяся к началу, ничего не фиксирует, и память о «было» кончается с отпусканием ползунка.
        AddHandler(PointerReleasedEvent, OnReleased, RoutingStrategies.Bubble, handledEventsToo: true);

        Sync();
    }

    /// <inheritdoc cref="AxNumberField.ValueCommittedEvent"/>
    public event EventHandler<AxNumberEventArgs>? ValueCommitted
    {
        add => AddHandler(AxNumberField.ValueCommittedEvent, value);
        remove => RemoveHandler(AxNumberField.ValueCommittedEvent, value);
    }

    /// <inheritdoc cref="AxNumberField.ValueChangingEvent"/>
    public event EventHandler<AxNumberEventArgs>? ValueChanging
    {
        add => AddHandler(AxNumberField.ValueChangingEvent, value);
        remove => RemoveHandler(AxNumberField.ValueChangingEvent, value);
    }

    /// <inheritdoc cref="AxNumberField.ScrubCancelledEvent"/>
    public event EventHandler<RoutedEventArgs>? ScrubCancelled
    {
        add => AddHandler(AxNumberField.ScrubCancelledEvent, value);
        remove => RemoveHandler(AxNumberField.ScrubCancelledEvent, value);
    }

    /// <inheritdoc cref="AxNumberField.ClearedEvent"/>
    public event EventHandler<RoutedEventArgs>? Cleared
    {
        add => AddHandler(AxNumberField.ClearedEvent, value);
        remove => RemoveHandler(AxNumberField.ClearedEvent, value);
    }

    /// <inheritdoc cref="ValueProperty"/>
    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <inheritdoc cref="MinimumProperty"/>
    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// <inheritdoc cref="MaximumProperty"/>
    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    /// <inheritdoc cref="StepProperty"/>
    public double Step
    {
        get => GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    /// <inheritdoc cref="DecimalsProperty"/>
    public int? Decimals
    {
        get => GetValue(DecimalsProperty);
        set => SetValue(DecimalsProperty, value);
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

    /// <inheritdoc cref="SizeProperty"/>
    public AxControlSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Ползунок.</summary>
    public AxSlider Slider => _slider;

    /// <summary>Поле числа — его тянет подпись строки свойства.</summary>
    public AxNumberField NumberField => _number;

    /// <summary>Идёт протяжка — ползунком или полем.</summary>
    public bool IsScrubbing => _slider.IsScrubbing || _number.IsScrubbing;

    /// <summary>В поле набрано то, что ещё не стало значением.</summary>
    public bool HasDraft => _number.HasDraft;

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnApplyTemplate(e);

        _host?.Children.Clear();
        _host = e.NameScope.Find<Panel>(HostPart);
        _host?.Children.Add(_number);
        _host?.Children.Add(_slider);
        _before = null;
    }

    /// <summary>Часть сдвинула значение: поле берёт его и показывает в другой части; событие идёт дальше как есть.</summary>
    private void OnPartValue(object? sender, AxNumberEventArgs e)
    {
        var slid = ReferenceEquals(e.Source, _slider);

        if (!slid && !ReferenceEquals(e.Source, _number))
            return;

        // Протяжку ползунком можно отменить: помнится то, что было до её первого шага, — и разные значения тоже.
        if (slid && e.RoutedEvent == AxNumberField.ValueChangingEvent)
            _before ??= (Value, IsMixed);

        if (e.RoutedEvent == AxNumberField.ValueCommittedEvent)
            _before = null;

        Take(e.Value, !slid && _number.IsMixed);
    }

    /// <summary>Протяжку отменили: ползунок вернул своё, поле — своё, и разные значения остаются разными.</summary>
    private void OnPartCancelled(object? sender, RoutedEventArgs e)
    {
        if (ReferenceEquals(e.Source, _slider))
        {
            var (value, mixed) = _before ?? (_slider.Value, IsMixed);

            _before = null;
            Take(value, mixed);
        }
        else if (ReferenceEquals(e.Source, _number))
        {
            Take(_number.Value, _number.IsMixed);
        }
    }

    private void OnReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_slider.IsScrubbing)
            _before = null;
    }

    /// <summary>Ставит значение, пришедшее от части, и показывает его второй.</summary>
    private void Take(double value, bool mixed)
    {
        _syncing = true;

        try
        {
            SetCurrentValue(IsMixedProperty, mixed);
            SetCurrentValue(ValueProperty, value);
        }
        finally
        {
            _syncing = false;
        }

        Sync();
    }

    /// <summary>Ставит частям значение, границы, шаг и вид; часть посреди протяжки своё значение ведёт сама.</summary>
    private void Sync()
    {
        if (_syncing)
            return;

        _slider.Minimum = Minimum;
        _slider.Maximum = Maximum;
        _slider.SmallChange = Step;
        _slider.LargeChange = Step * 10;
        _slider.TickFrequency = Step;
        _slider.IsSnapToTickEnabled = Step > 0;
        _slider.IsEnabled = !IsReadOnly;

        if (!_slider.IsScrubbing)
            _slider.SetCurrentValue(RangeBase.ValueProperty, Value);

        _number.Minimum = Minimum;
        _number.Maximum = Maximum;
        _number.Step = Step;
        _number.Decimals = Decimals;
        _number.IsReadOnly = IsReadOnly;
        _number.Size = Size;

        if (!_number.IsScrubbing)
        {
            _number.SetCurrentValue(AxNumberField.ValueProperty, Value);
            _number.SetCurrentValue(AxNumberField.IsMixedProperty, IsMixed);
        }
    }

    /// <summary>Называет части диктору именем поля: ползунок и число — одно значение.</summary>
    private void Announce()
    {
        var name = AutomationProperties.GetName(this);

        foreach (var part in new Control[] { _slider, _number })
        {
            if (string.IsNullOrEmpty(name))
                part.ClearValue(AutomationProperties.NameProperty);
            else
                AutomationProperties.SetName(part, name);
        }
    }
}
