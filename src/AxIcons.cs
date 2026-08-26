using Avalonia.Media;

namespace ArxisStudio.Controls;

/// <summary>
/// Набор иконок студии: контурные пути в системе координат 16×16 для
/// <see cref="AxIcon"/>. Плагин может пользоваться этими же путями или дать свои.
/// </summary>
public static class AxIcons
{
    private static Geometry P(string path) => Geometry.Parse(path);

    /// <summary>Папка.</summary>
    public static Geometry Folder { get; } = P("M2.5 4.8h4l1.4 1.4h5.6v6.3H2.5z");

    /// <summary>Документ.</summary>
    public static Geometry Document { get; } = P("M4 2.5h5.5L12.5 6v7.5H4zM9.5 2.5V6h3");

    /// <summary>Окно приложения.</summary>
    public static Geometry Window { get; } = P("M2.5 3.5h11v9h-11zM2.5 6h11");

    /// <summary>Сетка (Grid).</summary>
    public static Geometry Grid { get; } = P("M3 3.5h10v9H3zM8 3.5v9M3 8h10");

    /// <summary>Лупа поиска.</summary>
    public static Geometry Search { get; } = P("M10.9 7.2a3.7 3.7 0 1 1-7.4 0 3.7 3.7 0 0 1 7.4 0M10.2 10.2 13 13");

    /// <summary>Плюс.</summary>
    public static Geometry Plus { get; } = P("M8 3.5v9M3.5 8h9");

    /// <summary>Шеврон вниз.</summary>
    public static Geometry ChevronDown { get; } = P("M4.2 6 8 9.8 11.8 6");

    /// <summary>Шеврон вправо.</summary>
    public static Geometry ChevronRight { get; } = P("M6 4.2 9.8 8 6 11.8");

    /// <summary>Ползунки настроек.</summary>
    public static Geometry Settings { get; } = P("M3 5h10M3 11h10M7.9 5a1.7 1.7 0 1 1-3.4 0 1.7 1.7 0 0 1 3.4 0M11.5 11a1.7 1.7 0 1 1-3.4 0 1.7 1.7 0 0 1 3.4 0");

    /// <summary>Шаблоны: рамка с точкой.</summary>
    public static Geometry Template { get; } = P("M3 3h10v10H3zM9.5 6.5a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0");

    /// <summary>Треугольник воспроизведения.</summary>
    public static Geometry Play { get; } = P("M5.5 4v8l7-4z");

    /// <summary>Ветка репозитория.</summary>
    public static Geometry Branch { get; } = P("M6.5 11.3a1.8 1.8 0 1 1-3.6 0 1.8 1.8 0 0 1 3.6 0M13.1 4.7a1.8 1.8 0 1 1-3.6 0 1.8 1.8 0 0 1 3.6 0M4.7 9.5V8a2 2 0 0 1 2-2h2.6a2 2 0 0 0 2-1.5");

    /// <summary>Галочка.</summary>
    public static Geometry Check { get; } = P("M3.5 8.5l3 3 6-7");

    /// <summary>Крестик: закрыть, сбросить.</summary>
    public static Geometry Close { get; } = P("M4 4l8 8M12 4l-8 8");

    /// <summary>Стрелка загрузки.</summary>
    public static Geometry Download { get; } = P("M8 3v7M5 7.5l3 3 3-3M3.5 12.5h9");

    /// <summary>Диалог (чат, сообщество).</summary>
    public static Geometry Bubble { get; } = P("M3 4h10v6.5H8L5 13v-2.5H3z");

    /// <summary>Панели дашборда.</summary>
    public static Geometry Dashboard { get; } = P("M3 3h4.5v4.5H3zM8.5 3H13v7H8.5zM3 8.5h4.5V13H3zM8.5 11H13v2H8.5z");

    /// <summary>Медиа: кадр с треугольником.</summary>
    public static Geometry Media { get; } = P("M2.5 3.5h11v9h-11zM6.5 6l3.5 2-3.5 2z");

    /// <summary>Изображение.</summary>
    public static Geometry Image { get; } = P("M2.5 3.5h11v9h-11zM2.5 10l3.5-3 3 2.5 2-1.5 2.5 2");

    /// <summary>Список.</summary>
    public static Geometry List { get; } = P("M3 3.5h10v9H3zM5.5 6.5h5M5.5 9.5h5");

    /// <summary>Таблица данных.</summary>
    public static Geometry DataGrid { get; } = P("M2.5 3.5h11v9h-11zM2.5 6.5h11M2.5 9.5h11M8 6.5v6");

    /// <summary>Вкладки.</summary>
    public static Geometry Tabs { get; } = P("M3 6.5h10v6H3zM3 6.5V3.5h4.5v3");

    /// <summary>Куб (пакет, плагин).</summary>
    public static Geometry Package { get; } = P("M8 2.2l5 2.9v5.8l-5 2.9-5-2.9V5.1z");

    /// <summary>Предупреждение: круг с восклицательным знаком.</summary>
    public static Geometry Warning { get; } = P("M13.5 8a5.5 5.5 0 1 1-11 0 5.5 5.5 0 0 1 11 0M8 5.5V8.5M8 10.6v.2");

