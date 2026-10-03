using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Rendering;
using Avalonia.Threading;

namespace ArxisStudio.Controls;

/// <summary>
/// Поверхность <see cref="AxCodeView"/>: рисует видимые строки, номера, отметку, выделение и
/// каретку и сама ведёт прокрутку.
/// </summary>
/// <remarks>
/// <para>
/// Прокрутка логическая: область прокрутки отдаёт поверхности смещение, а не двигает её. Поэтому
/// номера строк стоят на месте, пока текст едет вбок, и строится только то, что видно: документ в
/// десятки тысяч строк стоит столько же, сколько экран. Единица прокрутки — пиксель раскладки.
/// </para>
/// <para>
/// Раскладка строки живёт, пока строка рядом с экраном — в пределах экрана сверху и снизу, — и
/// сбрасывается правкой текста, кусков, шрифта или кистей. Ширину прокрутки даёт самая длинная
/// строка в колонках, а разложенная строка шире оценки — например, с широкими знаками —
/// раздвигает её, как только её увидят.
/// </para>
/// </remarks>
internal sealed class AxCodeLines : Control, ILogicalScrollable, ICustomHitTest
{
    /// <summary>Строк за щелчок колеса — как в Windows по умолчанию.</summary>
    private const int WheelLines = 3;

    /// <summary>Колонок за щелчок колеса вбок.</summary>
    private const int WheelColumns = 8;

    /// <summary>Наименьшее число цифр в колонке номеров: она не прыгает на десятой строке.</summary>
    private const int MinimumDigits = 2;

    private readonly AxCodeView _view;
    private readonly Dictionary<int, TextLayout> _lines = [];
    private readonly Dictionary<int, TextLayout> _numbers = [];
    private CodeMetrics? _metrics;
    private TextRunProperties[]? _runs;
    private Size _extent;
    private Size _viewport;
    private Vector _offset;
    private double _widest;
    private bool _widening;
    private (AxCodeRange Range, bool Center)? _reveal;

    /// <summary>Создаёт поверхность для своего просмотра.</summary>
    public AxCodeLines(AxCodeView view)
    {
        _view = view;
        ClipToBounds = true;
    }

    /// <inheritdoc/>
    public event EventHandler? ScrollInvalidated;

    /// <summary>Строк, разложенных сейчас: столько, сколько видно, с запасом в экран.</summary>
    internal int RealizedLines => _lines.Count;

    /// <inheritdoc/>
    public bool CanHorizontallyScroll { get; set; }

    /// <inheritdoc/>
    public bool CanVerticallyScroll { get; set; }

    /// <inheritdoc/>
    public bool IsLogicalScrollEnabled => true;

    /// <inheritdoc/>
    public Size ScrollSize => new(Metrics.Column * WheelColumns, Metrics.Line * WheelLines);

    /// <inheritdoc/>
    public Size PageScrollSize => _viewport;

    /// <inheritdoc/>
    public Size Extent => _extent;

    /// <inheritdoc/>
    public Size Viewport => _viewport;

    /// <inheritdoc/>
    public Vector Offset
    {
        get => _offset;
        set
        {
            var clamped = Clamp(value);

            if (clamped != _offset)
            {
                _offset = clamped;
                InvalidateVisual();
            }

            // Смещение вне хода область прокрутки должна перечитать, иначе её полосы разойдутся с текстом.
            if (clamped != value)
                RaiseScrollInvalidated(EventArgs.Empty);
        }
    }

    /// <summary>Высота строки в единицах раскладки.</summary>
    internal double LineHeight => Metrics.Line;

    /// <summary>Шкала шрифта: гарнитура, кегль, колонка и строка.</summary>
    private CodeMetrics Metrics => _metrics ??= MeasureMetrics();

    /// <summary>Ширина колонки номеров вместе с её отступами; без номеров — ноль.</summary>
    private double Gutter
    {
        get
        {
            if (!_view.ShowLineNumbers)
                return 0;

            var padding = _view.GutterPadding;
            var digits = Math.Max(MinimumDigits, _view.Document.LineCount.ToString(CultureInfo.InvariantCulture).Length);

            return padding.Left + digits * Metrics.Column + padding.Right;
        }
    }

    /// <summary>Где на поверхности стоит колонка текста с номером 0.</summary>
    private double TextLeft => Gutter + _view.Padding.Left - _offset.X;

    /// <inheritdoc/>
    public bool BringIntoView(Control target, Rect targetRect) => false;

