using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace ArxisStudio.Controls;

/// <summary>Жетоны поля после правки — и что в ней добавилось и ушло.</summary>
/// <param name="routedEvent">Событие.</param>
/// <param name="tokens">Все жетоны после правки.</param>
/// <param name="added">Добавленные.</param>
/// <param name="removed">Снятые.</param>
public sealed class AxTokensChangedEventArgs(
    RoutedEvent routedEvent,
    IReadOnlyList<string> tokens,
    IReadOnlyList<string> added,
    IReadOnlyList<string> removed) : RoutedEventArgs(routedEvent)
{
    /// <summary>Все жетоны после правки.</summary>
    public IReadOnlyList<string> Tokens { get; } = tokens;

    /// <summary>Добавленные.</summary>
    public IReadOnlyList<string> Added { get; } = added;

    /// <summary>Снятые.</summary>
    public IReadOnlyList<string> Removed { get; } = removed;
}

/// <summary>
/// Поле жетонов: слова чипами и строка ввода за ними — классы стиля, теги, ключевые слова.
/// </summary>
/// <remarks>
/// <para>
/// <b>Набор.</b> Пробел, запятая и Enter превращают набранное в жетон; вставленное «primary large» становится
/// двумя. Повтор не добавляется. Набранное, но не завершённое, становится жетоном и на уходе каретки.
/// </para>
/// <para>
/// <b>Снятие.</b> Крестиком на жетоне — или с клавиатуры: Backspace в пустой строке ставит каретку на последний
/// жетон, ← и → ходят по жетонам, Delete и Backspace снимают тот, на котором каретка. Шаг через два нажатия
/// нарочно: одно лишнее нажатие Backspace не стирает то, что человек не собирался трогать.
/// </para>
/// <para>
/// Каждая правка поднимает <see cref="TokensChangedEvent"/> со всем списком: хозяин пишет его одним значением —
/// <c>Classes="primary large"</c>.
/// </para>
/// </remarks>
[TemplatePart(TokensPart, typeof(Panel))]
[TemplatePart(InputPart, typeof(TextBox))]
[PseudoClasses(":compact", ":editing", ":empty", ":readonly")]
public class AxTokenField : TemplatedControl
{
    /// <summary>Жетоны по порядку.</summary>
    public static readonly StyledProperty<IReadOnlyList<string>> TokensProperty =
        AvaloniaProperty.Register<AxTokenField, IReadOnlyList<string>>(
            nameof(Tokens), [], defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Подсказка в пустом поле.</summary>
    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<AxTokenField, string?>(nameof(PlaceholderText));

    /// <summary>Жетоны только читаются: строки ввода и крестиков нет.</summary>
    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<AxTokenField, bool>(nameof(IsReadOnly));

    /// <summary>Размер: обычный или тесный — для строк инспектора.</summary>
    public static readonly StyledProperty<AxControlSize> SizeProperty =
        AxButton.SizeProperty.AddOwner<AxTokenField>();

    /// <summary>Жетоны изменились правкой человека.</summary>
    public static readonly RoutedEvent<AxTokensChangedEventArgs> TokensChangedEvent =
        RoutedEvent.Register<AxTokenField, AxTokensChangedEventArgs>(nameof(TokensChanged), RoutingStrategies.Bubble);

    /// <summary>Имя панели жетонов в теме; строка ввода — её последний ребёнок.</summary>
    private const string TokensPart = "PART_Tokens";

    /// <summary>Имя строки ввода в теме.</summary>
    private const string InputPart = "PART_Input";

    private readonly List<AxChip> _chips = [];
    private Panel? _panel;
    private TextBox? _input;
    private bool _splitting;

    static AxTokenField()
    {
        TokensProperty.Changed.AddClassHandler<AxTokenField>((field, _) => field.Build());
        IsReadOnlyProperty.Changed.AddClassHandler<AxTokenField>((field, change) =>
        {
            field.PseudoClasses.Set(":readonly", change.GetNewValue<bool>());
            field.Build();
        });
        SizeProperty.Changed.AddClassHandler<AxTokenField>((field, change) =>
            AxLook.Mark(field.PseudoClasses, change.GetNewValue<AxControlSize>()));
        PlaceholderTextProperty.Changed.AddClassHandler<AxTokenField>((field, _) => field.Announce());
        AutomationProperties.NameProperty.Changed.AddClassHandler<AxTokenField>((field, _) => field.Announce());
    }

    /// <summary>Заводит пустое поле.</summary>
    public AxTokenField()
    {
        PseudoClasses.Set(":empty", true);
        AddHandler(AxChip.RemoveRequestedEvent, OnRemoveRequested);
    }

    /// <inheritdoc cref="TokensChangedEvent"/>
    public event EventHandler<AxTokensChangedEventArgs>? TokensChanged
    {
        add => AddHandler(TokensChangedEvent, value);
        remove => RemoveHandler(TokensChangedEvent, value);
    }

    /// <inheritdoc cref="TokensProperty"/>
    public IReadOnlyList<string> Tokens
    {
        get => GetValue(TokensProperty);
        set => SetValue(TokensProperty, value);
    }

    /// <inheritdoc cref="PlaceholderTextProperty"/>
    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
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

    /// <summary>Строка ввода: куда ставить каретку.</summary>
    public TextBox? Input => _input;

    /// <summary>Делит набранное на жетоны: пробелы и запятые — разделители, пустые части пропадают.</summary>
    /// <param name="text">Набранное.</param>
    public static IReadOnlyList<string> Split(string? text)
    {
        var parts = new List<string>();

        if (string.IsNullOrEmpty(text))
            return parts;

        var start = 0;

        for (var at = 0; at <= text.Length; at++)
        {
            if (at < text.Length && !IsSeparator(text[at]))
                continue;

            if (at > start)
                parts.Add(text[start..at]);

            start = at + 1;
        }

        return parts;
    }

    /// <summary>Добавляет жетоны, кроме тех, что уже есть.</summary>
    /// <param name="tokens">Жетоны.</param>
    /// <returns>Добавилось ли хоть что-то.</returns>
    public bool Add(IEnumerable<string> tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);

        var list = Tokens.ToList();
        var added = new List<string>();

        foreach (var token in tokens.SelectMany(Split))
        {
            if (list.Contains(token, StringComparer.Ordinal))
                continue;

            list.Add(token);
            added.Add(token);
        }

        if (added.Count == 0)
            return false;

        Change(list, added, []);
        return true;
    }

