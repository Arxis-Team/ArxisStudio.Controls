using Avalonia;
using Avalonia.Controls;

namespace ArxisStudio.Controls;

/// <summary>
/// Подсказка «Понятно»: карточка с заголовком, объяснением, счётчиком шагов
/// и действием. Показывается внутри Flyout у контрола, о котором рассказывает;
/// закрытие — забота показавшего: он знает, когда шаг пройден.
/// </summary>
public class AxTeachingTip : ContentControl
{
    /// <summary>Заголовок подсказки.</summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<AxTeachingTip, string?>(nameof(Title));

    /// <summary>Счётчик шагов вида «1/3»; пусто — подсказка одиночная.</summary>
    public static readonly StyledProperty<string?> StepTextProperty =
        AvaloniaProperty.Register<AxTeachingTip, string?>(nameof(StepText));

    /// <summary>Действие внизу карточки — обычно кнопка «Понятно».</summary>
    public static readonly StyledProperty<object?> ActionsProperty =
        AvaloniaProperty.Register<AxTeachingTip, object?>(nameof(Actions));

    /// <inheritdoc cref="TitleProperty"/>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <inheritdoc cref="StepTextProperty"/>
    public string? StepText
    {
        get => GetValue(StepTextProperty);
        set => SetValue(StepTextProperty, value);
    }

    /// <inheritdoc cref="ActionsProperty"/>
    public object? Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }
}
