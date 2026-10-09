using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace ArxisStudio.Controls;

/// <summary>
/// Подпись числового поля, за которую его тянут: протяжка влево и вправо меняет число, как у Unity.
/// </summary>
/// <remarks>
/// <para>
/// Протяжка начинается, когда указатель ушёл от точки нажатия дальше мёртвой зоны (<see cref="DeadZone"/>), и
/// идёт, пока кнопку не отпустили. Отпущенная фиксирует протянутое одним значением; Esc возвращает прежнее. Щелчок без протяжки
/// ставит каретку в поле: подпись и поле — одна строка.
/// </para>
/// <para>
/// Мышью дело не кончается: то же поле шагает стрелками, и протяжка для него — удобство, а не единственная
/// дорога. Подпись называет поле диктору (<see cref="AutomationProperties.LabeledByProperty"/>), а сама в
/// обход клавиатуры не встаёт.
/// </para>
/// </remarks>
[PseudoClasses(":scrubbable", ":scrubbing")]
public class AxScrubLabel : ContentControl
{
    /// <summary>Поле, которое тянет подпись.</summary>
    public static readonly StyledProperty<AxNumberField?> TargetProperty =
        AvaloniaProperty.Register<AxScrubLabel, AxNumberField?>(nameof(Target));

    /// <summary>
    /// Сколько пройти мышью, чтобы нажатие стало протяжкой, а не щелчком: четыре точки, как у Unity.
    /// </summary>
    /// <remarks>
    /// Меньше порога тяги вкладок студии нарочно: вкладку, сдвинутую по ошибке, приходится возвращать, а
    /// протянутое на шаг число — нет, и протяжка, начатая раньше, ощущается ровнее.
    /// </remarks>
    public const double DeadZone = 4;

    private Point? _pressed;
    private double _last;
    private bool _dragging;
    private IPointer? _pointer;
    private TopLevel? _keys;

    static AxScrubLabel()
    {
        FocusableProperty.OverrideDefaultValue<AxScrubLabel>(false);
        TargetProperty.Changed.AddClassHandler<AxScrubLabel>((label, change) =>
            label.Follow(change.GetOldValue<AxNumberField?>(), change.GetNewValue<AxNumberField?>()));
    }

    /// <inheritdoc cref="TargetProperty"/>
    public AxNumberField? Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        PseudoClasses.Set(":scrubbable", Target is { CanScrub: true });
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnPointerPressed(e);

        if (Target is null || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        _pressed = e.GetPosition(this);
        _last = _pressed.Value.X;
        _pointer = e.Pointer;
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

        PseudoClasses.Set(":scrubbable", false);
    }

    /// <inheritdoc/>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnPointerMoved(e);

        if (_pressed is not { } pressed || Target is not { } target)
            return;

        var x = e.GetPosition(this).X;

        if (!_dragging)
        {
            if (Math.Abs(x - pressed.X) < DeadZone || !target.BeginScrub())
                return;

            _dragging = true;
            _last = x;
            PseudoClasses.Set(":scrubbing", true);
            Listen();
            return;
        }

        target.ScrubBy(x - _last, e.KeyModifiers);
        _last = x;
        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnPointerReleased(e);

        if (_pressed is null)
            return;

        var dragged = _dragging;

        Finish(commit: true);

        // Щелчок без протяжки — то же, что щелчок по полю: каретка в нём, набранное выделено.
        if (!dragged && Target is { } target)
        {
            target.Focus(NavigationMethod.Pointer);
            target.SelectAll();
        }

        e.Handled = true;
    }

    /// <inheritdoc/>
    /// <remarks>Захват ушёл — окно потеряло мышь, его закрыли: протянутое уже видно и фиксируется.</remarks>
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);

        Finish(commit: true);
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        Finish(commit: true);
    }

    /// <summary>Кончает протяжку — зафиксировав или отменив её — и отпускает мышь.</summary>
    /// <remarks>
    /// Мышь отпускается и при отмене: Esc кончает протяжку раньше, чем отпущена кнопка, а подпись с
    /// захватом держала бы наведение на себе, пока кнопку не отпустят, — курсор шёл бы над соседями, и они
    /// бы его не видели. Сама Avalonia отпускает захват только с кнопкой.
    /// </remarks>
    private void Finish(bool commit)
    {
        Unlisten();

        var dragging = _dragging;
        var pointer = _pointer;

        _dragging = false;
        _pressed = null;
        _pointer = null;
        PseudoClasses.Set(":scrubbing", false);

        if (dragging)
            Target?.EndScrub(commit);

        if (pointer is not null && ReferenceEquals(pointer.Captured, this))
            pointer.Capture(null);
    }

    /// <summary>Слушает Esc, пока идёт протяжка: каретка у кого угодно, а отмена — у тянущего.</summary>
    private void Listen()
    {
        _keys = TopLevel.GetTopLevel(this);
        _keys?.AddHandler(KeyDownEvent, OnKey, RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    private void Unlisten()
    {
        _keys?.RemoveHandler(KeyDownEvent, OnKey);
        _keys = null;
    }

    private void OnKey(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape || !_dragging)
            return;

        Finish(commit: false);
        e.Handled = true;
    }

    /// <summary>Подпись называет поле диктору — и перестаёт, когда поле сменилось.</summary>
    private void Follow(AxNumberField? previous, AxNumberField? next)
    {
        Finish(commit: true);

        if (previous is not null && ReferenceEquals(AutomationProperties.GetLabeledBy(previous), this))
            previous.ClearValue(AutomationProperties.LabeledByProperty);

        if (next is not null && AutomationProperties.GetLabeledBy(next) is null)
            AutomationProperties.SetLabeledBy(next, this);
    }
}