    /// <summary>Снимает жетон.</summary>
    /// <param name="token">Жетон.</param>
    /// <returns>Был ли он.</returns>
    public bool Remove(string token)
    {
        var list = Tokens.ToList();

        if (!list.Remove(token))
            return false;

        Change(list, [], [token]);
        return true;
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnApplyTemplate(e);

        if (_input is not null)
        {
            _input.RemoveHandler(KeyDownEvent, OnInputKey);
            _input.TextChanged -= OnInputText;
            _input.GotFocus -= OnInputFocus;
            _input.LostFocus -= OnInputBlur;
        }

        foreach (var chip in _chips)
            _panel?.Children.Remove(chip);

        _chips.Clear();
        _panel = e.NameScope.Find<Panel>(TokensPart);
        _input = e.NameScope.Find<TextBox>(InputPart);

        if (_input is not null)
        {
            _input.AddHandler(KeyDownEvent, OnInputKey, RoutingStrategies.Tunnel);
            _input.TextChanged += OnInputText;
            _input.GotFocus += OnInputFocus;
            _input.LostFocus += OnInputBlur;
        }

        Build();
        Announce();
    }

    /// <inheritdoc/>
    /// <remarks>Щелчок по полю мимо жетонов — щелчок по строке ввода: человек целился в поле, а не в зазор.</remarks>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnPointerPressed(e);

        if (e.Handled || IsReadOnly || _input is null)
            return;

        _input.Focus(NavigationMethod.Pointer);
        _input.CaretIndex = _input.Text?.Length ?? 0;
        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        if (e.Source is AxChip chip && _chips.IndexOf(chip) is var at and >= 0 && e.KeyModifiers == KeyModifiers.None)
        {
            switch (e.Key)
            {
                case Key.Left:
                    Visit(Math.Max(0, at - 1));
                    e.Handled = true;
                    return;

                case Key.Right:
                    Visit(at + 1);
                    e.Handled = true;
                    return;

                case Key.Escape:
                    Visit(_chips.Count);
                    e.Handled = true;
                    return;
            }
        }