    // ------------------------------------------------------------------
    // Семейство 1 — действия и объекты (спецификация иконок, раздел 7).
    // Сетка 16×16, обводка 1.2, дуги настоящие; имена по смыслу.
    // ------------------------------------------------------------------

// Раскрытие
    /// <summary>Шеврон вверх: свернуть раскрытое.</summary>
    public static Geometry ChevronUp { get; } = P(
        "M4.2 10 8 6.2 11.8 10");
    /// <summary>Шеврон влево: назад, свернуть панель.</summary>
    public static Geometry ChevronLeft { get; } = P(
        "M10 4.2 6.2 8 10 11.8");
    /// <summary>Мелкий шеврон вниз для тесной строки (класс small).</summary>
    public static Geometry ChevronDownSmall { get; } = P(
        "M5.6 6.9 8 9.3 10.4 6.9");
    /// <summary>Свернуть всё дерево.</summary>
    public static Geometry CollapseAll { get; } = P(
        "M3.5 3.5H12.5M3.5 12.5H12.5M5.9 5.2 8 7.3 10.1 5.2M5.9 10.8 8 8.7 10.1 10.8");
    /// <summary>Развернуть всё дерево.</summary>
    public static Geometry ExpandAll { get; } = P(
        "M3.5 3.5H12.5M3.5 12.5H12.5M5.9 7.3 8 5.2 10.1 7.3M5.9 8.7 8 10.8 10.1 8.7");

    // Действия
    /// <summary>Минус: убрать, уменьшить.</summary>
    public static Geometry Minus { get; } = P(
        "M3.5 8H12.5");
    /// <summary>Карандаш: переименовать, изменить.</summary>
    public static Geometry Edit { get; } = P(
        "M10.7 2.8 13.2 5.3 5.8 12.7H3.3V10.2ZM9.2 4.3 11.7 6.8");
    /// <summary>Корзина: удалить.</summary>
    public static Geometry Trash { get; } = P(
        "M3 4.5H13M6.3 4.5V3.2H9.7V4.5M4.6 4.5 5.2 12.9H10.8L11.4 4.5M7 7V10.6M9 7V10.6");
    /// <summary>Два листа: копировать.</summary>
    public static Geometry Copy { get; } = P(
        "M5.5 5.5H12.5V12.5H5.5ZM10.5 3.5H3.5V10.5");
    /// <summary>Отменить последнюю правку.</summary>
    public static Geometry Undo { get; } = P(
        "M3.5 6H10A3.5 3.5 0 0 1 10 13H7.5M3.5 6 6.5 3M3.5 6 6.5 9");
    /// <summary>Вернуть отменённую правку.</summary>
    public static Geometry Redo { get; } = P(
        "M12.5 6H6A3.5 3.5 0 0 0 6 13H8.5M12.5 6 9.5 3M12.5 6 9.5 9");
    /// <summary>Обновить, перечитать.</summary>
    public static Geometry Refresh { get; } = P(
        "M13.5 8A5.5 5.5 0 1 1 8 2.5C9.5 2.5 11 3.1 12.1 4.2L13.5 5.6M13.5 2.5V5.6H10.4");

    // Поиск, списки, таблицы
    /// <summary>Лупа с плюсом: приблизить.</summary>
    public static Geometry ZoomIn { get; } = P(
        "M10.9 7.2a3.7 3.7 0 1 1-7.4 0 3.7 3.7 0 0 1 7.4 0M10.2 10.2 13 13M7.2 5.4V9M5.4 7.2H9");
    /// <summary>Лупа с минусом: отдалить.</summary>
    public static Geometry ZoomOut { get; } = P(
        "M10.9 7.2a3.7 3.7 0 1 1-7.4 0 3.7 3.7 0 0 1 7.4 0M10.2 10.2 13 13M5.4 7.2H9");
    /// <summary>Воронка: фильтр списка.</summary>
    public static Geometry Filter { get; } = P(
        "M3 4.2H13L9.2 8.8V12.3L6.8 13.4V8.8Z");
    /// <summary>Стрелки вверх-вниз: сортировка.</summary>
    public static Geometry Sort { get; } = P(
        "M4.5 3.5V12.5M2.6 10.6 4.5 12.5 6.4 10.6M11.5 12.5V3.5M9.6 5.4 11.5 3.5 13.4 5.4");
    /// <summary>Три точки вертикально: меню строки. IsFilled.</summary>
    public static Geometry MoreVertical { get; } = P(
        "M6.85 3.8A1.15 1.15 0 1 1 9.15 3.8A1.15 1.15 0 1 1 6.85 3.8ZM6.85 8A1.15 1.15 0 1 1 9.15 8A1.15 1.15 0 1 1 6.85 8ZM6.85 12.2A1.15 1.15 0 1 1 9.15 12.2A1.15 1.15 0 1 1 6.85 12.2Z");  // IsFilled="True"
    /// <summary>Три точки горизонтально: свёрнутое меню. IsFilled.</summary>
    public static Geometry MoreHorizontal { get; } = P(
        "M2.65 8A1.15 1.15 0 1 1 4.95 8A1.15 1.15 0 1 1 2.65 8ZM6.85 8A1.15 1.15 0 1 1 9.15 8A1.15 1.15 0 1 1 6.85 8ZM11.05 8A1.15 1.15 0 1 1 13.35 8A1.15 1.15 0 1 1 11.05 8Z");  // IsFilled="True"

    // Проект и файлы
    /// <summary>Раскрытая папка.</summary>
    public static Geometry FolderOpen { get; } = P(
        "M2.5 12.5V4.8H6.5L7.9 6.2H13.5V7.9M2.5 12.5 4.2 7.9H13.9L12.2 12.5Z");
    /// <summary>Лист с угловыми скобками: файл разметки или кода.</summary>
    public static Geometry DocumentCode { get; } = P(
        "M4 2.5h5.5L12.5 6v7.5H4zM9.5 2.5V6h3M6.4 8.8 5.2 10.1 6.4 11.4M9.6 8.8 10.8 10.1 9.6 11.4");
    /// <summary>Структура: иерархия элементов документа.</summary>
    public static Geometry Structure { get; } = P(
        "M2.5 3.5H13.5M5.5 8H13.5M8.5 12.5H13.5");
    /// <summary>Вилка-разъём: плагин.</summary>
    public static Geometry Plugin { get; } = P(
        "M6 2.5V5.5M10 2.5V5.5M4.5 5.5H11.5V8.5A3 3 0 0 1 8.5 11.5H7.5A3 3 0 0 1 4.5 8.5ZM8 11.5V13.5");
    /// <summary>Книга: документация.</summary>
    public static Geometry Docs { get; } = P(
        "M8 4.5V13M8 4.5C6.9 3.5 5.4 3 3 3V11.5C5.4 11.5 6.9 12 8 13M8 4.5C9.1 3.5 10.6 3 13 3V11.5C10.6 11.5 9.1 12 8 13");

