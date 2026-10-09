using System.Globalization;
using ArxisStudio.Controls;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace Controls.Gallery;

/// <summary>
/// Поля инспектора в одной половине галереи: значения карточек и строка последней фиксации.
/// </summary>
public partial class InspectorFieldCards : UserControl
{
    /// <summary>Подписи сторон отступа.</summary>
    public static readonly IReadOnlyList<string> Sides = ["L", "T", "R", "B"];

    /// <summary>Подписи осей точки.</summary>
    public static readonly IReadOnlyList<string> Axes = ["X", "Y"];

    /// <summary>Отступ «4,8».</summary>
    public static readonly IReadOnlyList<double> Offsets = [4, 8, 4, 8];

    /// <summary>Точка на холсте.</summary>
    public static readonly IReadOnlyList<double> Location = [120, 48];

    /// <summary>Отступ, одинаковый со всех сторон.</summary>
    public static readonly IReadOnlyList<double> Uniform = [6, 6, 6, 6];

    /// <summary>Классы стиля кнопки.</summary>
    public static readonly IReadOnlyList<string> StyleClasses = ["primary", "large"];

    /// <summary>Создаёт карточки и слушает фиксации маленького инспектора.</summary>
    public InspectorFieldCards()
    {
        InitializeComponent();

        // Колонка «Focused» показывает каретку в поле, а фокус в окне один на две половины галереи:
        // состояние ставится тем же псевдоклассом, которым его включает тема.
        FocusedNumber.AttachedToVisualTree += (_, _) =>
            ((IPseudoClasses)FocusedNumber.Classes).Set(":focus", true);

        Inspector.AddHandler(AxNumberField.ValueCommittedEvent, (_, e) =>
            Tell(e.Source, AxNumberField.Format(e.Value)));
        Inspector.AddHandler(AxVectorField.ValuesCommittedEvent, (_, e) =>
            Tell(e.Source, string.Join(",", e.Values.Select(value => AxNumberField.Format(value)))));
        Inspector.AddHandler(AxTokenField.TokensChangedEvent, (_, e) =>
            Tell(e.Source, string.Join(" ", e.Tokens)));
        Inspector.AddHandler(AxColorPicker.ColorCommittedEvent, (_, e) =>
            Tell(e.Source, e.Text ?? AxColorField.Format(e.Color)));
    }

    /// <summary>Пишет под инспектором, что и где зафиксировано.</summary>
    private void Tell(object? source, string value)
    {
        var row = (source as Control)?.FindAncestorOfType<AxPropertyRow>(includeSelf: true);
        var name = row?.Label as string ?? "?";

        LastCommit.Text = string.Create(CultureInfo.InvariantCulture, $"Зафиксировано: {name} = {value}");
    }
}
