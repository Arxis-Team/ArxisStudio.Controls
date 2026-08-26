using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
#if DEBUG
using AvaDevTools;
#endif

namespace Controls.Gallery;

public class App : Application
{
    /// <summary>Порт конечной точки MCP галереи — у студии свой, 5171.</summary>
    private const int McpPort = 5172;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow();

        AttachTools();

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Поднимает инструменты отладки интерфейса поверх галереи.
    /// </summary>
    /// <remarks>
    /// Галерея — то место, где контрол смотрят вблизи, и смотреть его глазами
    /// по снимку экрана значит мерить линейкой по фотографии. Инструменты живут
    /// в том же процессе и отвечают о тех же живых объектах: дерево, стили,
    /// действующие значения свойств, снимок отдельного элемента.
    ///
    /// Конечная точка поднимается сразу, а не по щелчку в окне инструментов:
    /// подключаются к уже запущенной галерее, и ждать, пока кто-то откроет
    /// окно, означало бы не подключиться вовсе. Разрешения на удержание
    /// всплывающих состояний и на ввод — то, чем наведение и нажатие
    /// проверяются так же, как их проверяет человек.
    /// </remarks>
    private static void AttachTools()
    {
#if DEBUG
        Current!.AttachAvaDevTools(new DevToolsOptions
        {
            McpServer = true,
            McpPort = McpPort,
            McpAllowHold = true,
            McpAllowInput = true,
        });
#endif
    }
}