        base.OnKeyDown(e);
    }

    /// <summary>Ставит чипы по жетонам — перед строкой ввода.</summary>
    private void Build()
    {
        PseudoClasses.Set(":empty", Tokens.Count == 0);

        if (_panel is null)
            return;

        foreach (var chip in _chips)
            _panel.Children.Remove(chip);

        _chips.Clear();

        foreach (var token in Tokens)
        {
            // Чип берёт каретку только стрелками из поля и только на это время: Tab ходит мимо жетонов, а
            // нажатие на крестик фокус отдаёт ближайшему фокусируемому предку — им стал бы сам чип, и
            // каретка ушла бы из строки ввода, куда человек дописывает.
            var chip = new AxChip
            {
                Content = token,
                IsRemovable = !IsReadOnly,
                Focusable = false,
                IsTabStop = false,
                VerticalAlignment = VerticalAlignment.Center,
            };

            chip.LostFocus += OnChipBlur;
            _chips.Add(chip);
        }

        var input = _input is not null ? _panel.Children.IndexOf(_input) : -1;

        _panel.Children.InsertRange(input < 0 ? _panel.Children.Count : input, _chips);
    }

    /// <summary>Называет строку ввода диктору: именем поля, а без него — подсказкой.</summary>
    private void Announce()
    {
        if (_input is null)
            return;

        var name = AutomationProperties.GetName(this);

        AutomationProperties.SetName(_input, string.IsNullOrEmpty(name) ? PlaceholderText : name);
    }

    private void Change(List<string> tokens, IReadOnlyList<string> added, IReadOnlyList<string> removed)
    {
        SetCurrentValue(TokensProperty, tokens.ToArray());
        RaiseEvent(new AxTokensChangedEventArgs(TokensChangedEvent, Tokens, added, removed));
    }

    /// <summary>Каретка на жетон с этим номером; за последним — в строку ввода.</summary>
    private void Visit(int index)
    {
        if (index < _chips.Count && !IsReadOnly)
        {
            _chips[index].Focusable = true;
            _chips[index].Focus(NavigationMethod.Directional);
        }
        else
        {
            _input?.Focus(NavigationMethod.Directional);
        }
    }

    /// <summary>Каретка ушла с жетона — он снова не берёт её мышью.</summary>
    private static void OnChipBlur(object? sender, FocusChangedEventArgs e)
    {
        if (sender is AxChip chip)
            chip.Focusable = false;
    }

    private void OnRemoveRequested(object? sender, RoutedEventArgs e)
    {
        var at = e.Source is AxChip source ? _chips.IndexOf(source) : -1;

        if (at < 0 || IsReadOnly)
            return;

        var chip = _chips[at];

        e.Handled = true;

        var hadFocus = chip.IsKeyboardFocusWithin;

        if (chip.Content is not string token || !Remove(token))
            return;

        // Снятый с клавиатуры отдаёт каретку соседу слева, а первый — строке ввода; крестик каретку не брал.
        if (hadFocus)
            Visit(at > 0 ? at - 1 : 0);
    }

    private void OnInputKey(object? sender, KeyEventArgs e)
    {
        if (_input is null || e.KeyModifiers != KeyModifiers.None)
            return;

        var atStart = _input.CaretIndex == 0 && _input.SelectionStart == _input.SelectionEnd;

        switch (e.Key)
        {
            case Key.Enter:
                Flush();
                e.Handled = true;
                break;

            case Key.Escape when !string.IsNullOrEmpty(_input.Text):
                _input.Text = string.Empty;
                e.Handled = true;
                break;

            case Key.Back when atStart && _chips.Count > 0:
            case Key.Left when atStart && _chips.Count > 0:
                Visit(_chips.Count - 1);
                e.Handled = true;
                break;
        }
    }

    /// <summary>Разделитель в набранном — граница жетона: всё до последнего становится жетонами.</summary>
    private void OnInputText(object? sender, TextChangedEventArgs e)
    {
        if (_splitting || _input?.Text is not { } text)
            return;

        var last = text.Length - 1;

        while (last >= 0 && !IsSeparator(text[last]))
            last--;

        if (last < 0)
            return;

        var rest = text[(last + 1)..];

        _splitting = true;

        try
        {
            Add(Split(text[..last]));
            _input.Text = rest;
            _input.CaretIndex = rest.Length;
        }
        finally
        {
            _splitting = false;
        }
    }

    private void OnInputFocus(object? sender, FocusChangedEventArgs e) => PseudoClasses.Set(":editing", true);

    private void OnInputBlur(object? sender, FocusChangedEventArgs e)
    {
        PseudoClasses.Set(":editing", false);
        Flush();
    }

    /// <summary>Набранное — в жетоны, строка пустеет.</summary>
    private void Flush()
    {
        if (_input is null || string.IsNullOrWhiteSpace(_input.Text))
            return;

        var text = _input.Text;

        _input.Text = string.Empty;
        Add(Split(text));
    }

    private static bool IsSeparator(char letter) => letter == ',' || char.IsWhiteSpace(letter);
}
