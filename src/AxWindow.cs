using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace ArxisStudio.Controls;

/// <summary>
/// Окно студии: своя полоса заголовка при системной рамке, покрашенной в цвет
/// темы.
/// </summary>
/// <remarks>
/// Заголовок окна рисует само приложение (<see cref="AxTitleBar"/>), а рамку
/// оставляет системе — она даёт тень, привязку к краям экрана и изменение
/// размера. Клиентская область при этом заходит в рамку: иначе её верхние
/// восемь пикселей остаются пустой полосой над заголовком. Windows красит эту рамку своим серым, и вокруг тёмной студии
/// появляется светлая кайма шириной в несколько пикселей; просить у системы
/// нужный цвет — единственный способ её убрать, не отказываясь от самой рамки.
/// <para>
/// Цвет берётся из ресурса <c>AxBg2Color</c> по текущему варианту темы: рамка
/// примыкает к полосе заголовка, и разница цветов заметнее всего именно там.
/// Перекрашивается окно само — и при открытии, и при смене темы, — поэтому
/// обходить открытые окна снаружи никому не нужно.
/// </para>
/// <para>
/// Настройка появилась в Windows 11; на более ранних версиях вызов ничего не
/// делает, как и на других платформах.
/// </para>
/// </remarks>
public class AxWindow : Window
{
    private const int UseImmersiveDarkMode = 20;
    private const int BorderColor = 34;
    private const int CaptionColor = 35;

    /// <summary>Заводит окно со своей полосой заголовка и системной рамкой.</summary>
    public AxWindow()
    {
        WindowDecorations = WindowDecorations.BorderOnly;

        // Клиентская область заходит в рамку. Без этого Windows оставляет
        // сверху восемь пикселей рамки изменения размера — при одном по бокам, —
        // и они ложатся полосой над заголовком: рамку студия красит в цвет
        // темы, так что пустота эта не невидимая, а хорошо заметная.
        //
        // Тянуть окно за верхний край это не мешает: рамка остаётся, ей просто
        // нечего показывать. Заодно чинится разворот — прежде развёрнутое окно
        // вылезало на те же восемь пикселей за каждый край экрана.
        ExtendClientAreaToDecorationsHint = true;

        // До открытия окна ручки платформы ещё нет, и красить нечего.
        Opened += (_, _) => Paint();
        ActualThemeVariantChanged += (_, _) => Paint();
    }

    /// <summary>Приводит рамку окна к нынешней теме.</summary>
    private void Paint()
    {
        if (!OperatingSystem.IsWindows())
            return;

        if (TryGetPlatformHandle()?.Handle is not { } handle || handle == IntPtr.Zero)
            return;

        var dark = ActualThemeVariant != ThemeVariant.Light;
        var colour = dark ? Color.FromRgb(0x2B, 0x2D, 0x30) : Color.FromRgb(0xF7, 0xF8, 0xFA);

        if (this.TryFindResource("AxBg2Color", ActualThemeVariant, out var value) && value is Color found)
            colour = found;

        Paint(handle, colour, dark);
    }

    [SupportedOSPlatform("windows")]
    private static void Paint(IntPtr handle, Color border, bool dark)
    {
        try
        {
            var mode = dark ? 1 : 0;
            SetWindowAttribute(handle, UseImmersiveDarkMode, ref mode, sizeof(int));

            // COLORREF: 0x00BBGGRR — порядок каналов обратный привычному.
            var colour = border.R | (border.G << 8) | (border.B << 16);
            SetWindowAttribute(handle, BorderColor, ref colour, sizeof(int));
            SetWindowAttribute(handle, CaptionColor, ref colour, sizeof(int));
        }
        catch (DllNotFoundException)
        {
            // Рамка останется системного цвета — это некрасиво, но не мешает работать.
        }
        catch (EntryPointNotFoundException)
        {
        }
    }

    [DllImport("dwmapi.dll", EntryPoint = "DwmSetWindowAttribute", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int SetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);
}
