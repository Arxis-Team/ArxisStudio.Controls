using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace ArxisStudio.Controls;

/// <summary>Значение числового поля — в событии фиксации и протяжки.</summary>
/// <param name="routedEvent">Событие.</param>
/// <param name="value">Значение; <see cref="double.NaN"/> — «авто» у поля, которое его разрешает.</param>
public sealed class AxNumberEventArgs(RoutedEvent routedEvent, double value) : RoutedEventArgs(routedEvent)
{
    /// <summary>Значение.</summary>
    public double Value { get; } = value;
}

/// <summary>
/// Числовое поле: текст, который читается числом, шагает стрелками и протягивается мышью за подпись.
/// </summary>
/// <remarks>
/// <para>
/// <b>Фиксация.</b> Набранное становится значением на Enter и на уходе фокуса — тогда поле поднимает
/// <see cref="ValueCommittedEvent"/>, — а Esc возвращает прежнее. Поле понимает выражение (<c>2*8</c>,
/// <c>(10+4)/2</c>), запятую на месте точки и, если разрешено «авто» (<see cref="AllowsAuto"/>), слово
/// <see cref="AutoText"/> или пустое. Пустое у поля без «авто» — просьба снять значение:
/// <see cref="ClearedEvent"/>. Значение пишется инвариантно, с точкой: поле правит текст разметки, а не
/// число на экране.
/// </para>
/// <para>
/// <b>Шаг с клавиатуры.</b> ↑ и ↓ меняют значение на <see cref="Step"/> и сразу фиксируют его; Shift — в десять
/// раз крупнее, Alt — в десять раз мельче. Это замена протяжки для того, кто не тянет мышью.
/// </para>
/// <para>
/// <b>Протяжка</b> — как у Unity: подпись (<see cref="AxScrubLabel"/>) зовёт <see cref="BeginScrub"/>,
/// <see cref="ScrubBy"/> и <see cref="EndScrub"/>. На каждом движении значение видно сразу
/// (<see cref="ValueChangingEvent"/>), а фиксируется одно — при отпускании; Esc протяжку отменяет
/// (<see cref="ScrubCancelledEvent"/>). Шаг на точку — <c>max(1, √|v|) · 0,03 · Step</c> от значения в начале
/// протяжки: мелкое значение тянется тонко, крупное — быстро. Shift — вчетверо быстрее, Alt — вчетверо тоньше.
/// </para>
/// <para>
/// <b>Разные значения.</b> У нескольких выбранных объектов значения могут расходиться: тогда
/// <see cref="IsMixed"/>, поле пусто, а тема показывает прочерк. <see cref="Value"/> при этом — значение
/// первого из выбранных: от него идут шаг и протяжка, как у Unity. Набранное или протянутое ляжет на все.
/// </para>
/// <para>
/// <b>Набранное не затирается.</b> Пока в поле набрано своё и каретка в нём, новое значение снаружи текста
/// не трогает: хозяин обновляет поле когда хочет, а человек дописывает то, что начал.
/// </para>
/// </remarks>
[PseudoClasses(":mixed", ":scrubbing")]
public class AxNumberField : AxTextBox
{
    /// <summary>Значение поля.</summary>
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<AxNumberField, double>(nameof(Value));

    /// <summary>Нижняя граница.</summary>
    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<AxNumberField, double>(nameof(Minimum), double.NegativeInfinity);

    /// <summary>Верхняя граница.</summary>
    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<AxNumberField, double>(nameof(Maximum), double.PositiveInfinity);

    /// <summary>Шаг стрелки и мера протяжки.</summary>
    public static readonly StyledProperty<double> StepProperty =
        AvaloniaProperty.Register<AxNumberField, double>(nameof(Step), 1d);

    /// <summary>Сколько знаков после точки; null — сколько нужно, не больше трёх.</summary>
    public static readonly StyledProperty<int?> DecimalsProperty =
        AvaloniaProperty.Register<AxNumberField, int?>(nameof(Decimals));

    /// <summary>Значение может быть «авто» — <see cref="double.NaN"/>: так Avalonia пишет размер по содержимому.</summary>
    public static readonly StyledProperty<bool> AllowsAutoProperty =
        AvaloniaProperty.Register<AxNumberField, bool>(nameof(AllowsAuto));

