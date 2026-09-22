using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

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
    /// <remarks>
    /// Стрелки, Home, End и листание страницей список ведёт сам. База Avalonia 12 двигает выбор теми
    /// же правилами, но каретку отдаёт соседу без способа — <c>MoveSelection</c> зовёт <c>Focus()</c>, —
    /// а кольцо фокуса горит только у пришедшего с клавиатуры: на первой же стрелке оно гасло. Выбор
    /// здесь идёт дорогой базы, каретка — способом <see cref="NavigationMethod.Directional"/>.
    /// </remarks>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        // База стрелку не проверяет на обработанность и сдвинула бы выбор второй раз.
        if (!e.Handled && Step(e))
        {
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);

        if (e.Handled || !AxRowKeys.AsksForMenu(e))
            return;

        AxRowKeys.AskForMenu(this);

        e.Handled = true;
    }

    /// <summary>Ведёт выбор и каретку клавишей направления — как база, но с кольцом фокуса.</summary>
    /// <returns>
    /// Ушёл ли выбор. С Ctrl стрелка базы ведёт одну каретку, у края списка идти некуда — такие
    /// клавиши остаются базе, как были.
    /// </returns>
    private bool Step(KeyEventArgs e)
    {
        if (e.Key.ToNavigationDirection() is not { } direction || !direction.IsDirectional()
            || TopLevel.GetTopLevel(this) is not { } top
            || (this.GetPlatformSettings()?.HotkeyConfiguration is { } hotkeys
                && (e.KeyModifiers & hotkeys.CommandModifiers) == hotkeys.CommandModifiers)
            || Presenter?.Panel is not INavigableContainer panel)
        {
            return false;
        }

        var from = GetContainerFromEventSource(top.FocusManager?.GetFocusedElement()) ?? ContainerFromIndex(Selection.AnchorIndex);

        // Каретки в списке нет — стрелка начинает с края, как у базы.
        if (from is null)
        {
            direction = direction switch
            {
                NavigationDirection.Down or NavigationDirection.Right => NavigationDirection.First,
                NavigationDirection.Up or NavigationDirection.Left => NavigationDirection.Last,
                _ => direction,
            };
        }

        if (GetNextControl(panel, direction, from, WrapSelection) is not Control next)
            return false;

        var at = IndexFromContainer(next);

        if (at < 0)
            return false;

        UpdateSelection(at, true, (e.KeyModifiers & KeyModifiers.Shift) != 0);
        next.Focus(NavigationMethod.Directional, e.KeyModifiers);

        return true;
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
