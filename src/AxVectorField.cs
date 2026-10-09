using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;

namespace ArxisStudio.Controls;

/// <summary>Составляющие вектора — в событии фиксации и протяжки.</summary>
/// <param name="routedEvent">Событие.</param>
/// <param name="values">Все составляющие после правки.</param>
/// <param name="index">Какая составляющая менялась; −1 — все разом: значения связаны.</param>
public sealed class AxVectorEventArgs(RoutedEvent routedEvent, IReadOnlyList<double> values, int index)
    : RoutedEventArgs(routedEvent)
{
    /// <summary>Все составляющие после правки.</summary>
    public IReadOnlyList<double> Values { get; } = values;

    /// <summary>Какая составляющая менялась; −1 — все разом: значения связаны.</summary>
    public int Index { get; } = index;
}

/// <summary>
/// Поле вектора: несколько чисел под общим именем — отступ, скругление, точка, размер, прямоугольник.
/// </summary>
/// <remarks>
/// <para>
/// Каждая составляющая — <see cref="AxNumberField"/> со своей подписью внутри (<see cref="Labels"/>): подпись
/// тянет число, как у Unity и Figma, а само поле понимает выражения и шаги стрелками. Ячейки стоят в ряд, а
/// в узкой колонке — поровну в несколько рядов (<see cref="AxVectorPanel"/>).
/// </para>
/// <para>
/// Правка одной составляющей поднимает <see cref="ValuesCommittedEvent"/> со всем вектором: так его пишут
/// одним значением — <c>Margin="4,8"</c>, а не четырьмя. Связанные значения (<see cref="IsLinked"/>) меняются
/// разом: правка любой ячейки ставит её число во все — «одинаково со всех сторон». Протяжка видна сразу
/// (<see cref="ValuesChangingEvent"/>), фиксируется при отпускании, Esc возвращает весь вектор.
/// </para>
/// <para>
/// Разные значения у нескольких выбранных — по составляющей: бит <c>i</c> в <see cref="MixedMask"/> — ячейка
/// <c>i</c> пуста и с прочерком. Составляющих — не больше 32.
/// </para>
/// </remarks>
[TemplatePart(CellsPart, typeof(Panel))]
[PseudoClasses(":compact")]
public class AxVectorField : TemplatedControl
{
    /// <summary>Подписи составляющих: X, Y; L, T, R, B. Их число — число ячеек.</summary>
    public static readonly StyledProperty<IReadOnlyList<string>?> LabelsProperty =
        AvaloniaProperty.Register<AxVectorField, IReadOnlyList<string>?>(nameof(Labels));

    /// <summary>Значения составляющих по порядку подписей; недостающие — 0.</summary>
    public static readonly StyledProperty<IReadOnlyList<double>?> ValuesProperty =
        AvaloniaProperty.Register<AxVectorField, IReadOnlyList<double>?>(nameof(Values));

    /// <summary>Какие составляющие расходятся у выбранных: бит <c>i</c> — ячейка <c>i</c>.</summary>
    public static readonly StyledProperty<int> MixedMaskProperty =
        AvaloniaProperty.Register<AxVectorField, int>(nameof(MixedMask));

    /// <summary>Нижняя граница каждой составляющей.</summary>
    public static readonly StyledProperty<double> MinimumProperty =
        AxNumberField.MinimumProperty.AddOwner<AxVectorField>();

    /// <summary>Верхняя граница каждой составляющей.</summary>
    public static readonly StyledProperty<double> MaximumProperty =
        AxNumberField.MaximumProperty.AddOwner<AxVectorField>();

    /// <summary>Шаг стрелки и мера протяжки.</summary>
    public static readonly StyledProperty<double> StepProperty =
        AxNumberField.StepProperty.AddOwner<AxVectorField>();

    /// <summary>Сколько знаков после точки; пусто — сколько нужно, не больше трёх.</summary>
    public static readonly StyledProperty<int?> DecimalsProperty =
        AxNumberField.DecimalsProperty.AddOwner<AxVectorField>();

