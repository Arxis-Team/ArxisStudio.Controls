using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace ArxisStudio.Controls;

/// <summary>Ползунок числового значения.</summary>
/// <remarks>
/// <para>
/// <b>Протяжка — сеансом</b>, как у числового поля: от нажатия до отпускания значение видно на ходу
/// (<see cref="AxNumberField.ValueChangingEvent"/>), а фиксируется одно — при отпускании
/// (<see cref="AxNumberField.ValueCommittedEvent"/>) и только если сдвинулось. Нажатие по дорожке — тоже сеанс:
/// значение встаёт под указатель и фиксируется при отпускании. Esc посреди протяжки возвращает значение начала
/// (<see cref="AxNumberField.ScrubCancelledEvent"/>), и до отпускания ползунок за указателем больше не идёт.
/// </para>
/// <para>
/// Шаг с клавиатуры — стрелки, PageUp и PageDown, Home и End — фиксируется каждый. Значение, поставленное хозяином,
/// событий не поднимает: они говорят о том, что сделал человек.
/// </para>
/// <para>
/// Нажатие и движение разбирает сам <see cref="Slider"/>, и привязка к делениям и раскладка дорожки остаются его.
/// Ползунок их наблюдает туннелем — раньше частей шаблона: нажатие на дорожку застаёт значение ещё прежним.
/// </para>
/// </remarks>
[PseudoClasses(":scrubbing")]
public class AxSlider : Slider
{
    private double _origin;
    private bool _scrubbing;
    private bool _cancelled;
    private bool _moved;

    /// <summary>Заводит ползунок: сеанс протяжки слушается раньше частей шаблона.</summary>
    public AxSlider()
    {
        AddHandler(PointerPressedEvent, OnPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(PointerReleasedEvent, OnReleased, RoutingStrategies.Bubble, handledEventsToo: true);
    }

    /// <inheritdoc cref="AxNumberField.ValueCommittedEvent"/>
    public event EventHandler<AxNumberEventArgs>? ValueCommitted
    {
        add => AddHandler(AxNumberField.ValueCommittedEvent, value);
        remove => RemoveHandler(AxNumberField.ValueCommittedEvent, value);
    }

    /// <inheritdoc cref="AxNumberField.ValueChangingEvent"/>
    public event EventHandler<AxNumberEventArgs>? ValueChanging
    {
        add => AddHandler(AxNumberField.ValueChangingEvent, value);
        remove => RemoveHandler(AxNumberField.ValueChangingEvent, value);
    }

    /// <inheritdoc cref="AxNumberField.ScrubCancelledEvent"/>
    public event EventHandler<RoutedEventArgs>? ScrubCancelled
    {
        add => AddHandler(AxNumberField.ScrubCancelledEvent, value);
        remove => RemoveHandler(AxNumberField.ScrubCancelledEvent, value);
    }

    /// <summary>Идёт протяжка: от нажатия до отпускания.</summary>
    public bool IsScrubbing => _scrubbing;

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        ArgumentNullException.ThrowIfNull(change);

        base.OnPropertyChanged(change);

        if (change.Property == ValueProperty && _scrubbing && !_cancelled)
        {
            _moved = true;
            RaiseEvent(new AxNumberEventArgs(AxNumberField.ValueChangingEvent, Value));
        }
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        if (e.Key == Key.Escape && _scrubbing && !_cancelled)
        {
            Cancel();
            e.Handled = true;
            return;
        }

        var before = Value;

        base.OnKeyDown(e);

        // Шаг клавишей — фиксация; посреди протяжки клавиши значение не трогают: его ведёт указатель.
        if (!_scrubbing && !Value.Equals(before))
            RaiseEvent(new AxNumberEventArgs(AxNumberField.ValueCommittedEvent, Value));
    }

    private void OnPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsEffectivelyEnabled || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        _scrubbing = true;
        _cancelled = false;
        _moved = false;
        _origin = Value;
        PseudoClasses.Set(":scrubbing", true);

        // Клавиатура переезжает на ползунок вместе с нажатием: Esc посреди протяжки обязан дойти до него.
        Focus(NavigationMethod.Pointer);
    }

    private void OnReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_scrubbing || e.InitialPressMouseButton != MouseButton.Left)
            return;

        var commit = !_cancelled && _moved && !Value.Equals(_origin);

        _scrubbing = false;
        _cancelled = false;
        _moved = false;
        PseudoClasses.Set(":scrubbing", false);

        if (commit)
            RaiseEvent(new AxNumberEventArgs(AxNumberField.ValueCommittedEvent, Value));
    }

    /// <summary>Отменяет протяжку: значение начала — обратно, и до отпускания ползунок стоит.</summary>
    private void Cancel()
    {
        _cancelled = true;

        // Тягу ведёт сам Slider до отпускания кнопки; кончить её можно только его же концом тяги ручки — иначе дорожка
        // шла бы за указателем и после отмены.
        OnThumbDragCompleted(new VectorEventArgs { RoutedEvent = Thumb.DragCompletedEvent });
        SetCurrentValue(ValueProperty, _origin);
        RaiseEvent(new RoutedEventArgs(AxNumberField.ScrubCancelledEvent));
    }
}