    // Запуск и отладка
    /// <summary>Квадрат: остановить. IsFilled.</summary>
    public static Geometry Stop { get; } = P(
        "M4.5 4.5H11.5V11.5H4.5Z");  // IsFilled="True"
    /// <summary>Пауза. IsFilled.</summary>
    public static Geometry Pause { get; } = P(
        "M4.6 4H6.8V12H4.6ZM9.2 4H11.4V12H9.2Z");  // IsFilled="True"
    /// <summary>Жук: отладка.</summary>
    public static Geometry Debug { get; } = P(
        "M5.6 8.4A2.4 2.4 0 0 1 10.4 8.4V9.8A2.4 2.4 0 0 1 5.6 9.8ZM6.4 4.4 7.2 6.2M9.6 4.4 8.8 6.2M3.6 8.6H5.6M10.4 8.6H12.4M4.2 11.4 5.9 10.5M11.8 11.4 10.1 10.5");
    /// <summary>Молния по кругу: горячая перезагрузка.</summary>
    public static Geometry HotReload { get; } = P(
        "M9.5 2.5 5 8.8H7.8L6.5 13.5 11 7.2H8.2Z");
    /// <summary>Приглашение консоли.</summary>
    public static Geometry Terminal { get; } = P(
        "M2.5 3.5H13.5V12.5H2.5ZM5.2 7.2 7.2 9.2 5.2 11.2M8.4 11.2H11.2");

    // Статусы
    /// <summary>Треугольник с восклицательным знаком: предупреждение в баннере и списке проблем.</summary>
    public static Geometry WarningTriangle { get; } = P(
        "M8 3 13.9 12.9H2.1ZM8 6.6V9.4M8 11.1v.2");
    /// <summary>Круг с «i»: сведения.</summary>
    public static Geometry Info { get; } = P(
        "M13.5 8a5.5 5.5 0 1 1-11 0 5.5 5.5 0 0 1 11 0M8 5.2v.2M8 7.5V11");
    /// <summary>Круг с галочкой: успех.</summary>
    public static Geometry Success { get; } = P(
        "M13.5 8a5.5 5.5 0 1 1-11 0 5.5 5.5 0 0 1 11 0M5.2 8.2 7 10 10.8 6");
    /// <summary>Круг с крестом: ошибка.</summary>
    public static Geometry Error { get; } = P(
        "M13.5 8a5.5 5.5 0 1 1-11 0 5.5 5.5 0 0 1 11 0M5.9 5.9 10.1 10.1M10.1 5.9 5.9 10.1");
    /// <summary>Круг с вопросом: справка.</summary>
    public static Geometry Question { get; } = P(
        "M13.5 8a5.5 5.5 0 1 1-11 0 5.5 5.5 0 0 1 11 0M6.4 6.4A1.7 1.7 0 0 1 9.7 7C9.7 8.2 8 8.4 8 9.7M8 11.4v.2");

    // Дизайнер форм
    /// <summary>Линии сетки холста с привязкой.</summary>
    public static Geometry GridSnap { get; } = P(
        "M2.5 6H13.5M2.5 10H13.5M6 2.5V13.5M10 2.5V13.5");
    /// <summary>Слои.</summary>
    public static Geometry Layers { get; } = P(
        "M8 2.6 13.9 6 8 9.4 2.1 6ZM3.4 8.4 2.1 9.2 8 12.6 13.9 9.2 12.6 8.4");
    /// <summary>Ромб-компонент.</summary>
    public static Geometry Component { get; } = P(
        "M3.5 3.5H12.5V12.5H3.5ZM6.5 6.5H9.5V9.5H6.5Z");
    /// <summary>Разделённый вид: дизайн и разметка рядом.</summary>
    public static Geometry Split { get; } = P(
        "M2.5 3.5H13.5V12.5H2.5ZM8 3.5V12.5");
    /// <summary>Глаз: предпросмотр.</summary>
    public static Geometry Preview { get; } = P(
        "M2.5 8C4.4 5.3 6.1 4.6 8 4.6C9.9 4.6 11.6 5.3 13.5 8C11.6 10.7 9.9 11.4 8 11.4C6.1 11.4 4.4 10.7 2.5 8ZM6.2 8A1.8 1.8 0 1 1 9.8 8A1.8 1.8 0 1 1 6.2 8Z");
    /// <summary>Перечёркнутый глаз: предпросмотр выключен.</summary>
    public static Geometry PreviewOff { get; } = P(
        "M2.5 8C4.4 5.3 6.1 4.6 8 4.6C9.9 4.6 11.6 5.3 13.5 8C11.6 10.7 9.9 11.4 8 11.4C6.1 11.4 4.4 10.7 2.5 8ZM3.2 12.8 12.8 3.2");
    /// <summary>Закрытый замок: слой заблокирован.</summary>
    public static Geometry Lock { get; } = P(
        "M4.5 7.5H11.5V12.8H4.5ZM6.2 7.5V5.9A1.8 1.8 0 0 1 9.8 5.9V7.5M8 9.5V11");
    /// <summary>Открытый замок.</summary>
    public static Geometry Unlock { get; } = P(
        "M4.5 7.5H11.5V12.8H4.5ZM6.2 7.5V5.9A1.8 1.8 0 0 1 9.8 5.4M8 9.5V11");
    /// <summary>Стрелки во все стороны: перемещение.</summary>
    public static Geometry Move { get; } = P(
        "M8 2.5V13.5M2.5 8H13.5M6.1 4.4 8 2.5 9.9 4.4M6.1 11.6 8 13.5 9.9 11.6M4.4 6.1 2.5 8 4.4 9.9M11.6 6.1 13.5 8 11.6 9.9");
    /// <summary>Курсор-указатель: инструмент выбора.</summary>
    public static Geometry Pointer { get; } = P(
        "M4.2 2.8 11.6 8.4 8.1 9 10.2 12.8 8.7 13.6 6.6 9.8 4.2 12.4Z");
    /// <summary>Вписать в экран.</summary>
    public static Geometry FitToScreen { get; } = P(
        "M2.5 6V2.5H6M10 2.5H13.5V6M13.5 10V13.5H10M6 13.5H2.5V10");
    /// <summary>Выровнять по левому краю.</summary>
    public static Geometry AlignLeft { get; } = P(
        "M2.5 2.5V13.5M4.5 5.5H11.5M4.5 8H13M4.5 10.5H9");
    /// <summary>Литера T: текст.</summary>
    public static Geometry Text { get; } = P(
        "M3.5 5V3.5H12.5V5M8 3.5V12.5M6 12.5H10");
    /// <summary>Звенья: привязка данных.</summary>
    public static Geometry Binding { get; } = P(
        "M9 5.9 10.1 4.8A2.5 2.5 0 0 1 13.5 8.2L12.4 9.3M7 10.1 5.9 11.2A2.5 2.5 0 0 1 2.5 7.8L3.6 6.7M6.2 9.8 9.8 6.2");

