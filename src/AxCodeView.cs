using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace ArxisStudio.Controls;

/// <summary>
/// Просмотр кода: текст моноширинным шрифтом с подсветкой по ролям, номерами строк, кареткой и
/// выделением — без правки.
/// </summary>
/// <remarks>
/// <para>
/// Текст разбирает не контрол, а его хозяин: он знает формат и отдаёт куски с ролями —
/// <see cref="Spans"/>. Контрол красит роль кистью темы и о синтаксисе не знает ничего. Так вид
/// разметки подсвечивается по тому же синтаксическому дереву, с которым работает его хозяин, а не по
/// второму разбору, который разошёлся бы с первым на первом же расширении разметки.
/// </para>
/// <para>
/// Строятся только видимые строки, поэтому документ в десятки тысяч строк стоит столько же, сколько
/// экран. Для блока в пару строк есть <see cref="AxCodeBlock"/>: он проще и выделяется как текст.
/// </para>
/// <para>
/// Отметка <see cref="Highlight"/> — то, что показывает хозяин: элемент, выбранный на холсте. Её
/// видно и без фокуса — заливкой под текстом и меткой у края строк. Выделение — то, что отметил
/// человек мышью или клавиатурой; его копируют. Одно другому не мешает.
/// </para>
/// <para>
/// О каретке, поставленной человеком, говорит <see cref="CaretMoved"/>; поставленная кодом события
/// не поднимает, и хозяин, отвечающий на событие отметкой или кареткой, не ходит по кругу.
/// </para>
/// </remarks>
[TemplatePart("PART_ScrollViewer", typeof(ScrollViewer))]
[PseudoClasses(":selection-active")]
public class AxCodeView : TemplatedControl
{
    /// <summary>Текст кода.</summary>
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<AxCodeView, string?>(nameof(Text));

    /// <summary>Куски подсветки: смещение, длина и роль; формат знает хозяин.</summary>
    public static readonly StyledProperty<IReadOnlyList<AxCodeSpan>?> SpansProperty =
        AvaloniaProperty.Register<AxCodeView, IReadOnlyList<AxCodeSpan>?>(nameof(Spans));

    /// <summary>Отметка хозяина: отрезок, который показывают заливкой и меткой у края строк.</summary>
    public static readonly StyledProperty<AxCodeRange?> HighlightProperty =
        AvaloniaProperty.Register<AxCodeView, AxCodeRange?>(nameof(Highlight));

    /// <summary>Смещение каретки в знаках UTF-16 от начала текста.</summary>
    /// <remarks>
    /// Значение встаёт туда, где каретка может стоять: в пределы текста, не внутрь перевода строки и
    /// не между половинами суррогатной пары. Поставленное кодом снимает выделение.
    /// </remarks>
    public static readonly StyledProperty<int> CaretOffsetProperty =
        AvaloniaProperty.Register<AxCodeView, int>(
            nameof(CaretOffset), defaultBindingMode: BindingMode.TwoWay, coerce: (view, offset) => ((AxCodeView)view)._text.Snap(offset));

    /// <summary>Выделение: от места, где его начали, до каретки, в порядке возрастания.</summary>
    public static readonly DirectProperty<AxCodeView, AxCodeRange> SelectionProperty =
        AvaloniaProperty.RegisterDirect<AxCodeView, AxCodeRange>(nameof(Selection), view => view.Selection);

    /// <summary>Показывать ли номера строк.</summary>
    public static readonly StyledProperty<bool> ShowLineNumbersProperty =
        AvaloniaProperty.Register<AxCodeView, bool>(nameof(ShowLineNumbers), true);

    /// <summary>Высота строки в долях кегля.</summary>
    /// <remarks>
    /// Безразмерная, как у <see cref="AxCodeBlock.LineHeightRatio"/>: строка — кегль, умноженный на это
    /// число, и растёт вместе с кеглем. NaN — естественная высота шрифта.
    /// </remarks>
    public static readonly StyledProperty<double> LineHeightRatioProperty =
        AvaloniaProperty.Register<AxCodeView, double>(
            nameof(LineHeightRatio), double.NaN, validate: ratio => double.IsNaN(ratio) || ratio > 0);