    /// <inheritdoc/>
    public Control? GetControlInDirection(NavigationDirection direction, Control? from) => null;

    /// <inheritdoc/>
    public void RaiseScrollInvalidated(EventArgs e) => ScrollInvalidated?.Invoke(this, e);

    /// <inheritdoc/>
    public bool HitTest(Point point) => new Rect(Bounds.Size).Contains(point);

    /// <summary>Текст сменился: строки, их число и ширина — заново.</summary>
    public void ResetText()
    {
        _widest = 0;
        ResetLooks();
    }

    /// <summary>Шрифт или высота строки сменились: шкала и всё, что по ней разложено, — заново.</summary>
    public void ResetMetrics()
    {
        _metrics = null;
        ResetText();
    }

    /// <summary>Куски или кисти сменились: раскладка строк заново, размеры прежние.</summary>
    public void ResetLooks()
    {
        _runs = null;
        Clear(_lines);
        Clear(_numbers);
        InvalidateMeasure();
        InvalidateVisual();
    }

    /// <summary>Кисть номеров сменилась.</summary>
    public void ResetNumbers()
    {
        Clear(_numbers);
        InvalidateVisual();
    }

    /// <summary>Смещение текста под точкой поверхности.</summary>
    /// <param name="point">Точка в координатах поверхности; вне её — ближайшая строка и колонка.</param>
    /// <param name="inGutter">Точка в колонке номеров.</param>
    public int OffsetAt(Point point, out bool inGutter)
    {
        var text = _view.Document;
        var line = Math.Clamp((int)Math.Floor((point.Y + _offset.Y - _view.Padding.Top) / Metrics.Line), 0, text.LineCount - 1);

        inGutter = point.X < Gutter;

        return OffsetAtX(line, point.X - TextLeft);
    }

    /// <summary>Смещение в строке, ближайшее к расстоянию от начала её текста.</summary>
    public int OffsetAtX(int line, double x)
    {
        var text = _view.Document;
        var hit = Line(line).HitTestPoint(new Point(x, Metrics.Line / 2));

        return text.StartOf(line) + Math.Clamp(hit.TextPosition, 0, text.LengthOf(line));
    }

    /// <summary>Расстояние от начала текста строки до смещения в ней.</summary>
    public double XOf(int offset)
    {
        var text = _view.Document;
        var line = text.LineOf(offset);

        return Line(line).HitTestTextPosition(offset - text.StartOf(line)).X;
    }

    /// <summary>Точка перед смещением посередине его строки — там, где встанет каретка.</summary>
    internal Point PointOf(int offset) =>
        new(TextLeft + XOf(offset), Top(_view.Document.LineOf(offset)) + Metrics.Line / 2);

    /// <summary>Сколько целых строк видно сразу.</summary>
    public int VisibleLines => Math.Max(1, (int)Math.Floor(_viewport.Height / Metrics.Line));

    /// <summary>Прокручивает на столько строк; со знаком минус — вверх.</summary>
    public void ScrollLines(int lines) => ScrollTo(new Vector(_offset.X, _offset.Y + lines * Metrics.Line));

    /// <summary>
    /// Показывает отрезок: целиком вне экрана — посередине, задевающий экран — ближайшим сдвигом.
    /// </summary>
    /// <param name="range">Отрезок текста; пустой — место каретки.</param>
    /// <param name="center">Ставить ли отрезок, ушедший за экран, посередине, а не к ближнему краю.</param>
    /// <remarks>
    /// До первой раскладки размер окна неизвестен, и просьба ждёт её: хозяин зовёт показ сразу
    /// после того, как отдал текст.
    /// </remarks>
    public void Reveal(AxCodeRange range, bool center)
    {
        _reveal = (range, center);

        if (IsArrangeValid && _viewport.Height > 0)
            ApplyReveal();
        else
            InvalidateArrange();
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        UpdateExtent();

        return new Size(Math.Min(availableSize.Width, _extent.Width), Math.Min(availableSize.Height, _extent.Height));
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var resized = finalSize != _viewport;

        _viewport = finalSize;

        if (!UpdateExtent() && resized)
            RaiseScrollInvalidated(EventArgs.Empty);

        ApplyReveal();

        return finalSize;
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Clear(_lines);
        Clear(_numbers);

        base.OnDetachedFromVisualTree(e);
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var text = _view.Document;
        var metrics = Metrics;
        var gutter = Gutter;
        var left = TextLeft;
        var top = _offset.Y - _view.Padding.Top;
        var first = Math.Clamp((int)Math.Floor(top / metrics.Line), 0, text.LineCount - 1);
        var last = Math.Clamp((int)Math.Floor((top + Bounds.Height) / metrics.Line), first, text.LineCount - 1);

        using (context.PushClip(new Rect(gutter, 0, Math.Max(0, Bounds.Width - gutter), Bounds.Height)))
        {
            if (_view.Highlight is { IsEmpty: false } highlight && _view.HighlightBrush is { } fill)
                Fill(context, fill, highlight, first, last, left);

            if (!_view.Selection.IsEmpty && _view.SelectionBrush is { } selection)
                Fill(context, selection, _view.Selection, first, last, left);

            for (var line = first; line <= last; line++)
                Line(line).Draw(context, new Point(left, Top(line)));

            if (_view.ShowsCaret && _view.CaretBrush is { } caret)
                DrawCaret(context, caret, first, last, left);
        }

        if (_view.ShowLineNumbers && _view.LineNumberBrush is { } numbers)
            DrawNumbers(context, numbers, first, last, gutter);

        if (_view.Highlight is { IsEmpty: false } marked && _view.HighlightMarkerBrush is { } marker)
            DrawMarker(context, marker, marked, first, last, gutter);

        Evict(first, last);
    }

