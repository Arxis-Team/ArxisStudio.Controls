using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace ArxisStudio.Controls;

/// <summary>
/// Полный текст подписи, сокращённой многоточием, — подсказкой, и только пока
/// подпись действительно сокращена.
/// </summary>
/// <remarks>
/// Подсказка, повторяющая целую подпись, — шум: человек читает одно и то же
/// дважды. Поэтому решается не заранее, а в миг открытия: сокращена ли сейчас
/// хоть одна строка текста внутри контрола. Раскладку при этом никто не
/// сторожит — ширина панели, язык и кегль меняются сколько угодно, а ответ
/// всегда свежий.
/// <para>
/// Об открытии Avalonia объявляет, только когда подсказка у контрола есть,
/// поэтому тема ставит вместе с правилом пустую подсказку. Подсказку, данную
/// контролу явно, правило не трогает: она говорит своё, а не повторяет подпись.
/// </para>
/// </remarks>
public static class AxTrimmedTip
{
    /// <summary>Показывать ли полный текст сокращённой подписи подсказкой.</summary>
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>("IsEnabled", typeof(AxTrimmedTip));

    /// <summary>Текст, который правило само положило в подсказку в последний раз.</summary>
    private static readonly AttachedProperty<string?> OwnTextProperty =
        AvaloniaProperty.RegisterAttached<Control, string?>("OwnText", typeof(AxTrimmedTip));

    static AxTrimmedTip()
    {
        IsEnabledProperty.Changed.AddClassHandler<Control>((control, change) =>
        {
            if (change.GetNewValue<bool>())
                ToolTip.AddToolTipOpeningHandler(control, Opening);
            else
                ToolTip.RemoveToolTipOpeningHandler(control, Opening);
        });
    }

    /// <summary>Показывает ли контрол полный текст сокращённой подписи подсказкой.</summary>
    /// <param name="control">Контрол.</param>
    public static bool GetIsEnabled(Control control)
    {
        ArgumentNullException.ThrowIfNull(control);

        return control.GetValue(IsEnabledProperty);
    }

    /// <summary>Включает или снимает подсказку с полным текстом сокращённой подписи.</summary>
    /// <param name="control">Контрол.</param>
    /// <param name="value">Показывать ли.</param>
    public static void SetIsEnabled(Control control, bool value)
    {
        ArgumentNullException.ThrowIfNull(control);

        control.SetValue(IsEnabledProperty, value);
    }

    private static void Opening(object? sender, CancelRoutedEventArgs e)
    {
        if (sender is not Control control || !ReferenceEquals(e.Source, control))
            return;

        var tip = ToolTip.GetTip(control);

        if (tip is not "" && !Equals(tip, control.GetValue(OwnTextProperty)))
            return;

        var trimmed = control.GetSelfAndVisualDescendants()
            .OfType<TextBlock>()
            .FirstOrDefault(text => text.IsEffectivelyVisible && text.TextLayout.TextLines.Any(line => line.HasCollapsed))
            ?.Text;

        if (trimmed is null)
        {
            e.Cancel = true;
            return;
        }

        control.SetValue(OwnTextProperty, trimmed);
        ToolTip.SetTip(control, trimmed);
    }
}