    /// <summary>Цвет имени элемента.</summary>
    public static readonly StyledProperty<IBrush?> TagBrushProperty =
        AvaloniaProperty.Register<AxCodeView, IBrush?>(nameof(TagBrush));

    /// <summary>Цвет имени свойства.</summary>
    public static readonly StyledProperty<IBrush?> AttributeBrushProperty =
        AvaloniaProperty.Register<AxCodeView, IBrush?>(nameof(AttributeBrush));

    /// <summary>Цвет значения в кавычках.</summary>
    public static readonly StyledProperty<IBrush?> StringBrushProperty =
        AvaloniaProperty.Register<AxCodeView, IBrush?>(nameof(StringBrush));

    /// <summary>Цвет комментария.</summary>
    public static readonly StyledProperty<IBrush?> CommentBrushProperty =
        AvaloniaProperty.Register<AxCodeView, IBrush?>(nameof(CommentBrush));

    /// <summary>Цвет расширения разметки.</summary>
    public static readonly StyledProperty<IBrush?> ExtensionBrushProperty =
        AvaloniaProperty.Register<AxCodeView, IBrush?>(nameof(ExtensionBrush));

    /// <summary>Цвет приставки пространства имён.</summary>
    public static readonly StyledProperty<IBrush?> PrefixBrushProperty =
        AvaloniaProperty.Register<AxCodeView, IBrush?>(nameof(PrefixBrush));

    /// <summary>Цвет директивы языка разметки.</summary>
    public static readonly StyledProperty<IBrush?> DirectiveBrushProperty =
        AvaloniaProperty.Register<AxCodeView, IBrush?>(nameof(DirectiveBrush));

    /// <summary>Цвет текста, который разбор не понял; он же у подчёркивания.</summary>
    public static readonly StyledProperty<IBrush?> ErrorBrushProperty =
        AvaloniaProperty.Register<AxCodeView, IBrush?>(nameof(ErrorBrush));

    /// <summary>Цвет номеров строк.</summary>
    public static readonly StyledProperty<IBrush?> LineNumberBrushProperty =
        AvaloniaProperty.Register<AxCodeView, IBrush?>(nameof(LineNumberBrush));

    /// <summary>Заливка выделения.</summary>
    public static readonly StyledProperty<IBrush?> SelectionBrushProperty =
        AvaloniaProperty.Register<AxCodeView, IBrush?>(nameof(SelectionBrush));

    /// <summary>Заливка отметки.</summary>
    public static readonly StyledProperty<IBrush?> HighlightBrushProperty =
        AvaloniaProperty.Register<AxCodeView, IBrush?>(nameof(HighlightBrush));

    /// <summary>Цвет метки отметки у края строк.</summary>
    public static readonly StyledProperty<IBrush?> HighlightMarkerBrushProperty =
        AvaloniaProperty.Register<AxCodeView, IBrush?>(nameof(HighlightMarkerBrush));

    /// <summary>Ширина метки отметки.</summary>
    public static readonly StyledProperty<double> HighlightMarkerWidthProperty =
        AvaloniaProperty.Register<AxCodeView, double>(nameof(HighlightMarkerWidth));

    /// <summary>Цвет каретки.</summary>
    public static readonly StyledProperty<IBrush?> CaretBrushProperty =
        AvaloniaProperty.Register<AxCodeView, IBrush?>(nameof(CaretBrush));

    /// <summary>Толщина каретки.</summary>
    public static readonly StyledProperty<double> CaretThicknessProperty =
        AvaloniaProperty.Register<AxCodeView, double>(nameof(CaretThickness));

