using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace ArxisStudio.Controls;

/// <summary>
/// Однострочное поле ввода студии.
/// </summary>
[PseudoClasses(":compact")]
public class AxTextBox : TextBox
{
    /// <summary>Размер: обычный или тесный — для шапки и полосы панели.</summary>
    public static readonly StyledProperty<AxControlSize> SizeProperty =
        AxButton.SizeProperty.AddOwner<AxTextBox>();

    static AxTextBox() =>
        SizeProperty.Changed.AddClassHandler<AxTextBox>((field, change) =>
            AxLook.Mark(field.PseudoClasses, change.GetNewValue<AxControlSize>()));

    /// <inheritdoc cref="SizeProperty"/>
    public AxControlSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }
}
