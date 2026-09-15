using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace ArxisStudio.Controls;

/// <summary>
/// Выпадающий список студии.
/// </summary>
[PseudoClasses(":compact")]
public class AxComboBox : ComboBox
{
    /// <summary>Размер: обычный или тесный — для шапки и полосы панели.</summary>
    public static readonly StyledProperty<AxControlSize> SizeProperty =
        AxButton.SizeProperty.AddOwner<AxComboBox>();

    static AxComboBox() =>
        SizeProperty.Changed.AddClassHandler<AxComboBox>((box, change) =>
            AxLook.Mark(box.PseudoClasses, change.GetNewValue<AxControlSize>()));

    /// <inheritdoc cref="SizeProperty"/>
    public AxControlSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        => new AxComboBoxItem();

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        => NeedsContainer<AxComboBoxItem>(item, out recycleKey);
}