    /// <summary>Значения связаны: правка любой ячейки ставит её число во все.</summary>
    public static readonly StyledProperty<bool> IsLinkedProperty =
        AvaloniaProperty.Register<AxVectorField, bool>(nameof(IsLinked), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Связь можно включить: тема показывает её переключатель.</summary>
    public static readonly StyledProperty<bool> CanLinkProperty =
        AvaloniaProperty.Register<AxVectorField, bool>(nameof(CanLink));

    /// <summary>Значения только читаются.</summary>
    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<AxVectorField, bool>(nameof(IsReadOnly));

    /// <summary>Размер ячеек: обычный или тесный — для строк инспектора.</summary>
    public static readonly StyledProperty<AxControlSize> SizeProperty =
        AxButton.SizeProperty.AddOwner<AxVectorField>();

    /// <summary>Составляющая зафиксирована: Enter, уход фокуса, шаг стрелкой или конец протяжки.</summary>
    public static readonly RoutedEvent<AxVectorEventArgs> ValuesCommittedEvent =
        RoutedEvent.Register<AxVectorField, AxVectorEventArgs>(nameof(ValuesCommitted), RoutingStrategies.Bubble);

    /// <summary>Составляющая меняется протяжкой: её видно, но она ещё не зафиксирована.</summary>
    public static readonly RoutedEvent<AxVectorEventArgs> ValuesChangingEvent =
        RoutedEvent.Register<AxVectorField, AxVectorEventArgs>(nameof(ValuesChanging), RoutingStrategies.Bubble);

    /// <summary>Протяжку отменили: вектор вернулся к тому, что был до неё.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> ScrubCancelledEvent =
        RoutedEvent.Register<AxVectorField, RoutedEventArgs>(nameof(ScrubCancelled), RoutingStrategies.Bubble);

    /// <summary>Имя панели ячеек в теме.</summary>
    private const string CellsPart = "PART_Cells";

    private readonly List<AxNumberField> _cells = [];
    private Panel? _panel;
    private double[]? _before;
    private int _beforeMask;

    static AxVectorField()
    {
        LabelsProperty.Changed.AddClassHandler<AxVectorField>((field, _) => field.Build());
        ValuesProperty.Changed.AddClassHandler<AxVectorField>((field, _) => field.Sync());
        MixedMaskProperty.Changed.AddClassHandler<AxVectorField>((field, _) => field.Sync());
        MinimumProperty.Changed.AddClassHandler<AxVectorField>((field, _) => field.Sync());
        MaximumProperty.Changed.AddClassHandler<AxVectorField>((field, _) => field.Sync());
        StepProperty.Changed.AddClassHandler<AxVectorField>((field, _) => field.Sync());
        DecimalsProperty.Changed.AddClassHandler<AxVectorField>((field, _) => field.Sync());
        IsReadOnlyProperty.Changed.AddClassHandler<AxVectorField>((field, _) => field.Sync());
        SizeProperty.Changed.AddClassHandler<AxVectorField>((field, change) =>
        {
            AxLook.Mark(field.PseudoClasses, change.GetNewValue<AxControlSize>());
            field.Sync();
        });
        AutomationProperties.NameProperty.Changed.AddClassHandler<AxVectorField>((field, _) => field.Announce());
    }

    /// <summary>Заводит поле: правки ячеек слушаются здесь и выходят наружу вектором.</summary>
    public AxVectorField()
    {
        AddHandler(AxNumberField.ValueChangingEvent, OnChanging);
        AddHandler(AxNumberField.ValueCommittedEvent, OnCommitted);
        AddHandler(AxNumberField.ScrubCancelledEvent, OnCancelled);
        AddHandler(AxNumberField.ClearedEvent, OnCleared);
    }

    /// <inheritdoc cref="ValuesCommittedEvent"/>
    public event EventHandler<AxVectorEventArgs>? ValuesCommitted
    {
        add => AddHandler(ValuesCommittedEvent, value);
        remove => RemoveHandler(ValuesCommittedEvent, value);
    }

    /// <inheritdoc cref="ValuesChangingEvent"/>
    public event EventHandler<AxVectorEventArgs>? ValuesChanging
    {
        add => AddHandler(ValuesChangingEvent, value);
        remove => RemoveHandler(ValuesChangingEvent, value);
    }

    /// <inheritdoc cref="ScrubCancelledEvent"/>
    public event EventHandler<RoutedEventArgs>? ScrubCancelled
    {
        add => AddHandler(ScrubCancelledEvent, value);
        remove => RemoveHandler(ScrubCancelledEvent, value);
    }

    /// <inheritdoc cref="LabelsProperty"/>
    public IReadOnlyList<string>? Labels
    {
        get => GetValue(LabelsProperty);
        set => SetValue(LabelsProperty, value);
    }

    /// <inheritdoc cref="ValuesProperty"/>
    public IReadOnlyList<double>? Values
    {
        get => GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }

    /// <inheritdoc cref="MixedMaskProperty"/>
    public int MixedMask
    {
        get => GetValue(MixedMaskProperty);
        set => SetValue(MixedMaskProperty, value);
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

    /// <inheritdoc cref="IsLinkedProperty"/>
    public bool IsLinked
    {
        get => GetValue(IsLinkedProperty);
        set => SetValue(IsLinkedProperty, value);
    }

    /// <inheritdoc cref="CanLinkProperty"/>
    public bool CanLink
    {
        get => GetValue(CanLinkProperty);
        set => SetValue(CanLinkProperty, value);
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

    /// <summary>Ячейки по порядку подписей: первая — куда ставить каретку.</summary>
    public IReadOnlyList<AxNumberField> Cells => _cells;

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnApplyTemplate(e);

        _panel?.Children.Clear();
        _panel = e.NameScope.Find<Panel>(CellsPart);
        Build();
    }

    /// <summary>Строит ячейки заново: по одной на подпись.</summary>
    private void Build()
    {
        _panel?.Children.Clear();
        _cells.Clear();
        _before = null;

        if (_panel is null)
            return;

        foreach (var label in Labels ?? [])
        {
            var cell = new AxNumberField();

            cell.InnerLeftContent = new AxScrubLabel { Content = label, Target = cell };
            _cells.Add(cell);
            _panel.Children.Add(cell);
        }

        Sync();
        Announce();
    }

    /// <summary>Ставит ячейкам значения, границы и вид поля.</summary>
    private void Sync()
    {
        var values = Current();

        for (var at = 0; at < _cells.Count; at++)
        {
            var cell = _cells[at];

            cell.Minimum = Minimum;
            cell.Maximum = Maximum;
            cell.Step = Step;
            cell.Decimals = Decimals;
            cell.IsReadOnly = IsReadOnly;
            cell.Size = Size;
            cell.Value = values[at];
            cell.IsMixed = at < 32 && (MixedMask & (1 << at)) != 0;
        }
    }

    /// <summary>
    /// Называет ячейки диктору: имя поля и подпись — «Margin L», а без имени поля — одна подпись.
    /// </summary>
    private void Announce()
    {
        var owner = AutomationProperties.GetName(this);
        var labels = Labels ?? [];

        for (var at = 0; at < _cells.Count && at < labels.Count; at++)
            AutomationProperties.SetName(_cells[at], string.IsNullOrEmpty(owner) ? labels[at] : $"{owner} {labels[at]}");
    }

    /// <summary>Значения по числу ячеек: недостающие — 0.</summary>
    private double[] Current()
    {
        var values = new double[_cells.Count];

        for (var at = 0; at < values.Length; at++)
            values[at] = Values is { } given && at < given.Count ? given[at] : 0;

        return values;
    }

    private void OnChanging(object? sender, AxNumberEventArgs e)
    {
        if (Index(e) is not { } index)
            return;

        e.Handled = true;

        // Начало протяжки — то, к чему вернёт Esc: связанные ячейки двигались вместе с тянутой.
        if (_before is null)
        {
            _before = Current();
            _beforeMask = MixedMask;
        }

        RaiseEvent(new AxVectorEventArgs(ValuesChangingEvent, Write(index, e.Value), IsLinked ? -1 : index));
    }

    private void OnCommitted(object? sender, AxNumberEventArgs e)
    {
        if (Index(e) is not { } index)
            return;

        e.Handled = true;
        _before = null;

        RaiseEvent(new AxVectorEventArgs(ValuesCommittedEvent, Write(index, e.Value), IsLinked ? -1 : index));
    }

    private void OnCancelled(object? sender, RoutedEventArgs e)
    {
        if (Index(e) is null)
            return;

        e.Handled = true;

        if (_before is { } before)
        {
            _before = null;
            SetCurrentValue(ValuesProperty, before);
            SetCurrentValue(MixedMaskProperty, _beforeMask);
        }

        RaiseEvent(new RoutedEventArgs(ScrubCancelledEvent));
    }

    /// <summary>Пустая составляющая вектора не снимается: ячейка просто возвращает своё число.</summary>
    private void OnCleared(object? sender, RoutedEventArgs e)
    {
        if (Index(e) is not null)
            e.Handled = true;
    }

    /// <summary>Ставит составляющую — или все, если значения связаны, — и отдаёт вектор целиком.</summary>
    private double[] Write(int index, double value)
    {
        var values = Current();

        if (IsLinked)
            Array.Fill(values, value);
        else
            values[index] = value;

        SetCurrentValue(ValuesProperty, values);
        SetCurrentValue(MixedMaskProperty, IsLinked ? 0 : index < 32 ? MixedMask & ~(1 << index) : MixedMask);

        return values;
    }

    /// <summary>Номер ячейки, из которой пришло событие; пусто — не из ячейки этого поля.</summary>
    private int? Index(RoutedEventArgs e)
    {
        var index = e.Source is AxNumberField cell ? _cells.IndexOf(cell) : -1;

        return index < 0 ? null : index;
    }
}
