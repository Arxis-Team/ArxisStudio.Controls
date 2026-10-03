namespace ArxisStudio.Controls;

/// <summary>
/// Текст просмотра кода, разложенный на строки, и куски его подсветки.
/// </summary>
/// <remarks>
/// Неизменяемый: новый текст или новые куски — новый экземпляр. Строку ищут двоичным поиском по
/// началам, кусок строки — так же по концам кусков, поэтому цена вопроса не растёт с документом.
/// </remarks>
internal sealed class AxCodeText
{
    /// <summary>Табуляция ведёт к следующей колонке, кратной этому числу.</summary>
    public const int TabSize = 4;

    private readonly int[] _starts;
    private readonly int[] _lengths;
    private readonly AxCodeSpan[] _spans;

    private AxCodeText(string text, int[] starts, int[] lengths, int columns, AxCodeSpan[] spans)
    {
        Text = text;
        _starts = starts;
        _lengths = lengths;
        Columns = columns;
        _spans = spans;
    }

    /// <summary>Пустой текст: одна пустая строка.</summary>
    public static AxCodeText Empty { get; } = Create(string.Empty, null);

    /// <summary>Весь текст.</summary>
    public string Text { get; }

    /// <summary>Длина текста в знаках.</summary>
    public int Length => Text.Length;

    /// <summary>Число строк; у пустого текста и у текста без переводов строки — одна.</summary>
    public int LineCount => _starts.Length;

    /// <summary>Ширина самой длинной строки в колонках, табуляция — до своей остановки.</summary>
    public int Columns { get; }

    /// <summary>Раскладывает текст на строки и укладывает куски подсветки.</summary>
    /// <param name="text">Текст; переводы строки — <c>\r\n</c>, <c>\n</c> или <c>\r</c>.</param>
    /// <param name="spans">Куски в любом порядке; пересекающийся кусок начинается там, где кончился предыдущий.</param>
    public static AxCodeText Create(string text, IReadOnlyList<AxCodeSpan>? spans)
    {
        var starts = new List<int> { 0 };
        var lengths = new List<int>();
        var columns = 0;
        var widest = 0;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];

            if (c is '\r' or '\n')
            {
                lengths.Add(i - starts[^1]);

                if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                    i++;

                starts.Add(i + 1);
                widest = Math.Max(widest, columns);
                columns = 0;

                continue;
            }

            columns = c == '\t' ? (columns / TabSize + 1) * TabSize : columns + 1;
        }

        lengths.Add(text.Length - starts[^1]);
        widest = Math.Max(widest, columns);

        return new AxCodeText(text, [.. starts], [.. lengths], widest, Lay(spans, text.Length));
    }

    /// <summary>Тот же текст с другими кусками.</summary>
    public AxCodeText WithSpans(IReadOnlyList<AxCodeSpan>? spans) =>
        new(Text, _starts, _lengths, Columns, Lay(spans, Text.Length));

    /// <summary>Смещение начала строки.</summary>
    public int StartOf(int line) => _starts[line];

    /// <summary>Длина строки без перевода строки.</summary>
    public int LengthOf(int line) => _lengths[line];

    /// <summary>Смещение конца строки — перед её переводом строки.</summary>
    public int EndOf(int line) => _starts[line] + _lengths[line];

    /// <summary>Знаки строки без перевода строки.</summary>
    public ReadOnlyMemory<char> LineText(int line) => Text.AsMemory(_starts[line], _lengths[line]);

    /// <summary>Строка, в которой стоит смещение; смещение внутри перевода строки относится к ней же.</summary>
    public int LineOf(int offset)
    {
        var index = Array.BinarySearch(_starts, Math.Clamp(offset, 0, Text.Length));

        return index >= 0 ? index : ~index - 1;
    }

    /// <summary>
    /// Ставит смещение туда, где может стоять каретка: в пределы текста, не внутрь перевода строки
    /// и не между половинами суррогатной пары.
    /// </summary>
    public int Snap(int offset)
    {
        offset = Math.Clamp(offset, 0, Text.Length);

        var end = EndOf(LineOf(offset));

        if (offset > end)
            return end;

        if (offset > 0 && offset < Text.Length && char.IsLowSurrogate(Text[offset]) && char.IsHighSurrogate(Text[offset - 1]))
            return offset - 1;

        return offset;
    }

    /// <summary>Куски, задевающие отрезок строки, по порядку.</summary>
    public ReadOnlySpan<AxCodeSpan> SpansIn(int start, int end)
    {
        // Куски не пересекаются и отсортированы, поэтому их концы растут вместе с началами.
        var low = 0;
        var high = _spans.Length;

        while (low < high)
        {
            var middle = (low + high) >>> 1;

            if (_spans[middle].End <= start)
                low = middle + 1;
            else
                high = middle;
        }

        var last = low;

        while (last < _spans.Length && _spans[last].Start < end)
            last++;

        return _spans.AsSpan(low, last - low);
    }

    /// <summary>Сортирует куски, обрезает их по тексту и друг по другу, выбрасывает пустые.</summary>
    private static AxCodeSpan[] Lay(IReadOnlyList<AxCodeSpan>? spans, int length)
    {
        if (spans is null || spans.Count == 0)
            return [];

        var laid = new List<AxCodeSpan>(spans.Count);
        var reached = 0;

        // OrderBy устойчив: из двух кусков с одним началом первым остаётся тот, что пришёл первым.
        foreach (var span in spans.OrderBy(span => span.Start))
        {
            var start = Math.Max(span.Start, reached);
            var end = (int)Math.Min((long)span.Start + span.Length, length);

            if (end <= start)
                continue;

            laid.Add(span with { Start = start, Length = end - start });
            reached = end;
        }

        return [.. laid];
    }
}
