using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace ArxisStudio.Controls;

/// <summary>
/// Окно «Цвет», как у Unity: выбор цвета поля (<see cref="AxColorField"/>) отдельным немодальным окном рядом с ним.
/// Окно одно на процесс, а сеанс правки в нём пишет одно значение.
/// </summary>
/// <remarks>
/// <para>
/// <b>Сеанс.</b> Всё, что меняется в окне, поле показывает на ходу (<see cref="AxColorField.ColorChanging"/>) — хозяин
/// видит цвет на объекте, — а фиксация одна: когда окно закрывают крестиком или Enter и когда оно переходит к другому
/// полю. Esc возвращает цвет начала (<see cref="AxColorField.ColorCancelled"/>) и закрывает окно; поле, ушедшее из
/// дерева насовсем, и окно поля, которое закрывается, — тоже отмена. Сеанс, вернувшийся к цвету начала, не пишет
/// ничего.
/// </para>
/// <para>
/// <b>Место.</b> Окно встаёт слева от поля, а не влезает — справа, в рабочей области экрана поля, и держится над окном
/// поля как его подчинённое; в панели задач его нет. Место ставится между первой раскладкой и показом: окно
/// появляется сразу там, где ему стоять.
/// </para>
/// </remarks>
internal sealed class AxColorPickerWindow : AxWindow
{
    /// <summary>Ключ заголовка окна в ресурсах темы.</summary>
    private const string TitleKey = "AxTextColorWindow";

    private static AxColorPickerWindow? _current;

    private readonly AxColorPicker _picker = new();
    private AxColorField? _field;
    private bool _changed;
    private bool _settled;
    private string? _text;
    private bool _byKeyboard;
    private bool _placed;

    /// <summary>Заводит окно: полоса заголовка «Цвет» и выбор цвета под ней.</summary>
    public AxColorPickerWindow()
    {
        ShowInTaskbar = false;
        CanResize = false;
        CanMinimize = false;
        CanMaximize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        WindowStartupLocation = WindowStartupLocation.Manual;

        var title = new TextBlock { VerticalAlignment = VerticalAlignment.Center };

        AxText.SetTone(title, AxTextTone.Secondary);
        title[!TextBlock.TextProperty] = title.GetResourceObservable(TitleKey).ToBinding();
        title[!MarginProperty] = title.GetResourceObservable("AxSpaceWideSidesThickness").ToBinding();
        title[!TextBlock.FontSizeProperty] = title.GetResourceObservable("AxFontSizeSmall").ToBinding();

        var bar = new AxTitleBar { Content = title };

        DockPanel.SetDock(bar, Dock.Top);

        var body = new Border { Child = _picker };

        body[!Border.PaddingProperty] = body.GetResourceObservable("AxSpaceWideThickness").ToBinding();

        Content = new DockPanel { Children = { bar, body } };
        this[!TitleProperty] = this.GetResourceObservable(TitleKey).ToBinding();

        _picker.ColorChanging += OnPickerColor;
        _picker.ColorCommitted += OnPickerColor;
        _picker.ColorCancelled += OnPickerCancelled;
        _picker.PropertyChanged += OnPickerProperty;
    }

    /// <summary>Открытое окно цвета; пусто — закрыто.</summary>
    internal static AxColorPickerWindow? Current => _current;

    /// <summary>Поле, чей сеанс идёт в окне.</summary>
    internal AxColorField? Field => _field;

    /// <summary>Выбор цвета окна.</summary>
    internal AxColorPicker Picker => _picker;

    /// <summary>Выбор цвета окна, если окно открыто для этого поля.</summary>
    /// <param name="field">Поле.</param>
    internal static AxColorPicker? PickerFor(AxColorField field) =>
        _current is { } window && ReferenceEquals(window._field, field) ? window._picker : null;

    /// <summary>
    /// Открывает окно для поля. Открытое для другого поля сперва фиксирует его сеанс и переходит к новому — или, если
    /// новое поле в другом окне, закрывается и открывается заново над ним.
    /// </summary>
    /// <param name="field">Поле.</param>
    /// <param name="byKeyboard">Открыли с клавиатуры: каретка в окне получит кольцо.</param>
    internal static void Open(AxColorField field, bool byKeyboard)
    {
        var owner = TopLevel.GetTopLevel(field) as Window;

        if (_current is { } open)
        {
            if (ReferenceEquals(open._field, field))
            {
                open.Activate();
                return;
            }

            if (owner is not null && ReferenceEquals(open.Owner, owner))
            {
                open.End(commit: true);
                open.Bind(field, byKeyboard);
                open.Activate();
                return;
            }

            open.Finish(commit: true, returnFocus: false);
        }

        var window = new AxColorPickerWindow();

        _current = window;
        window.Bind(field, byKeyboard);

        if (owner is not null)
            window.Show(owner);
        else
            window.Show();
    }

    /// <summary>Кончает сеанс поля и закрывает окно, если оно открыто для этого поля.</summary>
    /// <param name="field">Поле.</param>
    /// <param name="commit">Зафиксировать цвет или вернуть цвет начала.</param>
    internal static void Release(AxColorField field, bool commit)
    {
        if (_current is { } window && ReferenceEquals(window._field, field))
            window.Finish(commit, returnFocus: false);
    }