    // Окно, тема, прочее
    /// <summary>Свернуть окно.</summary>
    public static Geometry WindowMinimize { get; } = P(
        "M3.5 8H12.5");
    /// <summary>Развернуть окно.</summary>
    public static Geometry WindowMaximize { get; } = P(
        "M3.5 3.5H12.5V12.5H3.5Z");
    /// <summary>Восстановить окно из развёрнутого.</summary>
    public static Geometry WindowRestore { get; } = P(
        "M5.5 6.5H11.5V12.5H5.5ZM7.5 6.5V4.5H13.5V10.5H11.5");
    /// <summary>Шестерёнка: настройки в меню приложения (ползунки остаются за Settings).</summary>
    public static Geometry Gear { get; } = P(
        "M2.8 8A5.2 5.2 0 1 1 13.2 8A5.2 5.2 0 1 1 2.8 8ZM6 8A2 2 0 1 1 10 8A2 2 0 1 1 6 8ZM12.8 9.99L13.91 10.45M9.99 12.8L10.45 13.91M6.01 12.8L5.55 13.91M3.2 9.99L2.09 10.45M3.2 6.01L2.09 5.55M6.01 3.2L5.55 2.09M9.99 3.2L10.45 2.09M12.8 6.01L13.91 5.55");
    /// <summary>Стрелка из рамки: внешняя ссылка.</summary>
    public static Geometry ExternalLink { get; } = P(
        "M9.5 3H13V6.5M13 3 8.2 7.8M11.5 9.5V12A1.5 1.5 0 0 1 10 13.5H4A1.5 1.5 0 0 1 2.5 12V6A1.5 1.5 0 0 1 4 4.5H6.5");
    /// <summary>Кнопка-булавка: закрепить.</summary>
    public static Geometry Pin { get; } = P(
        "M5.5 2.5H10.5M6.8 2.5V7.5L5 9.5H11L9.2 7.5V2.5M8 9.5V13.5");
    /// <summary>Клавиатура: шорткаты.</summary>
    public static Geometry Keyboard { get; } = P(
        "M2.5 4.5H13.5V11.5H2.5ZM4.6 7h.1M6.9 7H7M9.2 7h.1M11.5 7h.1M5.2 9.4H10.8");
    /// <summary>Пользователь.</summary>
    public static Geometry User { get; } = P(
        "M5.5 5.9A2.5 2.5 0 1 1 10.5 5.9A2.5 2.5 0 1 1 5.5 5.9ZM3.2 13.5C3.2 10.9 5.3 9.6 8 9.6S12.8 10.9 12.8 13.5");
    /// <summary>Звезда: избранное.</summary>
    public static Geometry Star { get; } = P(
        "M8 2.8L9.35 6.54L13.33 6.67L10.19 9.11L11.29 12.93L8 10.7L4.71 12.93L5.81 9.11L2.67 6.67L6.65 6.54Z");
    /// <summary>Солнце: светлая тема.</summary>
    public static Geometry ThemeLight { get; } = P(
        "M5.6 8A2.4 2.4 0 1 1 10.4 8A2.4 2.4 0 1 1 5.6 8ZM12.1 8L13.9 8M10.9 10.9L12.17 12.17M8 12.1L8 13.9M5.1 10.9L3.83 12.17M3.9 8L2.1 8M5.1 5.1L3.83 3.83M8 3.9L8 2.1M10.9 5.1L12.17 3.83");
    /// <summary>Месяц: тёмная тема.</summary>
    public static Geometry ThemeDark { get; } = P(
        "M8 2A4 4 0 0 0 14 8A6 6 0 1 1 8 2Z");

