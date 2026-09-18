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
/// Цвет берётся из ресурса <c>AxSurfacePanelColor</c> по текущему варианту темы: рамка
/// примыкает к полосе заголовка, и разница цветов заметнее всего именно там.
/// Перекрашивается окно само — и при открытии, и при смене темы, — поэтому
/// обходить открытые окна снаружи никому не нужно.
/// </para>
/// <para>
/// Настройка появилась в Windows 11; на более ранних версиях вызов ничего не
/// делает, как и на других платформах.
/// </para>
/// <para>
/// Окно не открывается больше рабочей области своего экрана — ни размером, ни наименьшим размером:
/// числа из разметки заданы под обычный монитор, а ноутбук 1920 × 1080 при 150 % — это 1280 × 720
/// точек без панели задач.
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
        Opened += (_, _) =>
        {
            _opened = true;
            Paint();
        };
        ActualThemeVariantChanged += (_, _) => Paint();
    }

    /// <summary>Окно уже показывали: размер с этого мига держат человек и система.</summary>
    private bool _opened;

    /// <inheritdoc/>
    /// <remarks>
    /// Размер урезается при каждой его правке до первого показа — из разметки, из кода, из записанной
    /// раскладки, — а место окну выбирает показ: по центру уже урезанного размера. Один миг вроде
    /// конца разметки годился бы не всем: окно, заведённое кодом, получает размер уже после него.
    /// </remarks>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (!_opened
            && (change.Property == WidthProperty || change.Property == HeightProperty
                || change.Property == MinWidthProperty || change.Property == MinHeightProperty))
        {
            Fit();
        }
    }

    /// <summary>
    /// Урезает размер и наименьший размер окна до рабочей области экрана, на котором оно встанет.
    /// </summary>
    /// <remarks>
    /// Окно крупнее экрана вставало по центру с заголовком выше верхнего края — сдвинуть его было не
    /// за что, — а наименьший размер крупнее экрана не давал его и сжать. Наименьший трогается,
    /// только если не влезает: заданный стилем, он иначе прибился бы местным значением и перестал
    /// следовать теме.
    /// </remarks>
    private void Fit()
    {
        if ((Screens?.ScreenFromWindow(this) ?? Screens?.Primary) is not { } screen)
            return;

        var area = screen.WorkingArea.ToRect(screen.Scaling).Size;

        if (MinWidth > area.Width)
            MinWidth = area.Width;

        if (MinHeight > area.Height)
            MinHeight = area.Height;

        if (Width > area.Width)
            Width = area.Width;

        if (Height > area.Height)
            Height = area.Height;
    }

    /// <summary>Приводит рамку окна к нынешней теме.</summary>
    private void Paint()
    {
        if (!OperatingSystem.IsWindows())
            return;

        if (TryGetPlatformHandle()?.Handle is not { } handle || handle == IntPtr.Zero)
            return;

        var dark = ActualThemeVariant != ThemeVariant.Light;

        // Цвет рамки — только у темы. Запасное число здесь было вторым местом цвета панели и
        // отстало от палитры при первой же её настройке; без темы рамка остаётся системной.
        Color? border = this.TryFindResource("AxSurfacePanelColor", ActualThemeVariant, out var value) && value is Color found
            ? found
            : null;

        Paint(handle, border, dark);
    }

    [SupportedOSPlatform("windows")]
    private static void Paint(IntPtr handle, Color? border, bool dark)
    {
        try
        {
            var mode = dark ? 1 : 0;
            SetWindowAttribute(handle, UseImmersiveDarkMode, ref mode, sizeof(int));

            if (border is not { } tone)
                return;

            // COLORREF: 0x00BBGGRR — порядок каналов обратный привычному.
            var colour = tone.R | (tone.G << 8) | (tone.B << 16);
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
