using Avalonia.Controls;
using Avalonia.Input;

namespace ArxisStudio.Controls;

/// <summary>
/// Список студии с выделением строк.
/// </summary>
public class AxListBox : ListBox
{
    /// <summary>Заводит список и объявляет его областью выделения.</summary>
    /// <remarks>
    /// Полным цветом строка горит, пока клавиатура в этом списке, — а не в любом предке, как было
    /// со стилем по <c>:focus-within</c>: вложенный список загорался от фокуса внешнего.
    /// </remarks>
    public AxListBox() => AxSelectionScope.Track(this);

    private readonly AxRowKeys _keys = new();

    /// <inheritdoc/>
    /// <remarks>
    /// Набор букв ведёт выбор к строке, которая с них начинается: список в сотню строк не листают
    /// стрелкой по одной. Подпись строки берётся у её контейнера, а у нерождённого — у самого
    /// элемента: список виртуализован, и строки за краем окна контейнера не имеют.
    /// </remarks>
    protected override void OnTextInput(TextInputEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnTextInput(e);

        if (e.Handled || e.Text is not { Length: > 0 } typed || char.IsControl(typed[0]))
            return;

        var at = _keys.Match(typed, Label, ItemCount, SelectedIndex);

        if (at < 0)
            return;

        SelectedIndex = at;
        ScrollIntoView(at);

        // Каретка идёт за выбором: строку, с которой она ушла, список мог и переработать — она
        // уехала за край, — и следующая буква не досталась бы никому.
        (ContainerFromIndex(at) as Control)?.Focus(NavigationMethod.Tab);

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
        => new AxListBoxItem();

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        => NeedsContainer<AxListBoxItem>(item, out recycleKey);

    /// <summary>Подпись строки: надпись в ней, а у нерождённой — сам элемент.</summary>
    private string? Label(int at) =>
        ContainerFromIndex(at) is { } row ? AxRowKeys.Label(row) : ItemsView[at]?.ToString();
}