    /// <summary>
    /// Семейство 2 — глифы контролов для палитры дизайнера форм.
    /// Имя глифа = имя контрола Avalonia: <c>Toolbox.Button</c> ставится и для
    /// Button, и для AxButton. List, Tabs, DataGrid, Grid, Image, Window и
    /// Bubble живут в основном наборе и здесь не повторяются.
    /// </summary>
    public static class Toolbox
    {
        // Кнопки
        /// <summary>Глиф контрола Button для палитры дизайнера.</summary>
        public static Geometry Button { get; } = P(
            "M2.5 5.5H13.5V10.5H2.5ZM5.5 8H10.5");
        /// <summary>Глиф контрола RepeatButton для палитры дизайнера.</summary>
        public static Geometry RepeatButton { get; } = P(
            "M2.5 5.5H13.5V10.5H2.5ZM5 8H8.2M10 6.9 11.1 8 10 9.1M11.7 6.9 12.8 8 11.7 9.1");
        /// <summary>Глиф контрола ToggleButton для палитры дизайнера.</summary>
        public static Geometry ToggleButton { get; } = P(
            "M2.5 5.5H13.5V10.5H2.5ZM8 5.5V10.5");
        /// <summary>Глиф контрола SplitButton для палитры дизайнера.</summary>
        public static Geometry SplitButton { get; } = P(
            "M2.5 5.5H13.5V10.5H2.5ZM10 5.5V10.5M4.5 8H8M11.15 7.4 11.75 8 12.35 7.4");
        /// <summary>Глиф контрола DropDownButton для палитры дизайнера.</summary>
        public static Geometry DropDownButton { get; } = P(
            "M2.5 5.5H13.5V10.5H2.5ZM4.5 8H8.5M10.4 7.4 11.4 8.4 12.4 7.4");
        /// <summary>Глиф контрола HyperlinkButton для палитры дизайнера.</summary>
        public static Geometry HyperlinkButton { get; } = P(
            "M3.5 6.6H12.5M3.5 8.8H9.5M3.5 10.4H9.5");
        /// <summary>Глиф контрола ButtonSpinner для палитры дизайнера.</summary>
        public static Geometry ButtonSpinner { get; } = P(
            "M2.5 5.5H13.5V10.5H2.5ZM10 5.5V10.5M4.5 8H8M11 7.4 11.75 6.7 12.5 7.4M11 8.9 11.75 9.6 12.5 8.9");
        /// <summary>Глиф контрола NumericUpDown для палитры дизайнера.</summary>
        public static Geometry NumericUpDown { get; } = P(
            "M2.5 5.5H13.5V10.5H2.5ZM10 5.5V10.5M4.5 8H6.8M11 7.4 11.75 6.7 12.5 7.4M11 8.9 11.75 9.6 12.5 8.9");

        // Ввод текста
        /// <summary>Глиф контрола TextBox для палитры дизайнера.</summary>
        public static Geometry TextBox { get; } = P(
            "M2.5 5.5H13.5V10.5H2.5ZM4.5 8H8M10 6.8V9.2");
        /// <summary>Глиф контрола MaskedTextBox для палитры дизайнера.</summary>
        public static Geometry MaskedTextBox { get; } = P(
            "M2.5 5.5H13.5V10.5H2.5ZM4.6 8h.1M6.2 8h.1M7.8 8h.1M9.4 8h.1M11 8H12.5");
        /// <summary>Глиф контрола AutoCompleteBox для палитры дизайнера.</summary>
        public static Geometry AutoCompleteBox { get; } = P(
            "M2.5 4.5H13.5V7.3H2.5ZM4.5 5.3V6.5M2.5 10.1H13.5V12.9H2.5Z");
        /// <summary>Глиф контрола TextBlock для палитры дизайнера.</summary>
        public static Geometry TextBlock { get; } = P(
            "M3 5H13M3 8H13M3 11H9");
        /// <summary>Глиф контрола SelectableTextBlock для палитры дизайнера.</summary>
        public static Geometry SelectableTextBlock { get; } = P(
            "M3 5H13M3 11H9M2.6 6.9H10.4V9.1H2.6Z");
        /// <summary>Глиф контрола Label для палитры дизайнера.</summary>
        public static Geometry Label { get; } = P(
            "M3.5 8H9.5M11.5 8h.1");
        /// <summary>Глиф контрола TextArea для палитры дизайнера.</summary>
        public static Geometry TextArea { get; } = P(
            "M2.5 3.5H13.5V12.5H2.5ZM4.5 6H11.5M4.5 8H11.5M4.5 10H8.5");
        /// <summary>Глиф контрола SearchField для палитры дизайнера.</summary>
        public static Geometry SearchField { get; } = P(
            "M2.5 5.5H13.5V10.5H2.5ZM4 8A1.4 1.4 0 1 1 6.8 8A1.4 1.4 0 1 1 4 8ZM6.4 9 7.5 10.1M9 8H11.5");