    /// <summary>Отступы колонки номеров: слева от номеров и между ними и текстом.</summary>
    public static readonly StyledProperty<Thickness> GutterPaddingProperty =
        AvaloniaProperty.Register<AxCodeView, Thickness>(nameof(GutterPadding));

    /// <summary>
    /// Человек поставил каретку: щелчком, тягой — по отпусканию — или клавишей, которая её сдвинула.
    /// </summary>
    public static readonly RoutedEvent<AxCodeCaretMovedEventArgs> CaretMovedEvent =
        RoutedEvent.Register<AxCodeView, AxCodeCaretMovedEventArgs>(nameof(CaretMoved), RoutingStrategies.Bubble);

    private AxCodeText _text = AxCodeText.Empty;
    private AxCodeRange _selection;
    private ScrollViewer? _scroll;
    private AxCodeLines? _lines;
    private int _anchor;
    private double? _desiredX;
    private bool _moving;
    private bool _dragging;
    private bool _dragged;

    static AxCodeView()
    {
        FocusableProperty.OverrideDefaultValue<AxCodeView>(true);

        AxSelectionScope.IsActiveProperty.Changed.AddClassHandler<AxCodeView>(
            (view, change) => view.PseudoClasses.Set(":selection-active", change.GetNewValue<bool>()));
    }

    /// <summary>Создаёт просмотр кода.</summary>
    public AxCodeView() => AxSelectionScope.Track(this);

    /// <inheritdoc cref="CaretMovedEvent"/>
    public event EventHandler<AxCodeCaretMovedEventArgs>? CaretMoved
    {
        add => AddHandler(CaretMovedEvent, value);
        remove => RemoveHandler(CaretMovedEvent, value);
    }

    /// <inheritdoc cref="TextProperty"/>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <inheritdoc cref="SpansProperty"/>
    public IReadOnlyList<AxCodeSpan>? Spans
    {
        get => GetValue(SpansProperty);
        set => SetValue(SpansProperty, value);
    }

    /// <inheritdoc cref="HighlightProperty"/>
    public AxCodeRange? Highlight
    {
        get => GetValue(HighlightProperty);
        set => SetValue(HighlightProperty, value);
    }

    /// <inheritdoc cref="CaretOffsetProperty"/>
    public int CaretOffset
    {
        get => GetValue(CaretOffsetProperty);
        set => SetValue(CaretOffsetProperty, value);
    }

    /// <inheritdoc cref="SelectionProperty"/>
    public AxCodeRange Selection => _selection;

    /// <summary>Выделенный текст; пустая строка, если не выделено ничего.</summary>
    public string SelectedText => _selection.IsEmpty ? string.Empty : _text.Text.Substring(_selection.Start, _selection.Length);

    /// <inheritdoc cref="ShowLineNumbersProperty"/>
    public bool ShowLineNumbers
    {
        get => GetValue(ShowLineNumbersProperty);
        set => SetValue(ShowLineNumbersProperty, value);
    }

    /// <inheritdoc cref="LineHeightRatioProperty"/>
    public double LineHeightRatio
    {
        get => GetValue(LineHeightRatioProperty);
        set => SetValue(LineHeightRatioProperty, value);
    }

    /// <inheritdoc cref="TagBrushProperty"/>
    public IBrush? TagBrush
    {
        get => GetValue(TagBrushProperty);
        set => SetValue(TagBrushProperty, value);
    }

    /// <inheritdoc cref="AttributeBrushProperty"/>
    public IBrush? AttributeBrush
    {
        get => GetValue(AttributeBrushProperty);
        set => SetValue(AttributeBrushProperty, value);
    }

    /// <inheritdoc cref="StringBrushProperty"/>
    public IBrush? StringBrush
    {
        get => GetValue(StringBrushProperty);
        set => SetValue(StringBrushProperty, value);
    }

    /// <inheritdoc cref="CommentBrushProperty"/>
    public IBrush? CommentBrush
    {
        get => GetValue(CommentBrushProperty);
        set => SetValue(CommentBrushProperty, value);
    }

