using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace ArxisStudio.Controls;

/// <summary>
/// Блок кода: разметка моноширинным шрифтом с подсветкой в четыре цвета —
/// тег, атрибут, строка, комментарий. Цвета приходят из темы токенами
/// <c>AxCode*</c> свойствами-кистями, поэтому переключение варианта темы
/// перекрашивает уже показанный код.
/// </summary>
/// <remarks>
/// Подсветка — простой разбор XML-разметки, а не настоящий лексер: блок
/// показывает примеры и сниппеты, редактор кода живёт на AvaloniaEdit со
/// своей подсветкой.
/// </remarks>
public class AxCodeBlock : TemplatedControl
{
    /// <summary>Текст кода.</summary>
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<AxCodeBlock, string?>(nameof(Text));

    /// <summary>Цвет имени тега.</summary>
    public static readonly StyledProperty<IBrush?> TagBrushProperty =
        AvaloniaProperty.Register<AxCodeBlock, IBrush?>(nameof(TagBrush));

    /// <summary>Цвет имени атрибута.</summary>
    public static readonly StyledProperty<IBrush?> AttributeBrushProperty =
        AvaloniaProperty.Register<AxCodeBlock, IBrush?>(nameof(AttributeBrush));

    /// <summary>Цвет строкового значения.</summary>
    public static readonly StyledProperty<IBrush?> StringBrushProperty =
        AvaloniaProperty.Register<AxCodeBlock, IBrush?>(nameof(StringBrush));

    /// <summary>Цвет комментария.</summary>
    public static readonly StyledProperty<IBrush?> CommentBrushProperty =
        AvaloniaProperty.Register<AxCodeBlock, IBrush?>(nameof(CommentBrush));

    private SelectableTextBlock? _presenter;

    static AxCodeBlock()
    {
        // Любая правка текста или кистей перекладывает подсветку заново;
        // кисти меняются при переключении варианта темы.
        TextProperty.Changed.AddClassHandler<AxCodeBlock>((block, _) => block.Render());
        TagBrushProperty.Changed.AddClassHandler<AxCodeBlock>((block, _) => block.Render());
        AttributeBrushProperty.Changed.AddClassHandler<AxCodeBlock>((block, _) => block.Render());
        StringBrushProperty.Changed.AddClassHandler<AxCodeBlock>((block, _) => block.Render());
        CommentBrushProperty.Changed.AddClassHandler<AxCodeBlock>((block, _) => block.Render());
    }

    /// <inheritdoc cref="TextProperty"/>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
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

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _presenter = e.NameScope.Find<SelectableTextBlock>("PART_Text");
        Render();
    }

    private void Render()
    {
        if (_presenter is null)
            return;

        var inlines = new InlineCollection();

        foreach (var (kind, piece) in Tokenize(Text ?? string.Empty))
        {
            var run = new Run(piece);

            var brush = kind switch
            {
                TokenKind.Tag => TagBrush,
                TokenKind.Attribute => AttributeBrush,
                TokenKind.String => StringBrush,
                TokenKind.Comment => CommentBrush,
                _ => null,
            };

            if (brush is not null)
                run.Foreground = brush;

            inlines.Add(run);
        }

        _presenter.Inlines = inlines;
    }

    private enum TokenKind
    {
        Plain,
        Tag,
        Attribute,
        String,
        Comment,
    }

    /// <summary>
    /// Режет разметку на куски по виду: тег со скобками, атрибут, строка в
    /// кавычках, комментарий, остальное.
    /// </summary>
    private static IEnumerable<(TokenKind Kind, string Text)> Tokenize(string text)
    {
        var i = 0;

        while (i < text.Length)
        {
            if (text[i] == '<' && text.AsSpan(i).StartsWith("<!--"))
            {
                var end = text.IndexOf("-->", i, StringComparison.Ordinal);
                end = end < 0 ? text.Length : end + 3;
                yield return (TokenKind.Comment, text[i..end]);
                i = end;
                continue;
            }

            if (text[i] == '<')
            {
                // Имя тега вместе со скобкой и слэшами: до пробела или '>'.
                var j = i + 1;
                while (j < text.Length && (text[j] == '/' || char.IsLetterOrDigit(text[j]) || text[j] is '.' or ':' or '-'))
                    j++;
                if (j < text.Length && text[j] == '>')
                    j++;
                yield return (TokenKind.Tag, text[i..j]);
                i = j;

                // Внутри тега: атрибуты и строки до закрывающей скобки.
                while (i < text.Length && text[i - 1] != '>')
                {
                    if (text[i] == '"')
                    {
                        var q = text.IndexOf('"', i + 1);
                        q = q < 0 ? text.Length - 1 : q;
                        yield return (TokenKind.String, text[i..(q + 1)]);
                        i = q + 1;
                    }
                    else if (char.IsLetter(text[i]))
                    {
                        var j2 = i;
                        while (j2 < text.Length && (char.IsLetterOrDigit(text[j2]) || text[j2] is '.' or ':' or '-'))
                            j2++;
                        yield return (TokenKind.Attribute, text[i..j2]);
                        i = j2;
                    }
                    else if (text[i] is '>' || (text[i] is '/' && i + 1 < text.Length && text[i + 1] == '>'))
                    {
                        var j2 = text.IndexOf('>', i) + 1;
                        j2 = j2 == 0 ? text.Length : j2;
                        yield return (TokenKind.Tag, text[i..j2]);
                        i = j2;
                        break;
                    }
                    else
                    {
                        yield return (TokenKind.Plain, text[i].ToString());
                        i++;
                    }
                }

                continue;
            }

            var next = text.IndexOf('<', i);
            next = next < 0 ? text.Length : next;
            yield return (TokenKind.Plain, text[i..next]);
            i = next;
        }
    }
}