    /// <summary>Верх строки на поверхности.</summary>
    private double Top(int line) => _view.Padding.Top + line * Metrics.Line - _offset.Y;

    /// <summary>Заливает отрезок по строкам; перевод строки внутри отрезка — одной колонкой.</summary>
    private void Fill(DrawingContext context, IBrush brush, AxCodeRange range, int first, int last, double left)
    {
        var text = _view.Document;
        var end = Math.Clamp(range.End, 0, text.Length);
        var start = Math.Clamp(range.Start, 0, end);
        var metrics = Metrics;

        for (var line = Math.Max(text.LineOf(start), first); line <= Math.Min(text.LineOf(end), last); line++)
        {
            var lineStart = text.StartOf(line);
            var lineEnd = text.EndOf(line);
            var from = Math.Max(start, lineStart) - lineStart;
            var to = Math.Min(end, lineEnd) - lineStart;
            var top = Top(line);
            var layout = Line(line);

            if (to > from)
            {
                foreach (var bounds in layout.HitTestTextRange(from, to - from))
                    context.FillRectangle(brush, new Rect(left + bounds.X, top, bounds.Width, metrics.Line));
            }

            // Отрезок идёт дальше этой строки: её перевод строки показан одной колонкой, как в
            // редакторах, — иначе выделение пустой строки не было бы видно вовсе.
            if (end > lineEnd && start <= lineEnd && line < text.LineCount - 1)
            {
                var x = left + layout.HitTestTextPosition(lineEnd - lineStart).X;

                context.FillRectangle(brush, new Rect(x, top, metrics.Column, metrics.Line));
            }
        }
    }

    private void DrawCaret(DrawingContext context, IBrush brush, int first, int last, double left)
    {
        var text = _view.Document;
        var caret = _view.CaretOffset;
        var line = text.LineOf(caret);

        if (line < first || line > last)
            return;

        // Каретка встаёт на пиксель устройства: дробная ширина колонки размазала бы её на два.
        var scale = LayoutHelper.GetLayoutScale(this);
        var x = Math.Round((left + Line(line).HitTestTextPosition(caret - text.StartOf(line)).X) * scale) / scale;

        context.FillRectangle(brush, new Rect(x, Top(line), _view.CaretThickness, Metrics.Line));
    }

    private void DrawNumbers(DrawingContext context, IBrush brush, int first, int last, double gutter)
    {
        var right = gutter - _view.GutterPadding.Right;

        for (var line = first; line <= last; line++)
        {
            if (!_numbers.TryGetValue(line, out var number))
            {
                number = new TextLayout(
                    (line + 1).ToString(CultureInfo.InvariantCulture),
                    Metrics.Typeface,
                    Metrics.Size,
                    brush,
                    lineHeight: Metrics.LineHeightRequest);

                _numbers[line] = number;
            }

            number.Draw(context, new Point(right - number.WidthIncludingTrailingWhitespace, Top(line)));
        }
    }

