using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace ArxisStudio.Controls;

/// <summary>
/// Дерево: иерархия документа, файлы проекта, структура решения. Строка —
/// <see cref="AxTreeViewItem"/> высотой в строку списка.
/// </summary>
public class AxTreeView : TreeView
{
    /// <summary>Заводит дерево и объявляет его областью выделения.</summary>
    /// <remarks>
    /// Выбранная строка горит полным цветом, пока клавиатура в этом дереве. Вложенных деревьев у
    /// студии нет, но есть контекстное меню над выбранной строкой: со стилем по
    /// <c>:focus-within</c> выделение гасло ровно в тот миг, когда человек выбирает над ним
    /// действие.
    /// </remarks>
    public AxTreeView() => AxSelectionScope.Track(this);

    private readonly AxRowKeys _keys = new();

    /// <inheritdoc/>
    /// <remarks>
    /// Набор ищет по раскрытой части дерева — по тому, что человек видит: свёрнутая ветка прячет
    /// своих детей и от глаз, и от поиска, а раскрывать её без спроса значит менять дерево под
    /// рукой у того, кто просил всего лишь найти строку.
    /// </remarks>
    protected override void OnTextInput(TextInputEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnTextInput(e);

        if (e.Handled || e.Text is not { Length: > 0 } typed || char.IsControl(typed[0]))
            return;

        var rows = Rows();
        var at = _keys.Match(typed, number => AxRowKeys.Label(rows[number]), rows.Count, rows.FindIndex(row => row.IsSelected));

        if (at < 0)
            return;

        rows[at].IsSelected = true;
        rows[at].BringIntoView();

        // Каретка идёт за выбором: иначе следующая буква уходит туда, где выбора уже нет.
        rows[at].Focus(NavigationMethod.Tab);

        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnKeyDown(e);

        if (e.Handled || !AxRowKeys.AsksForMenu(e))
            return;

        AxRowKeys.AskForMenu(this);

        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        => new AxTreeViewItem();

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        => NeedsContainer<AxTreeViewItem>(item, out recycleKey);

    /// <summary>Строки дерева сверху вниз — те, что сейчас на экране.</summary>
    /// <remarks>
    /// Порядок визуального дерева и есть порядок строк: ветка идёт раньше своих детей, а
    /// свёрнутая детей не рождает вовсе.
    /// </remarks>
    private List<TreeViewItem> Rows() => [.. this.GetVisualDescendants().OfType<TreeViewItem>()];
}
