using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace ArxisStudio.Controls;

/// <summary>
/// Поле поиска: <see cref="AxTextBox"/> со значком поиска слева и крестиком
/// очистки справа.
/// </summary>
/// <remarks>
/// Крестик — не украшение, а единственный способ отменить поиск, не выделяя
/// набранное. Появляется он от текста и вместе с текстом исчезает: пустое поле
/// с крестиком предлагает стереть то, чего нет. Показом занимается тема,
/// поведением — контрол.
/// <para>
/// Кнопка — часть шаблона поля, и берётся она по ссылке. Прежде она жила в
/// <c>InnerRightContent</c>, своей области имён, и поле узнавало её по имени во
/// всплывшем нажатии: кнопка с тем же именем в чужом содержимом стирала бы запрос.
/// </para>
/// </remarks>
[TemplatePart(ClearPart, typeof(Button))]
public class AxSearchField : AxTextBox
{
    /// <summary>Имя кнопки очистки в теме.</summary>
    private const string ClearPart = "PART_Clear";

    private Button? _clear;

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnApplyTemplate(e);

        if (_clear is not null)
            _clear.Click -= OnClick;

        _clear = e.NameScope.Find<Button>(ClearPart);

        if (_clear is not null)
            _clear.Click += OnClick;
    }

    private void OnClick(object? sender, RoutedEventArgs e)
    {
        Clear();

        // Курсор остаётся в поле: человек стёр запрос, чтобы набрать другой,
        // а не чтобы уйти отсюда.
        Focus();
        e.Handled = true;
    }
}