    /// <inheritdoc/>
    /// <remarks>Enter фиксирует цвет и закрывает окно, Esc возвращает цвет начала; поле, которое их взяло, — раньше.</remarks>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        if (e.KeyModifiers == KeyModifiers.None && e.Key is Key.Enter or Key.Escape)
        {
            Finish(commit: e.Key == Key.Enter, returnFocus: true);
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Хозяина окно получает после первой раскладки и до показа: тогда его размер уже известен, а на экране оно ещё
    /// не стоит, — и место ставится здесь.
    /// </remarks>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        ArgumentNullException.ThrowIfNull(change);

        base.OnPropertyChanged(change);

        if (change.Property == OwnerProperty && !_placed && _field is { } field)
            Place(field);
    }

    /// <inheritdoc/>
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        if (!_placed && _field is { } field)
            Place(field);
    }

    /// <inheritdoc/>
    /// <remarks>Закрыли крестиком — фиксация; закрывается окно поля или вся студия — отмена.</remarks>
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnClosing(e);

        if (e.Cancel)
            return;

        var commit = e.CloseReason is WindowCloseReason.WindowClosing or WindowCloseReason.Undefined;

        End(commit, returnFocus: commit);
    }

    /// <inheritdoc/>
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        if (ReferenceEquals(_current, this))
            _current = null;
    }

    /// <summary>Встаёт сеансом поля: цвет поля — «стало» и «было», вид каналов — поля.</summary>
    private void Bind(AxColorField field, bool byKeyboard)
    {
        _field = field;
        _changed = false;
        _settled = false;
        _text = null;
        _byKeyboard = byKeyboard;

        field.BeginSession();

        _picker.AllowsAlpha = field.AllowsAlpha;
        _picker.Mode = field.PickerMode;
        _picker.Color = field.Color;
        _picker.OriginalColor = field.IsNone || field.IsMixed ? null : field.Color;

        // Каретка — в квадрат, когда окно уже на экране: до показа ставить её некуда.
        Dispatcher.UIThread.Post(() =>
        {
            if (ReferenceEquals(_field, field))
                _picker.FocusSpectrum(byKeyboard ? NavigationMethod.Tab : NavigationMethod.Pointer);
        }, DispatcherPriority.Loaded);
    }

    /// <summary>Кончает сеанс и закрывает окно; каретка возвращается к полосе поля, если просили.</summary>
    private void Finish(bool commit, bool returnFocus)
    {
        End(commit, returnFocus);
        Close();
    }

    /// <summary>
    /// Кончает сеанс поля: фиксирует цвет окна или возвращает цвет начала. Окно остаётся — закрывает или
    /// переводит его тот, кто позвал.
    /// </summary>
    private void End(bool commit, bool returnFocus = false)
    {
        if (_field is not { } field)
            return;

        _field = null;

        if (!_changed)
            field.EndSession();
        else if (commit)
            field.CommitSession(_picker.Color, _text);
        else
            field.RevertSession(end: true);

        field.SetCurrentValue(AxColorField.IsPickerOpenProperty, false);

        if (returnFocus)
            field.ReturnFocus(_byKeyboard);
    }

    /// <summary>Ставит окно слева от поля, а не влезает — справа; в рабочей области экрана поля.</summary>
    private void Place(AxColorField field)
    {
        _placed = true;

        if (TopLevel.GetTopLevel(field) is not { } top || !field.IsAttachedToVisualTree())
            return;

        var origin = field.PointToScreen(default);
        var corner = field.PointToScreen(new Point(field.Bounds.Width, field.Bounds.Height));
        var screen = top.Screens?.ScreenFromVisual(field) ?? top.Screens?.Primary;
        var scaling = screen?.Scaling ?? top.RenderScaling;
        var size = PixelSize.FromSize(FrameSize ?? ClientSize, scaling);
        var gap = (int)Math.Round((this.TryFindResource("AxSpace", out var space) && space is double value ? value : 0) * scaling);
        var x = origin.X - gap - size.Width;
        var y = origin.Y;

        if (screen?.WorkingArea is { Width: > 0, Height: > 0 } area)
        {
            if (x < area.X)
                x = corner.X + gap;

            x = Math.Clamp(x, area.X, Math.Max(area.X, area.Right - size.Width));
            y = Math.Clamp(y, area.Y, Math.Max(area.Y, area.Bottom - size.Height));
        }

        Position = new PixelPoint(x, y);
    }

    /// <summary>Цвет окна — на поле: хозяин видит его на объекте, а фиксация ждёт конца сеанса.</summary>
    private void OnPickerColor(object? sender, AxColorEventArgs e)
    {
        if (_field is not { } field)
            return;

        _changed = true;

        if (e.RoutedEvent == AxColorPicker.ColorCommittedEvent)
        {
            _settled = true;
            _text = e.Text;
        }
        else
        {
            _text = null;
        }

        field.ShowSession(e.Color);
    }

    /// <summary>
    /// Протяжку в окне отменили: выбор вернулся к цвету до неё. Если до неё что-то уже выбрали — поле показывает этот
    /// цвет; если нет — поле возвращается к тому, с чего начался сеанс.
    /// </summary>
    private void OnPickerCancelled(object? sender, RoutedEventArgs e)
    {
        if (_field is not { } field || !_changed)
            return;

        if (_settled)
        {
            field.ShowSession(_picker.Color);
            return;
        }

        _changed = false;
        field.RevertSession(end: false);
    }

    /// <summary>Вид каналов, выбранный в окне, — полю: хозяин помнит его для следующего раза.</summary>
    private void OnPickerProperty(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == AxColorPicker.ModeProperty && _field is { } field)
            field.SetCurrentValue(AxColorField.PickerModeProperty, _picker.Mode);
    }
}