    /// <inheritdoc cref="ExtensionBrushProperty"/>
    public IBrush? ExtensionBrush
    {
        get => GetValue(ExtensionBrushProperty);
        set => SetValue(ExtensionBrushProperty, value);
    }

    /// <inheritdoc cref="PrefixBrushProperty"/>
    public IBrush? PrefixBrush
    {
        get => GetValue(PrefixBrushProperty);
        set => SetValue(PrefixBrushProperty, value);
    }

    /// <inheritdoc cref="DirectiveBrushProperty"/>
    public IBrush? DirectiveBrush
    {
        get => GetValue(DirectiveBrushProperty);
        set => SetValue(DirectiveBrushProperty, value);
    }

    /// <inheritdoc cref="ErrorBrushProperty"/>
    public IBrush? ErrorBrush
    {
        get => GetValue(ErrorBrushProperty);
        set => SetValue(ErrorBrushProperty, value);
    }

    /// <inheritdoc cref="LineNumberBrushProperty"/>
    public IBrush? LineNumberBrush
    {
        get => GetValue(LineNumberBrushProperty);
        set => SetValue(LineNumberBrushProperty, value);
    }

    /// <inheritdoc cref="SelectionBrushProperty"/>
    public IBrush? SelectionBrush
    {
        get => GetValue(SelectionBrushProperty);
        set => SetValue(SelectionBrushProperty, value);
    }

    /// <inheritdoc cref="HighlightBrushProperty"/>
    public IBrush? HighlightBrush
    {
        get => GetValue(HighlightBrushProperty);
        set => SetValue(HighlightBrushProperty, value);
    }

    /// <inheritdoc cref="HighlightMarkerBrushProperty"/>
    public IBrush? HighlightMarkerBrush
    {
        get => GetValue(HighlightMarkerBrushProperty);
        set => SetValue(HighlightMarkerBrushProperty, value);
    }

    /// <inheritdoc cref="HighlightMarkerWidthProperty"/>
    public double HighlightMarkerWidth
    {
        get => GetValue(HighlightMarkerWidthProperty);
        set => SetValue(HighlightMarkerWidthProperty, value);
    }

    /// <inheritdoc cref="CaretBrushProperty"/>
    public IBrush? CaretBrush
    {
        get => GetValue(CaretBrushProperty);
        set => SetValue(CaretBrushProperty, value);
    }

    /// <inheritdoc cref="CaretThicknessProperty"/>
    public double CaretThickness
    {
        get => GetValue(CaretThicknessProperty);
        set => SetValue(CaretThicknessProperty, value);
    }

    /// <inheritdoc cref="GutterPaddingProperty"/>
    public Thickness GutterPadding
    {
        get => GetValue(GutterPaddingProperty);
        set => SetValue(GutterPaddingProperty, value);
    }

    /// <summary>Текст, разложенный на строки, вместе с кусками.</summary>
    internal AxCodeText Document => _text;

    /// <summary>Каретку видно: клавиатура в просмотре.</summary>
    internal bool ShowsCaret => IsFocused && IsEffectivelyEnabled;

    /// <summary>Поверхность строк, когда шаблон её поставил.</summary>
    internal AxCodeLines? Lines => _lines;

    /// <summary>Выделяет отрезок; каретка встаёт в его конец.</summary>
    /// <param name="start">Смещение начала.</param>
    /// <param name="length">Число знаков.</param>
    /// <remarks>Выделение кодом события <see cref="CaretMoved"/> не поднимает.</remarks>
    public void Select(int start, int length)
    {
        _anchor = _text.Snap(start);
        MoveCaret((int)Math.Clamp((long)start + Math.Max(0, length), 0, int.MaxValue), extend: true);
    }

    /// <summary>Выделяет весь текст; каретка встаёт в конец.</summary>
    public void SelectAll() => Select(0, _text.Length);

