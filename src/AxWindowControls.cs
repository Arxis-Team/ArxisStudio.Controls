using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace ArxisStudio.Controls;

/// <summary>
/// Кнопки окна — свернуть, развернуть, закрыть — для окна со своим заголовком.
/// </summary>
/// <remarks>
/// В Int UI главный тулбар и заголовок окна — одна полоса, поэтому системная
/// рамка выключается, а кнопки рисует приложение. На macOS их рисует система
/// слева, и там контрол прячется сам: <see cref="IsSupported"/> ложно.
/// </remarks>
public class AxWindowControls : TemplatedControl
{
    private Button? _minimize;
    private Button? _maximize;
    private Button? _close;
    private Window? _window;

    /// <summary>Окно развёрнуто — кнопка предлагает восстановить размер.</summary>
    public static readonly StyledProperty<bool> IsMaximizedProperty =
        AvaloniaProperty.Register<AxWindowControls, bool>(nameof(IsMaximized));

    /// <summary>
    /// Показывать ли кнопку «свернуть».
    /// </summary>
    /// <remarks>
    /// Есть окна, которым сворачиваться некуда. Окно при другом окне не заводит
    /// себе кнопки в панели задач и не встаёт в Alt+Tab: свёрнутое, оно
    /// исчезает без следа, и вернуть его человеку нечем. Кнопка, у которой нет
    /// дороги назад, — ловушка, и такому окну её не дают.
    /// </remarks>
    public static readonly StyledProperty<bool> ShowMinimizeProperty =
        AvaloniaProperty.Register<AxWindowControls, bool>(nameof(ShowMinimize), defaultValue: true);

    /// <summary>
    /// Показывать ли кнопку «развернуть».
    /// </summary>
    /// <remarks>
    /// Окну-палитре её не дают: у такого окна узкая шапка и один крестик — так
    /// заведено в Windows и так поступают среды разработки. Сам разворот при
    /// этом никуда не девается: двойной щелчок по шапке разворачивает и
    /// возвращает окно на всех окнах системы, и жест этот сам себе обратный.
    /// </remarks>
    public static readonly StyledProperty<bool> ShowMaximizeProperty =
        AvaloniaProperty.Register<AxWindowControls, bool>(nameof(ShowMaximize), defaultValue: true);

    /// <summary>Платформа ждёт кнопок от приложения, а не рисует их сама.</summary>
    public static bool IsSupported => !OperatingSystem.IsMacOS();

    /// <inheritdoc cref="IsMaximizedProperty"/>
    public bool IsMaximized
    {
        get => GetValue(IsMaximizedProperty);
        private set => SetValue(IsMaximizedProperty, value);
    }

    /// <inheritdoc cref="ShowMinimizeProperty"/>
    public bool ShowMinimize
    {
        get => GetValue(ShowMinimizeProperty);
        set => SetValue(ShowMinimizeProperty, value);
    }

    /// <inheritdoc cref="ShowMaximizeProperty"/>
    public bool ShowMaximize
    {
        get => GetValue(ShowMaximizeProperty);
        set => SetValue(ShowMaximizeProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        Detach();

        _minimize = e.NameScope.Find<Button>("PART_Minimize");
        _maximize = e.NameScope.Find<Button>("PART_Maximize");
        _close = e.NameScope.Find<Button>("PART_Close");

        if (_minimize is not null)
            _minimize.Click += OnMinimize;

        if (_maximize is not null)
            _maximize.Click += OnMaximize;

        if (_close is not null)
            _close.Click += OnClose;
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // Прятать себя контрол вправе только там, где кнопки рисует система:
        // присваивание IsVisible — локальное значение, а оно старше привязки
        // из шаблона, и ShowWindowControls="False" переставал что-либо значить.
        if (!IsSupported)
            IsVisible = false;

        _window = this.FindAncestorOfType<Window>();
        if (_window is not null)
        {
            _window.PropertyChanged += OnWindowPropertyChanged;
            IsMaximized = _window.WindowState == WindowState.Maximized;
        }
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_window is not null)
        {
            _window.PropertyChanged -= OnWindowPropertyChanged;
            _window = null;
        }

        base.OnDetachedFromVisualTree(e);
    }

    private void OnWindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Window.WindowStateProperty && sender is Window window)
            IsMaximized = window.WindowState == WindowState.Maximized;
    }

    private void OnMinimize(object? sender, RoutedEventArgs e)
    {
        if (_window is not null)
            _window.WindowState = WindowState.Minimized;
    }

    private void OnMaximize(object? sender, RoutedEventArgs e)
    {
        if (_window is null)
            return;

        _window.WindowState = _window.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void OnClose(object? sender, RoutedEventArgs e) => _window?.Close();

    private void Detach()
    {
        if (_minimize is not null)
            _minimize.Click -= OnMinimize;

        if (_maximize is not null)
            _maximize.Click -= OnMaximize;

        if (_close is not null)
            _close.Click -= OnClose;
    }
}
