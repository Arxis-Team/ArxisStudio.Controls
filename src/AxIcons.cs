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
}