    /// <summary>Кладёт выделенный текст в буфер обмена; без выделения не делает ничего.</summary>
    public async Task CopyAsync()
    {
        if (_selection.IsEmpty || TopLevel.GetTopLevel(this)?.Clipboard is not { } clipboard)
            return;

        await clipboard.SetTextAsync(SelectedText);
    }

    /// <summary>Показывает место в тексте.</summary>
    /// <param name="offset">Смещение.</param>
    /// <remarks>
    /// Видное место остаётся, где было; ушедшее за край, но задевающее экран, приезжает ближайшим
    /// сдвигом; ушедшее целиком встаёт посередине — как переход к месту в редакторах. Зовут сразу
    /// после того, как отдали текст: просьба дождётся раскладки.
    /// </remarks>
    public void ScrollIntoView(int offset) => _lines?.Reveal(new AxCodeRange(offset, 0), center: true);

    /// <summary>Показывает отрезок текста: его строки, а не влезает — его начало.</summary>
    /// <param name="range">Отрезок.</param>
    /// <remarks>Правило то же, что у <see cref="ScrollIntoView(int)"/>.</remarks>
    public void ScrollIntoView(AxCodeRange range) => _lines?.Reveal(range, center: true);

    /// <summary>Кисть роли; без своей — цвет текста.</summary>
    internal IBrush? BrushFor(AxCodeRole role) => role switch
    {
        AxCodeRole.Tag => TagBrush,
        AxCodeRole.Attribute => AttributeBrush,
        AxCodeRole.String => StringBrush,
        AxCodeRole.Comment => CommentBrush,
        AxCodeRole.Extension => ExtensionBrush,
        AxCodeRole.Prefix => PrefixBrush,
        AxCodeRole.Directive => DirectiveBrush,
        AxCodeRole.Error => ErrorBrush,
        _ => null,
    } ?? Foreground;

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_scroll is not null)
            _scroll.Content = null;

        _scroll = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer");
        _lines ??= new AxCodeLines(this);

        if (_scroll is not null)
            _scroll.Content = _lines;
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        var property = change.Property;

        if (property == TextProperty)
        {
            // Новый текст снимает выделение: какие знаки оно покрывало бы теперь, не знает никто.
            _text = AxCodeText.Create(Text ?? string.Empty, Spans);
            CoerceValue(CaretOffsetProperty);
            _anchor = CaretOffset;
            _desiredX = null;
            UpdateSelection();
            _lines?.ResetText();
        }
        else if (property == SpansProperty)
        {
            _text = _text.WithSpans(Spans);
            _lines?.ResetLooks();
        }
        else if (property == CaretOffsetProperty)
        {
            // Каретка, поставленная кодом, снимает выделение; сдвинутая жестом его ведёт.
            if (!_moving)
            {
                _anchor = CaretOffset;
                _desiredX = null;
            }

            UpdateSelection();
            _lines?.InvalidateVisual();
        }
        else if (property == FontFamilyProperty || property == FontSizeProperty || property == FontStyleProperty
                 || property == FontWeightProperty || property == FontStretchProperty || property == LineHeightRatioProperty)
        {
            _desiredX = null;
            _lines?.ResetMetrics();
        }
        else if (property == ForegroundProperty || property == TagBrushProperty || property == AttributeBrushProperty
                 || property == StringBrushProperty || property == CommentBrushProperty || property == ExtensionBrushProperty
                 || property == PrefixBrushProperty || property == DirectiveBrushProperty || property == ErrorBrushProperty)
        {
            _lines?.ResetLooks();
        }
        else if (property == LineNumberBrushProperty)
        {
            _lines?.ResetNumbers();
        }
        else if (property == ShowLineNumbersProperty || property == PaddingProperty || property == GutterPaddingProperty)
        {
            _lines?.InvalidateMeasure();
            _lines?.InvalidateVisual();
        }
        else if (property == HighlightProperty || property == SelectionBrushProperty || property == HighlightBrushProperty
                 || property == HighlightMarkerBrushProperty || property == HighlightMarkerWidthProperty
                 || property == CaretBrushProperty || property == CaretThicknessProperty
                 || property == IsFocusedProperty || property == IsEffectivelyEnabledProperty)
        {
            _lines?.InvalidateVisual();
        }
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        // Нажатие на полосе прокрутки — её, а не текста.
        if (e.Handled || _lines is null || (e.Source as Visual)?.FindAncestorOfType<ScrollBar>(includeSelf: true) is not null)
            return;

        var point = e.GetCurrentPoint(_lines);

        if (!point.Properties.IsLeftButtonPressed)
            return;

        Focus(NavigationMethod.Pointer);

        var offset = _lines.OffsetAt(point.Position, out var inGutter);
        var extend = (e.KeyModifiers & KeyModifiers.Shift) != 0;

        if (inGutter || e.ClickCount >= 3)
            SelectLine(_text.LineOf(offset), extend);
        else if (e.ClickCount == 2)
            SelectWord(offset);
        else
            MoveCaret(offset, extend);

        _dragging = true;
        _dragged = false;
        e.Pointer.Capture(this);
        e.Handled = true;

        RaiseCaretMoved();
    }

    /// <inheritdoc/>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (!_dragging || _lines is null)
            return;

        var offset = _lines.OffsetAt(e.GetPosition(_lines), out _);

        if (offset == CaretOffset)
            return;

        MoveCaret(offset, extend: true);
        _lines.Reveal(new AxCodeRange(CaretOffset, 0), center: false);
        _dragged = true;
    }

    /// <inheritdoc/>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (!_dragging)
            return;

        _dragging = false;
        e.Pointer.Capture(null);

        if (_dragged)
            RaiseCaretMoved();
    }

    /// <inheritdoc/>
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);

        _dragging = false;
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (!e.Handled)
            e.Handled = HandleKey(e);
    }

    /// <inheritdoc/>
    protected override AutomationPeer OnCreateAutomationPeer() => new CodePeer(this);

    private bool HandleKey(KeyEventArgs e)
    {
        var keymap = this.GetPlatformSettings()?.HotkeyConfiguration;

        if (Matches(keymap?.SelectAll, e))
        {
            var before = CaretOffset;

            SelectAll();

            if (CaretOffset != before)
                RaiseCaretMoved();

            return true;
        }

        if (Matches(keymap?.Copy, e))
        {
            _ = CopyAsync();

            return true;
        }

        var shift = (e.KeyModifiers & KeyModifiers.Shift) != 0;
        var word = (e.KeyModifiers & (keymap?.WholeWordTextActionModifiers ?? KeyModifiers.Control)) != 0;
        var command = (e.KeyModifiers & (keymap?.CommandModifiers ?? KeyModifiers.Control)) != 0;
        var caret = CaretOffset;
        var line = _text.LineOf(caret);
        var vertical = false;
        int target;

        if (Matches(keymap?.MoveCursorToTheStartOfDocument, e) || Matches(keymap?.MoveCursorToTheStartOfDocumentWithSelection, e))
            target = 0;
        else if (Matches(keymap?.MoveCursorToTheEndOfDocument, e) || Matches(keymap?.MoveCursorToTheEndOfDocumentWithSelection, e))
            target = _text.Length;
        else if (Matches(keymap?.MoveCursorToTheStartOfLine, e) || Matches(keymap?.MoveCursorToTheStartOfLineWithSelection, e))
            target = Home(caret);
        else if (Matches(keymap?.MoveCursorToTheEndOfLine, e) || Matches(keymap?.MoveCursorToTheEndOfLineWithSelection, e))
            target = _text.EndOf(line);
        else
        {
            switch (e.Key)
            {
                case Key.Left:
                    target = !shift && !_selection.IsEmpty ? _selection.Start : word ? WordLeft(caret) : CharLeft(caret);
                    break;
                case Key.Right:
                    target = !shift && !_selection.IsEmpty ? _selection.End : word ? WordRight(caret) : CharRight(caret);
                    break;
                case Key.Up when command && !shift:
                    _lines?.ScrollLines(-1);
                    return true;
                case Key.Down when command && !shift:
                    _lines?.ScrollLines(1);
                    return true;
                case Key.Up:
                    target = Vertical(line - 1);
                    vertical = true;
                    break;
                case Key.Down:
                    target = Vertical(line + 1);
                    vertical = true;
                    break;
                case Key.PageUp:
                    target = Page(line, -1);
                    vertical = true;
                    break;
                case Key.PageDown:
                    target = Page(line, 1);
                    vertical = true;
                    break;
                case Key.Home:
                    target = command ? 0 : Home(caret);
                    break;
                case Key.End:
                    target = command ? _text.Length : _text.EndOf(line);
                    break;
                default:
                    return false;
            }
        }

        MoveCaret(target, shift, keepColumn: vertical);
        _lines?.Reveal(new AxCodeRange(CaretOffset, 0), center: false);

        if (CaretOffset != caret)
            RaiseCaretMoved();

        return true;
    }

    private static bool Matches(List<KeyGesture>? gestures, KeyEventArgs e) =>
        gestures is not null && gestures.Exists(gesture => gesture.Matches(e));

    /// <summary>Ставит каретку; с <paramref name="extend"/> выделение тянется от прежнего начала.</summary>
    private void MoveCaret(int offset, bool extend, bool keepColumn = false)
    {
        var anchor = _anchor;

        _moving = true;

        try
        {
            SetCurrentValue(CaretOffsetProperty, offset);
        }
        finally
        {
            _moving = false;
        }

        _anchor = extend ? anchor : CaretOffset;

        if (!keepColumn)
            _desiredX = null;

        UpdateSelection();
        _lines?.InvalidateVisual();
    }

    private void UpdateSelection()
    {
        var caret = CaretOffset;
        var anchor = Math.Min(_anchor, _text.Length);

        SetAndRaise(SelectionProperty, ref _selection, new AxCodeRange(Math.Min(anchor, caret), Math.Abs(caret - anchor)));
    }

    private void RaiseCaretMoved() => RaiseEvent(new AxCodeCaretMovedEventArgs(CaretMovedEvent, CaretOffset));

    /// <summary>Выделяет строку вместе с её переводом; с <paramref name="extend"/> — от прежнего начала до неё.</summary>
    private void SelectLine(int line, bool extend)
    {
        var start = _text.StartOf(line);
        var end = line + 1 < _text.LineCount ? _text.StartOf(line + 1) : _text.Length;

        if (extend)
        {
            MoveCaret(start < _anchor ? start : end, extend: true);

            return;
        }

        _anchor = start;
        MoveCaret(end, extend: true);
    }

    /// <summary>Выделяет слово, знаки или пробелы под смещением — то, что одного рода с ним.</summary>
    private void SelectWord(int offset)
    {
        var line = _text.LineOf(offset);
        var start = _text.StartOf(line);
        var end = _text.EndOf(line);

        if (start == end)
        {
            MoveCaret(offset, extend: false);

            return;
        }

        var at = Math.Min(offset, end - 1);
        var kind = Kind(_text.Text[at]);
        var from = at;
        var to = at + 1;

        while (from > start && Kind(_text.Text[from - 1]) == kind)
            from--;

        while (to < end && Kind(_text.Text[to]) == kind)
            to++;

        _anchor = from;
        MoveCaret(to, extend: true);
    }

    private int CharLeft(int offset)
    {
        if (offset == 0)
            return 0;

        var line = _text.LineOf(offset);

        return offset == _text.StartOf(line) ? _text.EndOf(line - 1) : _text.Snap(offset - 1);
    }

    private int CharRight(int offset)
    {
        if (offset >= _text.Length)
            return _text.Length;

        var line = _text.LineOf(offset);

        if (offset == _text.EndOf(line))
            return line + 1 < _text.LineCount ? _text.StartOf(line + 1) : offset;

        var next = offset + 1;

        // Суррогатная пара — один знак: каретка перешагивает её целиком.
        return next < _text.Length && char.IsLowSurrogate(_text.Text[next]) ? next + 1 : next;
    }

    /// <summary>Начало предыдущего слова: пробелы назад, затем знаки одного рода.</summary>
    private int WordLeft(int offset)
    {
        var line = _text.LineOf(offset);
        var start = _text.StartOf(line);

        if (offset == start)
            return CharLeft(offset);

        while (offset > start && char.IsWhiteSpace(_text.Text[offset - 1]))
            offset--;

        if (offset > start)
        {
            var kind = Kind(_text.Text[offset - 1]);

            while (offset > start && Kind(_text.Text[offset - 1]) == kind)
                offset--;
        }

        return offset;
    }

    /// <summary>Начало следующего слова: знаки одного рода вперёд, затем пробелы.</summary>
    private int WordRight(int offset)
    {
        var line = _text.LineOf(offset);
        var end = _text.EndOf(line);

        if (offset == end)
            return CharRight(offset);

        var kind = Kind(_text.Text[offset]);

        while (offset < end && Kind(_text.Text[offset]) == kind)
            offset++;

        while (offset < end && char.IsWhiteSpace(_text.Text[offset]))
            offset++;

        return offset;
    }

    /// <summary>Род знака для слов: пробел, буква или цифра, прочее.</summary>
    private static int Kind(char c) => char.IsWhiteSpace(c) ? 0 : char.IsLetterOrDigit(c) || c == '_' ? 1 : 2;

    /// <summary>Начало строки по-редакторски: к первому непробельному знаку, а с него — к колонке 0.</summary>
    private int Home(int offset)
    {
        var line = _text.LineOf(offset);
        var start = _text.StartOf(line);
        var end = _text.EndOf(line);
        var text = start;

        while (text < end && char.IsWhiteSpace(_text.Text[text]))
            text++;

        return offset == text ? start : text;
    }

    /// <summary>Место в строке под той же колонкой, с которой ушли по вертикали.</summary>
    private int Vertical(int line)
    {
        if (_lines is null || line < 0 || line >= _text.LineCount)
            return CaretOffset;

        _desiredX ??= _lines.XOf(CaretOffset);

        return _lines.OffsetAtX(line, _desiredX.Value);
    }

    /// <summary>Страница вверх или вниз: каретка и текст едут на одно и то же число строк.</summary>
    private int Page(int line, int direction)
    {
        if (_lines is null)
            return CaretOffset;

        var lines = Math.Max(1, _lines.VisibleLines - 1) * direction;
        var target = Math.Clamp(line + lines, 0, _text.LineCount - 1);

        if (target == line)
            return direction < 0 ? 0 : _text.Length;

        _lines.ScrollLines(target - line);

        return Vertical(target);
    }

    /// <summary>Диктору просмотр кода — документ только для чтения, а его значение — текст.</summary>
    private sealed class CodePeer(AxCodeView owner) : ControlAutomationPeer(owner), IValueProvider
    {
        public bool IsReadOnly => true;

        public string? Value => ((AxCodeView)Owner).Text;

        public void SetValue(string? value) => throw new InvalidOperationException("Просмотр кода только для чтения.");

        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Document;
    }
}

/// <summary>Аргументы <see cref="AxCodeView.CaretMoved"/>.</summary>
public sealed class AxCodeCaretMovedEventArgs : RoutedEventArgs
{
    /// <summary>Создаёт аргументы события.</summary>
    /// <param name="routedEvent">Событие.</param>
    /// <param name="offset">Смещение, на котором встала каретка.</param>
    public AxCodeCaretMovedEventArgs(RoutedEvent routedEvent, int offset)
        : base(routedEvent)
    {
        Offset = offset;
    }

    /// <summary>Смещение, на котором встала каретка.</summary>
    public int Offset { get; }
}