        // Выбор
        /// <summary>Глиф контрола CheckBox для палитры дизайнера.</summary>
        public static Geometry CheckBox { get; } = P(
            "M2.5 5.5H7.5V10.5H2.5ZM3.7 8 4.9 9.2 6.4 6.9M9.5 8H13.5");
        /// <summary>Глиф контрола RadioButton для палитры дизайнера.</summary>
        public static Geometry RadioButton { get; } = P(
            "M2.5 8A2.5 2.5 0 1 1 7.5 8A2.5 2.5 0 1 1 2.5 8ZM4 8A1 1 0 1 1 6 8A1 1 0 1 1 4 8ZM9.5 8H13.5");
        /// <summary>Глиф контрола ToggleSwitch для палитры дизайнера.</summary>
        public static Geometry ToggleSwitch { get; } = P(
            "M4.6 5.9H9.4A2.1 2.1 0 0 1 9.4 10.1H4.6A2.1 2.1 0 0 1 4.6 5.9ZM8.2 8A1.2 1.2 0 1 1 10.6 8A1.2 1.2 0 1 1 8.2 8Z");
        /// <summary>Глиф контрола ComboBox для палитры дизайнера.</summary>
        public static Geometry ComboBox { get; } = P(
            "M2.5 5.5H13.5V10.5H2.5ZM4.5 8H8M10.2 7.3 11.3 8.4 12.4 7.3");
        /// <summary>Глиф контрола ComboBoxItem для палитры дизайнера.</summary>
        public static Geometry ComboBoxItem { get; } = P(
            "M2.5 6.5H13.5V9.5H2.5ZM4.2 8 5 8.8 6.4 7.2M8 8H11.5");
        /// <summary>Глиф контрола Slider для палитры дизайнера.</summary>
        public static Geometry Slider { get; } = P(
            "M2.5 8H13.5M7.5 8A2 2 0 1 1 11.5 8A2 2 0 1 1 7.5 8Z");
        /// <summary>Глиф контрола SegmentedControl для палитры дизайнера.</summary>
        public static Geometry SegmentedControl { get; } = P(
            "M2.5 5.5H13.5V10.5H2.5ZM6.2 5.5V10.5M9.8 5.5V10.5");

        // Дата и время
        /// <summary>Глиф контрола Calendar для палитры дизайнера.</summary>
        public static Geometry Calendar { get; } = P(
            "M2.5 4H13.5V13H2.5ZM2.5 7H13.5M5.5 4V2.6M10.5 4V2.6M5.5 9.6h.1M8 9.6h.1M10.5 9.6h.1M5.5 11.4h.1M8 11.4h.1");
        /// <summary>Глиф контрола CalendarDatePicker для палитры дизайнера.</summary>
        public static Geometry CalendarDatePicker { get; } = P(
            "M2.5 5.5H9.5V10.5H2.5ZM4.5 8H7M11 5.5H13.5V8.5H11ZM11.6 5.5V4.7M12.9 5.5V4.7");
        /// <summary>Глиф контрола DatePicker для палитры дизайнера.</summary>
        public static Geometry DatePicker { get; } = P(
            "M2.5 5.5H13.5V10.5H2.5ZM6.2 5.5V10.5M9.8 5.5V10.5M4 8h.1M7.6 8h.1M11.4 8h.1");
        /// <summary>Глиф контрола TimePicker для палитры дизайнера.</summary>
        public static Geometry TimePicker { get; } = P(
            "M2.8 8A5.2 5.2 0 1 1 13.2 8A5.2 5.2 0 1 1 2.8 8ZM8 8V5.2M8 8H10.4");

        // Списки и данные
        /// <summary>Глиф контрола ItemsControl для палитры дизайнера.</summary>
        public static Geometry ItemsControl { get; } = P(
            "M4.5 4.5H13.5M4.5 8H13.5M4.5 11.5H13.5M2.6 4.5h.1M2.6 8h.1M2.6 11.5h.1");
        /// <summary>Глиф контрола ListBoxItem для палитры дизайнера.</summary>
        public static Geometry ListBoxItem { get; } = P(
            "M2.5 6.5H13.5V9.5H2.5ZM4.5 8H9.5");
        /// <summary>Глиф контрола TreeView для палитры дизайнера.</summary>
        public static Geometry TreeView { get; } = P(
            "M2.5 3.5H13.5V12.5H2.5ZM4.5 6.3H11.5M6.8 9.7H11.5M4.5 6.3V9.7");
        /// <summary>Глиф контрола TreeViewItem для палитры дизайнера.</summary>
        public static Geometry TreeViewItem { get; } = P(
            "M2.8 6.9 4 8 2.8 9.1M5.5 8H13.5");
        /// <summary>Глиф контрола Carousel для палитры дизайнера.</summary>
        public static Geometry Carousel { get; } = P(
            "M4.5 4.5H11.5V11.5H4.5ZM3.4 6.8 2.2 8 3.4 9.2M12.6 6.8 13.8 8 12.6 9.2");
        /// <summary>Глиф контрола PipsPager для палитры дизайнера.</summary>
        public static Geometry PipsPager { get; } = P(
            "M2.4 8A1.1 1.1 0 1 1 4.6 8A1.1 1.1 0 1 1 2.4 8ZM6.9 8h.1M9.7 8h.1M12.5 8h.1");

        // Меню и всплывающие
        /// <summary>Глиф контрола Menu для палитры дизайнера.</summary>
        public static Geometry Menu { get; } = P(
            "M2.5 4.5H13.5V7.5H2.5ZM5.5 4.5V7.5M8.5 4.5V7.5M2.5 9.5H8.5V13.5H2.5Z");
        /// <summary>Глиф контрола MenuItem для палитры дизайнера.</summary>
        public static Geometry MenuItem { get; } = P(
            "M2.5 6.5H13.5V9.5H2.5ZM4.5 8H9M11 7.1 11.9 8 11 8.9");
        /// <summary>Глиф контрола ContextMenu для палитры дизайнера.</summary>
        public static Geometry ContextMenu { get; } = P(
            "M3.5 2.5H12.5V13.5H3.5ZM5.5 5H10.5M3.5 6.8H12.5M5.5 9H10.5M5.5 11.5H10.5");
        /// <summary>Глиф контрола ToolTip для палитры дизайнера.</summary>
        public static Geometry ToolTip { get; } = P(
            "M3.5 5.5H12.5V9.5H3.5ZM6.5 9.5 7.5 11.3 8.5 9.5");
        /// <summary>Глиф контрола Popup для палитры дизайнера.</summary>
        public static Geometry Popup { get; } = P(
            "M2.5 2.5H10.5V10.5H2.5ZM5.5 5.5H13.5V13.5H5.5Z");
        /// <summary>Глиф контрола TeachingTip для палитры дизайнера.</summary>
        public static Geometry TeachingTip { get; } = P(
            "M3 3.5H13V10H8.5L5.5 12.8V10H3ZM8 5.5V7.4M8 8.6v.2");
        /// <summary>Глиф контрола QuickSearch для палитры дизайнера.</summary>
        public static Geometry QuickSearch { get; } = P(
            "M2.5 3.5H13.5V12.5H2.5ZM2.5 6.9H13.5M4.5 4.6V5.8M4.5 9.7H11.5");

