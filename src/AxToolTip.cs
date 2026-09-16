using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace ArxisStudio.Controls;

/// <summary>
/// Жест команды рядом с подсказкой.
/// </summary>
/// <remarks>
/// Кнопка полосы несёт один значок, и как её зовут с клавиатуры, узнать было негде: подсказка под
/// курсором называла действие, а сочетание пряталось в настройках. В Rider и в Visual Studio жест
/// стоит в той же подсказке — там его и читают, когда надоедает тянуться к мыши.
/// <para>
/// Свойство наследуемое, и это не хитрость, а единственная дорога: подсказку показывает отдельное
/// окно, и своего жеста у него нет — как нет и у него же своего контекста данных. Оба приходят
/// сверху, от того контрола, над которым стоит курсор.
/// </para>
/// <para>
/// Жест хранится как <see cref="KeyGesture"/>, а не строкой: писать его словами — дело показа, и
/// на macOS та же команда пишется знаками модификаторов.
/// </para>
/// </remarks>
public static class AxToolTip
{
    /// <summary>Каким сочетанием зовут то, что делает этот контрол.</summary>
    public static readonly AttachedProperty<KeyGesture?> GestureProperty =
        AvaloniaProperty.RegisterAttached<Control, KeyGesture?>("Gesture", typeof(AxToolTip), inherits: true);

    /// <summary>Читает жест, объявленный контролу или унаследованный им.</summary>
    /// <param name="control">Контрол или содержимое подсказки над ним.</param>
    public static KeyGesture? GetGesture(Control control)
    {
        ArgumentNullException.ThrowIfNull(control);

        return control.GetValue(GestureProperty);
    }

    /// <summary>Объявляет жест контролу.</summary>
    /// <param name="control">Контрол, у которого есть подсказка.</param>
    /// <param name="value">Сочетание; <c>null</c> — жеста нет.</param>
    public static void SetGesture(Control control, KeyGesture? value)
    {
        ArgumentNullException.ThrowIfNull(control);

        control.SetValue(GestureProperty, value);
    }
}
