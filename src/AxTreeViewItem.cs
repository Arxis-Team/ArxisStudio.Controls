using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace ArxisStudio.Controls;

/// <summary>
/// Строка дерева со стрелкой раскрытия, значком и отступом по уровню.
/// </summary>
/// <remarks>
/// Значок — путь в клетке 16 и его цвет, как у вкладки. Прежде значком было любое
/// содержимое, и рядом с глифом папки стояли плашки типа файла со своими размерами:
/// клетка набора ложится в пиксели, а плашка — как получится. Тип документа
/// различают цветом значка, <see cref="IconBrush"/>.
/// </remarks>
[PseudoClasses(":selection-active")]
public class AxTreeViewItem : TreeViewItem
{
    static AxTreeViewItem() =>
        AxSelectionScope.IsActiveProperty.Changed.AddClassHandler<AxTreeViewItem>(
            (item, change) => item.PseudoClasses.Set(":selection-active", change.GetNewValue<bool>()));

    /// <summary>Значок слева от подписи.</summary>
    public static readonly StyledProperty<Geometry?> IconProperty =
        AvaloniaProperty.Register<AxTreeViewItem, Geometry?>(nameof(Icon));

    /// <summary>Цвет значка; без него значок идёт цветом иконки темы.</summary>
    public static readonly StyledProperty<IBrush?> IconBrushProperty =
        AvaloniaProperty.Register<AxTreeViewItem, IBrush?>(nameof(IconBrush));

    /// <inheritdoc cref="IconProperty"/>
    public Geometry? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <inheritdoc cref="IconBrushProperty"/>
    public IBrush? IconBrush
    {
        get => GetValue(IconBrushProperty);
        set => SetValue(IconBrushProperty, value);
    }

    /// <summary>
    /// Двойной щелчок по строке раскрывает и сворачивает узел.
    /// </summary>
    /// <remarks>
    /// Попасть в стрелку шириной 12 куда труднее, чем в строку, и во всяком
    /// файловом дереве это привычный способ.
    ///
    /// Слушаем жест, а не считаем нажатия сами: у нажатия счёт щелчков ведёт
    /// платформа, и подряд идущие двойные щелчки продолжают его — третьим,
    /// четвёртым, — а жест каждый раз приходит ровно один.
    /// </remarks>
    public AxTreeViewItem() => AddHandler(DoubleTappedEvent, OnRowDoubleTapped);

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        => new AxTreeViewItem();

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        => NeedsContainer<AxTreeViewItem>(item, out recycleKey);

    /// <inheritdoc/>
    /// <remarks>
    /// Пир строки дерева в Avalonia 12 раскрытия не знает, и экранный диктор не слышал у узла ни
    /// «свёрнуто», ни «развёрнуто», а раскрыть его своей командой не мог вовсе. Раскрытие — своё,
    /// выбор и дети — от пира Avalonia.
    /// </remarks>
    protected override AutomationPeer OnCreateAutomationPeer() => new NodePeer(this);

    /// <inheritdoc/>
    /// <remarks>Смену раскрытия диктор узнаёт от пира: иначе услышал бы её, только вернувшись к строке.</remarks>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        ArgumentNullException.ThrowIfNull(change);

        base.OnPropertyChanged(change);

        if (change.Property == IsExpandedProperty && ItemCount > 0 && ControlAutomationPeer.FromElement(this) is { } peer)
            peer.RaisePropertyChangedEvent(
                ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
                State(change.GetOldValue<bool>()),
                State(change.GetNewValue<bool>()));
    }

    /// <summary>Как раскрытие читает диктор: лист, свёрнут или развёрнут.</summary>
    private ExpandCollapseState State(bool expanded) =>
        ItemCount == 0 ? ExpandCollapseState.LeafNode
        : expanded ? ExpandCollapseState.Expanded
        : ExpandCollapseState.Collapsed;

    private void OnRowDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (ItemCount == 0 || e.Source is not Visual source)
            return;

        // Складывается своя строка. Жест поднимается из глубины наружу, и без
        // этой проверки двойной щелчок по файлу свернул бы папку над ним, а
        // следом и всё дерево.
        if (source.FindAncestorOfType<AxTreeViewItem>(includeSelf: true) != this)
            return;

        // Щелчок по стрелке — дело самой стрелки: она уже переключилась дважды,
        // и третье переключение отсюда вернуло бы узел не туда.
        if (source.FindAncestorOfType<Button>(includeSelf: true) is not null)
            return;

        IsExpanded = !IsExpanded;
        e.Handled = true;
    }

    /// <summary>Узел дерева для диктора: раскрывается и сворачивается так же, как стрелкой.</summary>
    private sealed class NodePeer(AxTreeViewItem owner) : TreeViewItemAutomationPeer(owner), IExpandCollapseProvider
    {
        public ExpandCollapseState ExpandCollapseState => owner.State(owner.IsExpanded);

        public bool ShowsMenu => false;

        public void Expand()
        {
            if (owner.ItemCount > 0)
                owner.SetCurrentValue(IsExpandedProperty, true);
        }

        public void Collapse()
        {
            if (owner.ItemCount > 0)
                owner.SetCurrentValue(IsExpandedProperty, false);
        }
    }
}
