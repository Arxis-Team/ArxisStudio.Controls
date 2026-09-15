using Avalonia;
using Avalonia.Controls;

namespace ArxisStudio.Controls;

/// <summary>Что проверка сказала о значении поля.</summary>
public enum AxValidationState
{
    /// <summary>Сказать нечего.</summary>
    None,

    /// <summary>Значение годится, но стоит взглянуть.</summary>
    Warning,

    /// <summary>Значение не годится.</summary>
    Error,
}

/// <summary>
/// Состояние проверки у поля, флажка или списка — присоединённым свойством.
/// </summary>
/// <remarks>
/// Поле с ошибкой видно по контуру и без фокуса: проблему человек должен увидеть сразу, а
/// не когда вернётся в поле. Прежде это были классы <c>error</c> и <c>warning</c>, и
/// опечатка в них оставляла поле обычным. Тема видит состояние псевдоклассами
/// <c>:validation-warning</c> и <c>:validation-error</c>; свои, а не <c>:error</c>
/// Avalonia, — тот ставит и снимает проверка привязки, и сказанное здесь она бы стёрла.
/// </remarks>
public static class AxValidation
{
    /// <summary>Состояние проверки.</summary>
    public static readonly AttachedProperty<AxValidationState> StateProperty =
        AvaloniaProperty.RegisterAttached<Control, AxValidationState>("State", typeof(AxValidation));

    static AxValidation()
    {
        StateProperty.Changed.AddClassHandler<Control>((control, change) =>
            AxLook.Mark(control.Classes, "validation", change.GetOldValue<AxValidationState>(), change.GetNewValue<AxValidationState>(), AxValidationState.None));
    }

    /// <summary>Состояние проверки контрола.</summary>
    /// <param name="control">Контрол.</param>
    public static AxValidationState GetState(Control control)
    {
        ArgumentNullException.ThrowIfNull(control);

        return control.GetValue(StateProperty);
    }

    /// <summary>Задаёт состояние проверки контрола.</summary>
    /// <param name="control">Контрол.</param>
    /// <param name="value">Состояние.</param>
    public static void SetState(Control control, AxValidationState value)
    {
        ArgumentNullException.ThrowIfNull(control);

        control.SetValue(StateProperty, value);
    }
}