    /// <summary>Метка отрезка у края текста: видно, какие строки он занимает, даже без заливки под ним.</summary>
    private void DrawMarker(DrawingContext context, IBrush brush, AxCodeRange range, int first, int last, double gutter)
    {
        var width = _view.HighlightMarkerWidth;

        if (width <= 0)
            return;

        var text = _view.Document;
        var start = Math.Clamp(range.Start, 0, text.Length);
        var from = Math.Max(text.LineOf(start), first);

        // Последний знак отрезка, а не его конец: отрезок, кончившийся переводом строки, следующую строку не занимает.
        var to = Math.Min(text.LineOf(Math.Clamp(range.End - 1, start, text.Length)), last);

        if (to < from)
            return;

        var top = Top(from);

        context.FillRectangle(brush, new Rect(gutter, top, width, Top(to) + Metrics.Line - top));
    }

    /// <summary>Раскладка строки: из запаса или новая.</summary>
    private TextLayout Line(int line)
    {
        if (_lines.TryGetValue(line, out var layout))
            return layout;

        var text = _view.Document;
        var start = text.StartOf(line);
        var metrics = Metrics;
        var source = new LineSource(text.LineText(line), text.SpansIn(start, text.EndOf(line)).ToArray(), start, Run);

        layout = new TextLayout(source, new CodeParagraph(Run(AxCodeRole.Text), metrics.LineHeightRequest, metrics.Column * AxCodeText.TabSize),
            TextTrimming.None, double.PositiveInfinity, double.PositiveInfinity, 0);

        _lines[line] = layout;

        if (layout.WidthIncludingTrailingWhitespace > _widest)
            Widen(layout.WidthIncludingTrailingWhitespace);

        return layout;
    }

    /// <summary>
    /// Строка оказалась шире всех прежних: ширина прокрутки растёт после текущего прохода, а не
    /// посреди отрисовки.
    /// </summary>
    private void Widen(double width)
    {
        _widest = width;

        if (_widening)
            return;

        _widening = true;

        Dispatcher.UIThread.Post(() =>
        {
            _widening = false;
            InvalidateMeasure();
            UpdateExtent();
        }, DispatcherPriority.Background);
    }

    private TextRunProperties Run(AxCodeRole role)
    {
        if (_runs is null)
        {
            var metrics = Metrics;
            var roles = Enum.GetValues<AxCodeRole>();

            _runs = new TextRunProperties[roles.Length];

            foreach (var each in roles)
            {
                _runs[(int)each] = new GenericTextRunProperties(
                    metrics.Typeface,
                    metrics.Size,
                    textDecorations: each == AxCodeRole.Error ? TextDecorations.Underline : null,
                    foregroundBrush: _view.BrushFor(each));
            }
        }

        return _runs[(int)role];
    }

    /// <summary>Пересчитывает протяжённость и прижимает смещение; говорит, если что-то сдвинулось.</summary>
    private bool UpdateExtent()
    {
        var text = _view.Document;
        var metrics = Metrics;
        var padding = _view.Padding;

        var extent = new Size(
            Gutter + padding.Left + Math.Max(text.Columns * metrics.Column, _widest) + padding.Right,
            padding.Top + text.LineCount * metrics.Line + padding.Bottom);

        var changed = extent != _extent;

        _extent = extent;

        var offset = Clamp(_offset);

        if (offset != _offset)
        {
            _offset = offset;
            changed = true;
        }

        if (changed)
        {
            InvalidateVisual();
            RaiseScrollInvalidated(EventArgs.Empty);
        }

        return changed;
    }

    private Vector Clamp(Vector offset) => new(
        Math.Clamp(offset.X, 0, Math.Max(0, _extent.Width - _viewport.Width)),
        Math.Clamp(offset.Y, 0, Math.Max(0, _extent.Height - _viewport.Height)));

    private void ScrollTo(Vector offset)
    {
        var clamped = Clamp(offset);

        if (clamped == _offset)
            return;

        _offset = clamped;
        InvalidateVisual();
        RaiseScrollInvalidated(EventArgs.Empty);
    }