    /// <summary>Как поле пишет и читает «авто».</summary>
    public static readonly StyledProperty<string> AutoTextProperty =
        AvaloniaProperty.Register<AxNumberField, string>(nameof(AutoText), "Auto");

    /// <summary>Значения у выбранных расходятся: поле пусто, тема показывает прочерк.</summary>
    public static readonly StyledProperty<bool> IsMixedProperty =
        AvaloniaProperty.Register<AxNumberField, bool>(nameof(IsMixed));

    /// <summary>Значение зафиксировано: Enter, уход фокуса, шаг стрелкой или конец протяжки.</summary>
    public static readonly RoutedEvent<AxNumberEventArgs> ValueCommittedEvent =
        RoutedEvent.Register<AxNumberField, AxNumberEventArgs>(nameof(ValueCommitted), RoutingStrategies.Bubble);

    /// <summary>Значение меняется протяжкой: его видно, но оно ещё не зафиксировано.</summary>
    public static readonly RoutedEvent<AxNumberEventArgs> ValueChangingEvent =
        RoutedEvent.Register<AxNumberField, AxNumberEventArgs>(nameof(ValueChanging), RoutingStrategies.Bubble);

    /// <summary>Протяжку отменили: значение вернулось к тому, что было до неё.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> ScrubCancelledEvent =
        RoutedEvent.Register<AxNumberField, RoutedEventArgs>(nameof(ScrubCancelled), RoutingStrategies.Bubble);

    /// <summary>Поле очистили у поля без «авто»: значение просят снять.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> ClearedEvent =
        RoutedEvent.Register<AxNumberField, RoutedEventArgs>(nameof(Cleared), RoutingStrategies.Bubble);

    private bool _scrubbing;
    private double _scrubStart;
    private double _scrubOrigin;
    private bool _scrubMixed;
    private double _scrubbed;
    private bool _writing;
    private bool _dirty;
    private bool _invalid;

    static AxNumberField()
    {
        ValueProperty.Changed.AddClassHandler<AxNumberField>((field, _) => field.Show());
        DecimalsProperty.Changed.AddClassHandler<AxNumberField>((field, _) => field.Show());
        AutoTextProperty.Changed.AddClassHandler<AxNumberField>((field, _) => field.Show());
        IsMixedProperty.Changed.AddClassHandler<AxNumberField>((field, change) =>
        {
            field.PseudoClasses.Set(":mixed", change.GetNewValue<bool>());
            field.Show();
        });
    }

    /// <summary>Заводит поле со значением 0.</summary>
    public AxNumberField() => Show();

    /// <inheritdoc cref="ValueCommittedEvent"/>
    public event EventHandler<AxNumberEventArgs>? ValueCommitted
    {
        add => AddHandler(ValueCommittedEvent, value);
        remove => RemoveHandler(ValueCommittedEvent, value);
    }

    /// <inheritdoc cref="ValueChangingEvent"/>
    public event EventHandler<AxNumberEventArgs>? ValueChanging
    {
        add => AddHandler(ValueChangingEvent, value);
        remove => RemoveHandler(ValueChangingEvent, value);
    }

    /// <inheritdoc cref="ScrubCancelledEvent"/>
    public event EventHandler<RoutedEventArgs>? ScrubCancelled
    {
        add => AddHandler(ScrubCancelledEvent, value);
        remove => RemoveHandler(ScrubCancelledEvent, value);
    }

