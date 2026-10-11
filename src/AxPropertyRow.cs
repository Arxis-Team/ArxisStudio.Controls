using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace ArxisStudio.Controls;

/// <summary>Как строка свойства ставит поле относительно подписи.</summary>
public enum AxPropertyRowLayout
{
    /// <summary>Подпись колонкой слева, поле справа; тесно — поле уходит под подпись само.</summary>
    Inline,

    /// <summary>Поле всегда под подписью, во всю ширину: сегменты, многострочный текст.</summary>
    Below,

    /// <summary>Подписи нет, поле во всю ширину: редактор, который подписывает себя сам.</summary>
    Full,
}

/// <summary>
/// Строка свойства: подпись колонкой, поле, кнопки действий и заметка под полем — как строка инспектора Unity.
/// </summary>
/// <remarks>
/// <para>
/// <b>Колонка подписей одна на список.</b> Ширину подписи задаёт наследуемое <see cref="LabelWidthProperty"/>,
/// поставленное на список строк: тогда подписи всех разделов стоят одной колонкой, а отступ вложенной строки
/// съедает подпись, а не сдвигает поле. Не задано — строка считает ширину сама, по-юнитиевски: 0,45 своей
/// ширины без отступа, но не уже наименьшей (<see cref="AxPropertyRowPanel"/>).
/// </para>
/// <para>
/// <b>Тесно — поле уходит под подпись</b>, как в окне настроек: поле уже наименьшего — это уже не поле, а
/// прорезь.
/// </para>
/// <para>
/// <b>Задано здесь</b> (<see cref="IsOverridden"/>) — полужирная подпись: так Unity отмечает значение,
/// переопределённое в экземпляре префаба, а инспектор студии — значение, которое написал сам элемент разметки,
/// а не стиль или предок. Полосы у края, как у Unity, тема не ставит: в списке, где задана половина строк, они
/// рябят столбиком и спорят с меткой выбранной строки.
/// </para>
/// <para>
/// <b>Подпись тянет число</b> (<see cref="ScrubTarget"/>): числовое поле протягивается за подпись, как у Unity,
/// а щелчок по ней ставит каретку в поле.
/// </para>
/// </remarks>
[PseudoClasses(":overridden", ":readonly", ":full")]
public class AxPropertyRow : ContentControl
{
    /// <summary>Ширина колонки подписей — наследуется от списка строк; не число — строка считает сама.</summary>
    public static readonly AttachedProperty<double> LabelWidthProperty =
        AvaloniaProperty.RegisterAttached<AxPropertyRow, Control, double>("LabelWidth", double.NaN, inherits: true);

    /// <summary>Подпись.</summary>
    public static readonly StyledProperty<object?> LabelProperty =
        AvaloniaProperty.Register<AxPropertyRow, object?>(nameof(Label));

    /// <summary>Заметка под полем: почему значение не годится, откуда оно, что значит.</summary>
    public static readonly StyledProperty<object?> NoteProperty =
        AvaloniaProperty.Register<AxPropertyRow, object?>(nameof(Note));

    /// <summary>Кнопки справа от поля: сбросить, меню.</summary>
    public static readonly StyledProperty<object?> ActionsProperty =
        AvaloniaProperty.Register<AxPropertyRow, object?>(nameof(Actions));

    /// <summary>Значение задано здесь, а не пришло со стороны: полужирная подпись.</summary>
    public static readonly StyledProperty<bool> IsOverriddenProperty =
        AvaloniaProperty.Register<AxPropertyRow, bool>(nameof(IsOverridden));

    /// <summary>Значение только читается: подпись вторичным тоном, поле — выделяемым текстом, а не выключенным.</summary>
    /// <remarks>Поле строка не трогает: только чтение ему ставит тот, кто его построил.</remarks>
    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<AxPropertyRow, bool>(nameof(IsReadOnly));

    /// <summary>Как поле стоит относительно подписи.</summary>
    public static readonly StyledProperty<AxPropertyRowLayout> LayoutProperty =
        AvaloniaProperty.Register<AxPropertyRow, AxPropertyRowLayout>(nameof(Layout));

    /// <summary>Уровень вложенности: каждый сдвигает подпись на ступень дерева.</summary>
    public static readonly StyledProperty<int> IndentProperty =
        AvaloniaProperty.Register<AxPropertyRow, int>(nameof(Indent));

    /// <summary>Числовое поле, которое тянет подпись; пусто — подпись не тянет.</summary>
    public static readonly StyledProperty<AxNumberField?> ScrubTargetProperty =
        AvaloniaProperty.Register<AxPropertyRow, AxNumberField?>(nameof(ScrubTarget));

    static AxPropertyRow()
    {
        IsOverriddenProperty.Changed.AddClassHandler<AxPropertyRow>((row, change) =>
            row.PseudoClasses.Set(":overridden", change.GetNewValue<bool>()));
        IsReadOnlyProperty.Changed.AddClassHandler<AxPropertyRow>((row, change) =>
            row.PseudoClasses.Set(":readonly", change.GetNewValue<bool>()));
        LayoutProperty.Changed.AddClassHandler<AxPropertyRow>((row, change) =>
            row.PseudoClasses.Set(":full", change.GetNewValue<AxPropertyRowLayout>() == AxPropertyRowLayout.Full));
    }

    /// <inheritdoc cref="LabelProperty"/>
    public object? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <inheritdoc cref="NoteProperty"/>
    public object? Note
    {
        get => GetValue(NoteProperty);
        set => SetValue(NoteProperty, value);
    }

    /// <inheritdoc cref="ActionsProperty"/>
    public object? Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }

    /// <inheritdoc cref="IsOverriddenProperty"/>
    public bool IsOverridden
    {
        get => GetValue(IsOverriddenProperty);
        set => SetValue(IsOverriddenProperty, value);
    }

    /// <inheritdoc cref="IsReadOnlyProperty"/>
    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <inheritdoc cref="LayoutProperty"/>
    public AxPropertyRowLayout Layout
    {
        get => GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }

    /// <inheritdoc cref="IndentProperty"/>
    public int Indent
    {
        get => GetValue(IndentProperty);
        set => SetValue(IndentProperty, value);
    }

    /// <inheritdoc cref="ScrubTargetProperty"/>
    public AxNumberField? ScrubTarget
    {
        get => GetValue(ScrubTargetProperty);
        set => SetValue(ScrubTargetProperty, value);
    }

    /// <summary>Ширина колонки подписей у списка строк.</summary>
    /// <param name="element">Список или строка.</param>
    public static double GetLabelWidth(Control element)
    {
        ArgumentNullException.ThrowIfNull(element);

        return element.GetValue(LabelWidthProperty);
    }

    /// <summary>Задаёт ширину колонки подписей списку строк.</summary>
    /// <param name="element">Список или строка.</param>
    /// <param name="value">Ширина; не число — каждая строка считает сама.</param>
    public static void SetLabelWidth(Control element, double value)
    {
        ArgumentNullException.ThrowIfNull(element);

        element.SetValue(LabelWidthProperty, value);
    }
}
