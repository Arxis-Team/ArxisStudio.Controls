using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace ArxisStudio.Controls;

/// <summary>
/// Выпадающий список студии.
/// </summary>
/// <remarks>
/// Список, в который можно набрать своё (<see cref="ComboBox.IsEditable"/>), ставит <c>:editing</c>, пока каретка в его
/// поле ввода: рамку и кольцо рисует список целиком, как у поля жетонов, — человек пишет в список, а не в строку
/// внутри него.
/// </remarks>
[TemplatePart(InputPart, typeof(TextBox))]
[PseudoClasses(":compact", ":editing")]
public class AxComboBox : ComboBox
{
    /// <summary>Размер: обычный или тесный — для шапки и полосы панели.</summary>
    public static readonly StyledProperty<AxControlSize> SizeProperty =
        AxButton.SizeProperty.AddOwner<AxComboBox>();

    /// <summary>Имя поля ввода в теме — то, которое находит и <see cref="ComboBox"/>.</summary>
    private const string InputPart = "PART_EditableTextBox";

    private TextBox? _input;

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
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnApplyTemplate(e);

        if (_input is not null)
        {
            _input.GotFocus -= OnInputFocus;
            _input.LostFocus -= OnInputBlur;
        }

        _input = e.NameScope.Find<TextBox>(InputPart);
        PseudoClasses.Set(":editing", _input?.IsFocused == true);

        if (_input is not null)
        {
            _input.GotFocus += OnInputFocus;
            _input.LostFocus += OnInputBlur;
        }
    }

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        => new AxComboBoxItem();

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        => NeedsContainer<AxComboBoxItem>(item, out recycleKey);

    private void OnInputFocus(object? sender, FocusChangedEventArgs e) => PseudoClasses.Set(":editing", true);

    private void OnInputBlur(object? sender, FocusChangedEventArgs e) => PseudoClasses.Set(":editing", false);
}
