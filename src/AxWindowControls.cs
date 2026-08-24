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

    /// <summary>Платформа ждёт кнопок от приложения, а не рисует их сама.</summary>
    public static bool IsSupported => !OperatingSystem.IsMacOS();

    /// <inheritdoc cref="IsMaximizedProperty"/>
    public bool IsMaximized
    {
        get => GetValue(IsMaximizedProperty);
        private set => SetValue(IsMaximizedProperty, value);
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

        IsVisible = IsSupported;

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