        // Вкладки
        /// <summary>Глиф контрола TabItem для палитры дизайнера.</summary>
        public static Geometry TabItem { get; } = P(
            "M4 7.5V4.5H10V7.5M2.5 7.5H13.5");
        /// <summary>Глиф контрола TabStrip для палитры дизайнера.</summary>
        public static Geometry TabStrip { get; } = P(
            "M2.5 6H13.5V10H2.5ZM6.2 6V10M9.8 6V10M2.5 12.8H6.2");
        /// <summary>Глиф контрола TabStripItem для палитры дизайнера.</summary>
        public static Geometry TabStripItem { get; } = P(
            "M4 6H12V10H4ZM4 12.8H12");
        /// <summary>Глиф контрола BreadcrumbBar для палитры дизайнера.</summary>
        public static Geometry BreadcrumbBar { get; } = P(
            "M2.5 8H4.8M6 6.8 7.2 8 6 9.2M8.4 8H10.7M11.9 6.8 13.1 8 11.9 9.2");

        // Контейнеры и компоновка
        /// <summary>Глиф контрола Panel для палитры дизайнера.</summary>
        public static Geometry Panel { get; } = P(
            "M2.5 3.5H13.5V12.5H2.5Z");
        /// <summary>Глиф контрола Border для палитры дизайнера.</summary>
        public static Geometry Border { get; } = P(
            "M2.5 2.5H13.5V13.5H2.5ZM5 5H11V11H5Z");
        /// <summary>Глиф контрола ContentControl для палитры дизайнера.</summary>
        public static Geometry ContentControl { get; } = P(
            "M2.5 3.5H13.5V12.5H2.5ZM5.8 8A2.2 2.2 0 1 1 10.2 8A2.2 2.2 0 1 1 5.8 8Z");
        /// <summary>Глиф контрола StackPanel для палитры дизайнера.</summary>
        public static Geometry StackPanel { get; } = P(
            "M2.5 3.5H13.5V6.3H2.5ZM2.5 9.1H13.5V11.9H2.5Z");
        /// <summary>Глиф контрола UniformGrid для палитры дизайнера.</summary>
        public static Geometry UniformGrid { get; } = P(
            "M2.5 3.5H13.5V12.5H2.5ZM6.17 3.5V12.5M9.83 3.5V12.5M2.5 8H13.5");
        /// <summary>Глиф контрола DockPanel для палитры дизайнера.</summary>
        public static Geometry DockPanel { get; } = P(
            "M2.5 3.5H13.5V12.5H2.5ZM2.5 6H13.5M5 6V12.5M11 6V12.5");
        /// <summary>Глиф контрола WrapPanel для палитры дизайнера.</summary>
        public static Geometry WrapPanel { get; } = P(
            "M3.8 3.8H6.6V6.6H3.8ZM9.4 3.8H12.2V6.6H9.4ZM3.8 9.4H6.6V12.2H3.8ZM9.4 9.4H12.2V12.2H9.4Z");
        /// <summary>Глиф контрола Canvas для палитры дизайнера.</summary>
        public static Geometry Canvas { get; } = P(
            "M2.5 3.5H13.5V12.5H2.5ZM6 6.5H10V9.5H6Z");
        /// <summary>Глиф контрола RelativePanel для палитры дизайнера.</summary>
        public static Geometry RelativePanel { get; } = P(
            "M2.5 3.5H13.5V12.5H2.5ZM8.5 7.5H12.5V10.5H8.5ZM8.5 9H2.5M10.5 7.5V3.5");
        /// <summary>Глиф контрола Expander для палитры дизайнера.</summary>
        public static Geometry Expander { get; } = P(
            "M2.5 3.5H13.5V6.3H2.5ZM4.4 4.6 5.15 5.35 5.9 4.6M2.5 9.1H13.5V12.5H2.5Z");
        /// <summary>Глиф контрола GroupBox для палитры дизайнера.</summary>
        public static Geometry GroupBox { get; } = P(
            "M2.5 5.5H13.5V12.5H2.5ZM4.5 5.5V3.5H9.5V5.5");
        /// <summary>Глиф контрола SplitView для палитры дизайнера.</summary>
        public static Geometry SplitView { get; } = P(
            "M2.5 3.5H13.5V12.5H2.5ZM6.5 3.5V12.5");
        /// <summary>Глиф контрола Viewbox для палитры дизайнера.</summary>
        public static Geometry Viewbox { get; } = P(
            "M2.5 3.5H13.5V12.5H2.5ZM5.5 6.5 10.5 9.5M5.5 6.5H7.9M5.5 6.5V8.9M10.5 9.5H8.1M10.5 9.5V7.1");

