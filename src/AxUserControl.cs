using Avalonia.Controls;

namespace ArxisStudio.Controls;

/// <summary>
/// Корень разметки: с него начинается панель, написанная на <c>.axaml</c>.
/// </summary>
/// <remarks>
/// Своего шаблона у него нет — это <see cref="UserControl"/> студии. Нужен он
/// затем, что корень панели иначе оказался бы единственным голым виджетом
/// Avalonia в интерфейсе расширения: теме не за что зацепиться, и панель
/// плагина отличалась бы от соседней панели студии фоном и кистью текста.
/// </remarks>
public class AxUserControl : UserControl
{
}