    /// <inheritdoc cref="ClearedEvent"/>
    public event EventHandler<RoutedEventArgs>? Cleared
    {
        add => AddHandler(ClearedEvent, value);
        remove => RemoveHandler(ClearedEvent, value);
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

    /// <inheritdoc cref="AllowsAutoProperty"/>
    public bool AllowsAuto
    {
        get => GetValue(AllowsAutoProperty);
        set => SetValue(AllowsAutoProperty, value);
    }

    /// <inheritdoc cref="AutoTextProperty"/>
    public string AutoText
    {
        get => GetValue(AutoTextProperty);
        set => SetValue(AutoTextProperty, value);
    }

    /// <inheritdoc cref="IsMixedProperty"/>
    public bool IsMixed
    {
        get => GetValue(IsMixedProperty);
        set => SetValue(IsMixedProperty, value);
    }

    /// <summary>Идёт протяжка.</summary>
    public bool IsScrubbing => _scrubbing;

    /// <summary>В поле набрано то, что ещё не стало значением.</summary>
    public bool HasDraft => !_scrubbing && _dirty;

    /// <summary>Можно ли тянуть поле: оно включено и не только для чтения.</summary>
    public bool CanScrub => IsEffectivelyEnabled && !IsReadOnly;

    /// <summary>Значение текстом — так, как его пишет поле: инвариантно, с точкой.</summary>
    /// <param name="value">Значение.</param>
    /// <param name="decimals">Сколько знаков после точки; null — сколько нужно, не больше трёх.</param>
    /// <param name="auto">Как писать <see cref="double.NaN"/>.</param>
    public static string Format(double value, int? decimals = null, string auto = "Auto")
    {
        if (double.IsNaN(value))
            return auto;

        var digits = Math.Clamp(decimals ?? 3, 0, 15);

        return value.ToString(digits == 0 ? "0" : "0." + new string('#', digits), CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Читает число или выражение: <c>12.5</c>, <c>12,5</c>, <c>2*8</c>, <c>(10+4)/2</c>, <c>-3</c>.
    /// </summary>
    /// <param name="text">Текст.</param>
    /// <param name="value">Значение; не число — если не прочлось.</param>
    /// <returns><c>true</c>, если прочлось конечное число.</returns>
    /// <remarks>
    /// Запятая — разделитель дробной части, только если точки в тексте нет: «1,5» по-русски и «1.5»
    /// инвариантно значат одно, а смешивать их негде.
    /// </remarks>
    public static bool TryEvaluate(string? text, out double value)
    {
        value = double.NaN;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        var source = text.Contains('.', StringComparison.Ordinal) ? text : text.Replace(',', '.');

        try
        {
            var parser = new Expression(source);
            var result = parser.Sum();

            if (!parser.AtEnd || !double.IsFinite(result))
                return false;

            value = result;
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>Фиксирует набранное: значение, «авто», просьба снять или отказ.</summary>
    /// <returns><c>true</c>, если набранное прочлось — или было нечего фиксировать.</returns>
    /// <remarks>
    /// Непрочитанное остаётся в поле с отметкой ошибки (<see cref="AxValidation"/>): человек видит, что
    /// не так, и правит, а не набирает заново.
    /// </remarks>
    public bool Commit()
    {
        if (_scrubbing || !_dirty)
            return true;

        var text = (Text ?? string.Empty).Trim();

        if (text.Length == 0 && !AllowsAuto)
        {
            RaiseEvent(new RoutedEventArgs(ClearedEvent));

            // Хозяин снял значение и поставил то, что осталось, — или не стал: тогда возвращается прежнее.
            Show(force: true);

            return true;
        }

        double value;

        if (AllowsAuto && (text.Length == 0 || IsAuto(text)))
        {
            value = double.NaN;
        }
        else if (TryEvaluate(text, out var read))
        {
            value = Fit(read);
        }
        else
        {
            _invalid = true;
            AxValidation.SetState(this, AxValidationState.Error);
            return false;
        }

        Settle(value);

        return true;
    }

    /// <summary>Возвращает в поле значение — набранное пропадает.</summary>
    public void Revert() => Show(force: true);

    /// <summary>Начинает протяжку от нынешнего значения.</summary>
    /// <returns><c>false</c> — тянуть нечего: поле выключено, только для чтения или протяжка уже идёт.</returns>
    public bool BeginScrub()
    {
        if (_scrubbing || !CanScrub)
            return false;

        // Набранное, но не зафиксированное, — то, от чего человек тянет: оно и становится началом.
        if (HasDraft && !Commit())
            Revert();

        _scrubbing = true;
        _scrubOrigin = Value;
        _scrubMixed = IsMixed;
        _scrubStart = double.IsNaN(Value) ? 0 : Value;
        _scrubbed = 0;
        PseudoClasses.Set(":scrubbing", true);

        return true;
    }

    /// <summary>Тянет значение на столько точек экрана.</summary>
    /// <param name="pixels">Сдвиг указателя по горизонтали с прошлого раза.</param>
    /// <param name="modifiers">Клавиши: Shift — быстрее, Alt — тоньше.</param>
    public void ScrubBy(double pixels, KeyModifiers modifiers)
    {
        if (!_scrubbing || !double.IsFinite(pixels))
            return;

        _scrubbed += pixels * Sensitivity(modifiers);

        var value = Fit(Round(_scrubStart + _scrubbed, Sensitivity(KeyModifiers.None)));

        if (value.Equals(Value) && !IsMixed)
            return;

        _writing = true;

        try
        {
            SetCurrentValue(IsMixedProperty, false);
            SetCurrentValue(ValueProperty, value);
        }
        finally
        {
            _writing = false;
        }

        Show(force: true);
        RaiseEvent(new AxNumberEventArgs(ValueChangingEvent, value));
    }

    /// <summary>Кончает протяжку.</summary>
    /// <param name="commit"><c>true</c> — зафиксировать протянутое; <c>false</c> — вернуть прежнее.</param>
    public void EndScrub(bool commit)
    {
        if (!_scrubbing)
            return;

        _scrubbing = false;
        PseudoClasses.Set(":scrubbing", false);

        if (commit)
        {
            if (_scrubbed != 0)
                RaiseEvent(new AxNumberEventArgs(ValueCommittedEvent, Value));

            return;
        }

        // Возвращается то, что было, а не число, от которого тянули: «авто» остаётся «авто», разные — разными.
        _writing = true;

        try
        {
            SetCurrentValue(ValueProperty, _scrubOrigin);
            SetCurrentValue(IsMixedProperty, _scrubMixed);
        }
        finally
        {
            _writing = false;
        }

        Show(force: true);
        RaiseEvent(new RoutedEventArgs(ScrubCancelledEvent));
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        switch (e.Key)
        {
            case Key.Up or Key.Down when !IsReadOnly && e.KeyModifiers is KeyModifiers.None or KeyModifiers.Shift or KeyModifiers.Alt:
                StepBy(e.Key == Key.Up ? 1 : -1, e.KeyModifiers);
                e.Handled = true;
                return;

            case Key.Enter when !IsReadOnly:
                Commit();
                e.Handled = true;
                return;

            case Key.Escape when _scrubbing:
                EndScrub(commit: false);
                e.Handled = true;
                return;

            case Key.Escape when HasDraft:
                Revert();
                e.Handled = true;
                return;
        }

        base.OnKeyDown(e);
    }

    /// <inheritdoc/>
    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);

        if (!IsReadOnly)
            Commit();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        ArgumentNullException.ThrowIfNull(change);

        base.OnPropertyChanged(change);

        // Набранное — текст, разошедшийся с тем, что поле написало бы о значении; показ снимает отметку сам.
        if (change.Property == TextProperty)
            _dirty = !string.Equals(Text ?? string.Empty, Shown(), StringComparison.Ordinal);
    }

    /// <summary>Шаг стрелкой: значение меняется и сразу фиксируется.</summary>
    private void StepBy(int direction, KeyModifiers modifiers)
    {
        // Набранное, но не зафиксированное, — отправная точка шага: человек набрал «10» и жмёт ↑.
        if (HasDraft && !Commit())
            return;

        var step = Step * (modifiers.HasFlag(KeyModifiers.Shift) ? 10 : modifiers.HasFlag(KeyModifiers.Alt) ? 0.1 : 1);
        var start = double.IsNaN(Value) ? 0 : Value;

        Settle(Fit(Round(start + direction * step, Math.Abs(step))));
    }

    /// <summary>Ставит значение, показывает его и говорит о фиксации.</summary>
    private void Settle(double value)
    {
        _writing = true;

        try
        {
            SetCurrentValue(IsMixedProperty, false);
            SetCurrentValue(ValueProperty, value);
        }
        finally
        {
            _writing = false;
        }

        Show(force: true);
        RaiseEvent(new AxNumberEventArgs(ValueCommittedEvent, value));
    }

    /// <summary>Мера протяжки: сколько значения на точку экрана.</summary>
    private double Sensitivity(KeyModifiers modifiers)
    {
        var rate = Math.Max(1, Math.Sqrt(Math.Abs(_scrubStart))) * 0.03 * (Step > 0 ? Step : 1);

        if (modifiers.HasFlag(KeyModifiers.Shift))
            rate *= 4;

        if (modifiers.HasFlag(KeyModifiers.Alt))
            rate *= 0.25;

        return rate;
    }

    /// <summary>Округляет до знаков поля, а без них — до точности шага: протяжка не пишет шум долей.</summary>
    private double Round(double value, double precision)
    {
        if (Decimals is { } decimals)
            return Math.Round(value, Math.Clamp(decimals, 0, 15), MidpointRounding.AwayFromZero);

        var digits = precision >= 1 ? 0 : Math.Clamp((int)Math.Ceiling(-Math.Log10(precision)), 0, 6);

        return Math.Round(value, digits, MidpointRounding.AwayFromZero);
    }

    /// <summary>Значение в границах поля; «авто» границ не знает.</summary>
    private double Fit(double value) =>
        double.IsNaN(value) ? value : Math.Clamp(value, Math.Min(Minimum, Maximum), Math.Max(Minimum, Maximum));

    /// <summary>
    /// Пишет в поле значение — если человек не набирает там своё; <paramref name="force"/> — всегда.
    /// </summary>
    /// <remarks>Ошибку, поставленную самим полем, показ снимает: она была о набранном, а его больше нет.</remarks>
    private void Show(bool force = false)
    {
        if (_writing)
            return;

        if (!force && _dirty && IsKeyboardFocusWithin)
            return;

        SetCurrentValue(TextProperty, Shown());
        _dirty = false;

        if (_invalid)
        {
            _invalid = false;
            AxValidation.SetState(this, AxValidationState.None);
        }
    }

    /// <summary>Что поле пишет о значении: пусто — у разных значений.</summary>
    private string Shown() => IsMixed ? string.Empty : Format(Value, Decimals, AutoText);

    private bool IsAuto(string text) =>
        string.Equals(text, AutoText, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(text, "auto", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(text, "NaN", StringComparison.OrdinalIgnoreCase);

    /// <summary>Разбор выражения: сумма произведений чисел и скобок, унарный минус.</summary>
    private sealed class Expression(string text)
    {
        private int _at;

        public bool AtEnd
        {
            get
            {
                Skip();
                return _at >= text.Length;
            }
        }

        public double Sum()
        {
            var value = Product();

            while (true)
            {
                if (Take('+'))
                    value += Product();
                else if (Take('-'))
                    value -= Product();
                else
                    return value;
            }
        }

        private double Product()
        {
            var value = Unary();

            while (true)
            {
                if (Take('*'))
                    value *= Unary();
                else if (Take('/'))
                    value /= Unary();
                else
                    return value;
            }
        }

        private double Unary()
        {
            if (Take('-'))
                return -Unary();

            if (Take('+'))
                return Unary();

            if (Take('('))
            {
                var inner = Sum();

                if (!Take(')'))
                    throw new FormatException();

                return inner;
            }

            return Number();
        }

        private double Number()
        {
            Skip();

            var start = _at;

            while (_at < text.Length && (char.IsAsciiDigit(text[_at]) || text[_at] == '.' ||
                                         (text[_at] is 'e' or 'E' && _at > start) ||
                                         (text[_at] is '+' or '-' && _at > start && text[_at - 1] is 'e' or 'E')))
            {
                _at++;
            }

            if (_at == start ||
                !double.TryParse(text.AsSpan(start, _at - start), NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                throw new FormatException();
            }

            return value;
        }

        private bool Take(char symbol)
        {
            if (!Peek(symbol))
                return false;

            _at++;
            return true;
        }

        private bool Peek(char symbol)
        {
            Skip();
            return _at < text.Length && text[_at] == symbol;
        }

        private void Skip()
        {
            while (_at < text.Length && char.IsWhiteSpace(text[_at]))
                _at++;
        }
    }
}