        // Прокрутка и разделители
        /// <summary>Глиф контрола ScrollViewer для палитры дизайнера.</summary>
        public static Geometry ScrollViewer { get; } = P(
            "M2.5 3.5H11.5V12.5H2.5ZM13 4.5V8.5");
        /// <summary>Глиф контрола ScrollBar для палитры дизайнера.</summary>
        public static Geometry ScrollBar { get; } = P(
            "M6.4 2.5H9.6V13.5H6.4ZM6.4 5H9.6M6.4 10.5H9.6");
        /// <summary>Глиф контрола GridSplitter для палитры дизайнера.</summary>
        public static Geometry GridSplitter { get; } = P(
            "M2.5 3.5H6.8V12.5H2.5ZM9.2 3.5H13.5V12.5H9.2ZM8 6.5V9.5");
        /// <summary>Глиф контрола Separator для палитры дизайнера.</summary>
        public static Geometry Separator { get; } = P(
            "M2.5 8H6M7.2 8H8.8M10 8H13.5");

        // Прогресс, медиа, оверлеи
        /// <summary>Глиф контрола ProgressBar для палитры дизайнера.</summary>
        public static Geometry ProgressBar { get; } = P(
            "M2.5 6.8H13.5V9.2H2.5ZM8 6.8V9.2");
        /// <summary>Глиф контрола Spinner для палитры дизайнера.</summary>
        public static Geometry Spinner { get; } = P(
            "M8 2.8A5.2 5.2 0 1 1 2.8 8");
        /// <summary>Глиф контрола RefreshContainer для палитры дизайнера.</summary>
        public static Geometry RefreshContainer { get; } = P(
            "M2.5 5.5H13.5V12.5H2.5ZM8 2V4.6M6.8 3.4 8 4.6 9.2 3.4");
        /// <summary>Глиф контрола PathIcon для палитры дизайнера.</summary>
        public static Geometry PathIcon { get; } = P(
            "M3 11.5C5 5 8 11.5 13 4.5M2.4 11.5h.1M13 4.5h.1");
        /// <summary>Глиф контрола NotificationCard для палитры дизайнера.</summary>
        public static Geometry NotificationCard { get; } = P(
            "M2.5 4.5H13.5V11.5H2.5ZM5 4.5V11.5M7 7H11.5M7 9H10");
        /// <summary>Глиф контрола WindowNotificationManager для палитры дизайнера.</summary>
        public static Geometry WindowNotificationManager { get; } = P(
            "M2.5 3.5H13.5V12.5H2.5ZM8.5 6.5H12.5V9.5H8.5Z");

        // Контролы студии
        /// <summary>Глиф контрола ToolWindow для палитры дизайнера.</summary>
        public static Geometry ToolWindow { get; } = P(
            "M2.5 3.5H13.5V12.5H2.5ZM5.5 3.5V12.5M5.5 6H13.5");
        /// <summary>Глиф контрола TitleBar для палитры дизайнера.</summary>
        public static Geometry TitleBar { get; } = P(
            "M2.5 4.5H13.5V11.5H2.5ZM2.5 7H13.5M9.5 5.7h.1M11 5.7h.1M12.5 5.7h.1");
        /// <summary>Глиф контрола WindowControls для палитры дизайнера.</summary>
        public static Geometry WindowControls { get; } = P(
            "M3 8H5M7 6.5H9.5V9.5H7ZM11.5 6.5 13.5 9.5M13.5 6.5 11.5 9.5");
        /// <summary>Глиф контрола ToolBar для палитры дизайнера.</summary>
        public static Geometry ToolBar { get; } = P(
            "M2.5 4.8H13.5M3.8 7.6H6.6V10.4H3.8ZM9.4 7.6H12.2V10.4H9.4Z");
        /// <summary>Глиф контрола Banner для палитры дизайнера.</summary>
        public static Geometry Banner { get; } = P(
            "M2.5 5.5H13.5V10.5H2.5ZM3.7 8A1.3 1.3 0 1 1 6.3 8A1.3 1.3 0 1 1 3.7 8ZM7.5 8H11.5");
        /// <summary>Глиф контрола Card для палитры дизайнера.</summary>
        public static Geometry Card { get; } = P(
            "M2.5 3.5H13.5V12.5H2.5ZM2.5 8H13.5M4.5 10.2H11.5");
        /// <summary>Глиф контрола Chip для палитры дизайнера.</summary>
        public static Geometry Chip { get; } = P(
            "M5.4 5.9H10.6A2.1 2.1 0 0 1 10.6 10.1H5.4A2.1 2.1 0 0 1 5.4 5.9ZM7 8H9");
        /// <summary>Глиф контрола Badge для палитры дизайнера.</summary>
        public static Geometry Badge { get; } = P(
            "M2.5 7.2H8.2V13.2H2.5ZM9.9 4.8A2 2 0 1 1 13.9 4.8A2 2 0 1 1 9.9 4.8Z");
        /// <summary>Глиф контрола Avatar для палитры дизайнера.</summary>
        public static Geometry Avatar { get; } = P(
            "M2.5 3.5H13.5V12.5H2.5ZM6.2 7A1.8 1.8 0 1 1 9.8 7A1.8 1.8 0 1 1 6.2 7ZM4.8 12.5C4.8 10.6 6.2 9.8 8 9.8S11.2 10.6 11.2 12.5");
        /// <summary>Глиф контрола GroupHeader для палитры дизайнера.</summary>
        public static Geometry GroupHeader { get; } = P(
            "M2.5 6.8H6.2V9.2H2.5ZM7.6 8H13.5");
        /// <summary>Глиф контрола CodeBlock для палитры дизайнера.</summary>
        public static Geometry CodeBlock { get; } = P(
            "M2.5 3.5H13.5V12.5H2.5ZM6.2 6.8 4.6 8.4 6.2 10M9.8 6.8 11.4 8.4 9.8 10");
        /// <summary>Глиф контрола Dialog для палитры дизайнера.</summary>
        public static Geometry Dialog { get; } = P(
            "M2.5 3.5H13.5V12.5H2.5ZM2.5 6H13.5M8 9.6H10M11 9.6H12.8");
    }
}