    private void ApplyReveal()
    {
        if (_reveal is not { } reveal || _viewport.Height <= 0)
            return;

        _reveal = null;

        var text = _view.Document;
        var metrics = Metrics;
        var padding = _view.Padding;
        var start = text.Snap(reveal.Range.Start);
        var end = text.Snap(Math.Max(reveal.Range.Start, reveal.Range.End));
        var firstLine = text.LineOf(start);
        var lastLine = text.LineOf(end);

        // По вертикали — строки отрезка; не влезает — его начало.
        var top = padding.Top + firstLine * metrics.Line;
        var bottom = padding.Top + (lastLine + 1) * metrics.Line;
        var height = _viewport.Height;
        var y = _offset.Y;

        if (bottom - top > height)
            bottom = top + metrics.Line;

        if (reveal.Center && (bottom <= y || top >= y + height))
            y = (top + bottom - height) / 2;
        else if (top < y)
            y = top;
        else if (bottom > y + height)
            y = bottom - height;

        // По горизонтали — начало отрезка; однострочный — целиком, если влезает.
        var width = Math.Max(0, _viewport.Width - Gutter);
        var left = padding.Left + XOf(start);
        var right = padding.Left + (lastLine == firstLine ? XOf(end) : XOf(start)) + _view.CaretThickness;
        var x = _offset.X;

        if (right - left > width - padding.Left - padding.Right)
            right = left;

        if (left - padding.Left < x)
            x = left - padding.Left;
        else if (right + padding.Right > x + width)
            x = right + padding.Right - width;

        ScrollTo(new Vector(x, y));
    }

    private void Evict(int first, int last)
    {
        var spare = last - first + 1;

        Drop(_lines, first - spare, last + spare);
        Drop(_numbers, first - spare, last + spare);
    }

    private static void Drop(Dictionary<int, TextLayout> cache, int from, int to)
    {
        if (cache.Count <= to - from + 1)
            return;

        List<int>? gone = null;

        foreach (var line in cache.Keys)
        {
            if (line < from || line > to)
                (gone ??= []).Add(line);
        }

        if (gone is null)
            return;

        foreach (var line in gone)
        {
            cache[line].Dispose();
            cache.Remove(line);
        }
    }

    private static void Clear(Dictionary<int, TextLayout> cache)
    {
        foreach (var layout in cache.Values)
            layout.Dispose();

        cache.Clear();
    }

    private CodeMetrics MeasureMetrics()
    {
        var typeface = new Typeface(_view.FontFamily, _view.FontStyle, _view.FontWeight, _view.FontStretch);
        var size = _view.FontSize;
        var ratio = _view.LineHeightRatio;

        using var probe = new TextLayout("0", typeface, size, null);

        var request = double.IsNaN(ratio) ? double.NaN : size * ratio;

        return new CodeMetrics(typeface, size, probe.WidthIncludingTrailingWhitespace, double.IsNaN(request) ? probe.Height : request, request);
    }

    /// <summary>Шкала шрифта просмотра.</summary>
    /// <param name="Typeface">Гарнитура с начертанием.</param>
    /// <param name="Size">Кегль.</param>
    /// <param name="Column">Ширина колонки — знака моноширинного шрифта.</param>
    /// <param name="Line">Высота строки.</param>
    /// <param name="LineHeightRequest">Высота строки, которую просит доля кегля; NaN — естественная высота шрифта.</param>
    private sealed record CodeMetrics(Typeface Typeface, double Size, double Column, double Line, double LineHeightRequest);

    /// <summary>Строка по кускам: каждый кусок — свой прогон со своей кистью.</summary>
    private sealed class LineSource(ReadOnlyMemory<char> text, AxCodeSpan[] spans, int origin, Func<AxCodeRole, TextRunProperties> run) : ITextSource
    {
        public TextRun GetTextRun(int textSourceIndex)
        {
            if (textSourceIndex >= text.Length)
                return new TextEndOfParagraph();

            var at = origin + textSourceIndex;

            foreach (var span in spans)
            {
                if (span.End <= at)
                    continue;

                return span.Start > at
                    ? Characters(textSourceIndex, span.Start - origin, AxCodeRole.Text)
                    : Characters(textSourceIndex, span.End - origin, span.Role);
            }

            return Characters(textSourceIndex, text.Length, AxCodeRole.Text);
        }

        private TextCharacters Characters(int from, int to, AxCodeRole role) =>
            new(text[from..Math.Min(to, text.Length)], run(role));
    }

    /// <summary>Абзац строки кода: без переноса, табуляция — до остановки в четыре колонки.</summary>
    private sealed class CodeParagraph(TextRunProperties text, double lineHeight, double tab) : TextParagraphProperties
    {
        public override FlowDirection FlowDirection => FlowDirection.LeftToRight;

        public override TextAlignment TextAlignment => TextAlignment.Left;

        public override double LineHeight => lineHeight;

        public override bool FirstLineInParagraph => true;

        public override TextRunProperties DefaultTextRunProperties => text;

        public override TextWrapping TextWrapping => TextWrapping.NoWrap;

        public override double Indent => 0;

        public override double DefaultIncrementalTab => tab;
    }
}
