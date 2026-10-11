using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace ArxisStudio.Controls;

/// <summary>Как образец показывает прозрачность цвета.</summary>
public enum AxColorAlphaDisplay
{
    /// <summary>
    /// Цвет — непрозрачным, а прозрачность — полоской снизу: белое — доля непрозрачности, чёрное — остаток. Так поле
    /// цвета у Unity: оттенок читается целиком при любой прозрачности.
    /// </summary>
    Bar,

    /// <summary>Цвет как есть поверх шахматки: прозрачное видно насквозь — образцы выбора цвета и палитры.</summary>
    Checker,
}

/// <summary>
/// Образец цвета: рисует цвет, его прозрачность и «нет цвета» сам — в поле цвета, в образцах «было/стало» выбора и в
/// клетках палитры.
/// </summary>
/// <remarks>
/// <para>
/// Рисунок — не шаблон, а <see cref="Render"/>: доля непрозрачности ставит ширину белой части полоски, а шаблону темы
/// нечем перевести число в ширину. Кисти и высоту полоски задаёт тема — у образца своих чисел нет.
/// </para>
/// <para>
/// <see cref="Brush"/> рисуется вместо цвета как есть, поверх шахматки: так видно кисть, которую цветом не описать, —
/// градиент. <see cref="IsNone"/> — цвета нет вовсе: шахматка, перечёркнутая косой чертой из нижнего левого угла.
/// </para>
/// </remarks>
public class AxColorSwatch : Control
{
    /// <summary>Цвет образца.</summary>
    public static readonly StyledProperty<Color> ColorProperty =
        AvaloniaProperty.Register<AxColorSwatch, Color>(nameof(Color), Colors.White);

    /// <summary>Кисть вместо цвета — то, что цветом не описать: градиент. Пусто — рисуется цвет.</summary>
    public static readonly StyledProperty<IBrush?> BrushProperty =
        AvaloniaProperty.Register<AxColorSwatch, IBrush?>(nameof(Brush));

    /// <summary>Цвета нет: шахматка и косая черта.</summary>
    public static readonly StyledProperty<bool> IsNoneProperty =
        AvaloniaProperty.Register<AxColorSwatch, bool>(nameof(IsNone));

    /// <summary>Как показать прозрачность.</summary>
    public static readonly StyledProperty<AxColorAlphaDisplay> AlphaDisplayProperty =
        AvaloniaProperty.Register<AxColorSwatch, AxColorAlphaDisplay>(nameof(AlphaDisplay));

    /// <summary>Высота полоски прозрачности у <see cref="AxColorAlphaDisplay.Bar"/>.</summary>
    public static readonly StyledProperty<double> AlphaBarHeightProperty =
        AvaloniaProperty.Register<AxColorSwatch, double>(nameof(AlphaBarHeight));

    /// <summary>Доля непрозрачности на полоске.</summary>
    public static readonly StyledProperty<IBrush?> AlphaFillBrushProperty =
        AvaloniaProperty.Register<AxColorSwatch, IBrush?>(nameof(AlphaFillBrush));

    /// <summary>Остаток полоски — прозрачная доля.</summary>
    public static readonly StyledProperty<IBrush?> AlphaEmptyBrushProperty =
        AvaloniaProperty.Register<AxColorSwatch, IBrush?>(nameof(AlphaEmptyBrush));

    /// <summary>Шахматка под прозрачным.</summary>
    public static readonly StyledProperty<IBrush?> CheckerBrushProperty =
        AvaloniaProperty.Register<AxColorSwatch, IBrush?>(nameof(CheckerBrush));

    /// <summary>Косая черта «нет цвета».</summary>
    public static readonly StyledProperty<IBrush?> NoneBrushProperty =
        AvaloniaProperty.Register<AxColorSwatch, IBrush?>(nameof(NoneBrush));

    /// <summary>Толщина косой черты «нет цвета».</summary>
    public static readonly StyledProperty<double> NoneThicknessProperty =
        AvaloniaProperty.Register<AxColorSwatch, double>(nameof(NoneThickness), 1);

    /// <summary>Скругление образца.</summary>
    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<AxColorSwatch, CornerRadius>(nameof(CornerRadius));

    static AxColorSwatch() =>
        AffectsRender<AxColorSwatch>(
            ColorProperty, BrushProperty, IsNoneProperty, AlphaDisplayProperty, AlphaBarHeightProperty,
            AlphaFillBrushProperty, AlphaEmptyBrushProperty, CheckerBrushProperty, NoneBrushProperty,
            NoneThicknessProperty, CornerRadiusProperty);

    /// <inheritdoc cref="ColorProperty"/>
    public Color Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <inheritdoc cref="BrushProperty"/>
    public IBrush? Brush
    {
        get => GetValue(BrushProperty);
        set => SetValue(BrushProperty, value);
    }

    /// <inheritdoc cref="IsNoneProperty"/>
    public bool IsNone
    {
        get => GetValue(IsNoneProperty);
        set => SetValue(IsNoneProperty, value);
    }

    /// <inheritdoc cref="AlphaDisplayProperty"/>
    public AxColorAlphaDisplay AlphaDisplay
    {
        get => GetValue(AlphaDisplayProperty);
        set => SetValue(AlphaDisplayProperty, value);
    }

    /// <inheritdoc cref="AlphaBarHeightProperty"/>
    public double AlphaBarHeight
    {
        get => GetValue(AlphaBarHeightProperty);
        set => SetValue(AlphaBarHeightProperty, value);
    }

    /// <inheritdoc cref="AlphaFillBrushProperty"/>
    public IBrush? AlphaFillBrush
    {
        get => GetValue(AlphaFillBrushProperty);
        set => SetValue(AlphaFillBrushProperty, value);
    }

    /// <inheritdoc cref="AlphaEmptyBrushProperty"/>
    public IBrush? AlphaEmptyBrush
    {
        get => GetValue(AlphaEmptyBrushProperty);
        set => SetValue(AlphaEmptyBrushProperty, value);
    }

    /// <inheritdoc cref="CheckerBrushProperty"/>
    public IBrush? CheckerBrush
    {
        get => GetValue(CheckerBrushProperty);
        set => SetValue(CheckerBrushProperty, value);
    }

    /// <inheritdoc cref="NoneBrushProperty"/>
    public IBrush? NoneBrush
    {
        get => GetValue(NoneBrushProperty);
        set => SetValue(NoneBrushProperty, value);
    }

    /// <inheritdoc cref="NoneThicknessProperty"/>
    public double NoneThickness
    {
        get => GetValue(NoneThicknessProperty);
        set => SetValue(NoneThicknessProperty, value);
    }

    /// <inheritdoc cref="CornerRadiusProperty"/>
    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var bounds = new Rect(Bounds.Size);

        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        // Без скругления — прямоугольником: обрезку скруглённым прямоугольником умеет не всякий контекст рисования.
        using var clip = CornerRadius == default
            ? context.PushClip(bounds)
            : context.PushGeometryClip(new RectangleGeometry(bounds) { RadiusX = CornerRadius.TopLeft, RadiusY = CornerRadius.TopLeft });

        if (IsNone)
        {
            Fill(context, CheckerBrush, bounds);

            if (NoneBrush is { } none)
                context.DrawLine(new ImmutablePen(none.ToImmutable(), NoneThickness), bounds.BottomLeft, bounds.TopRight);

            return;
        }

        if (Brush is { } brush)
        {
            Fill(context, CheckerBrush, bounds);
            Fill(context, brush, bounds);
            return;
        }

        var color = Color;

        if (AlphaDisplay == AxColorAlphaDisplay.Checker)
        {
            if (color.A < byte.MaxValue)
                Fill(context, CheckerBrush, bounds);

            Fill(context, new ImmutableSolidColorBrush(color), bounds);
            return;
        }

        var bar = Math.Clamp(AlphaBarHeight, 0, bounds.Height);
        var solid = new Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height - bar);

        Fill(context, new ImmutableSolidColorBrush(Color.FromRgb(color.R, color.G, color.B)), solid);

        if (bar <= 0)
            return;

        // Доля непрозрачности — в целых точках: полоска в пару пикселей с дробным краем читалась бы серой.
        var filled = Math.Round(bounds.Width * color.A / byte.MaxValue);
        var top = bounds.Bottom - bar;

        Fill(context, AlphaFillBrush, new Rect(bounds.X, top, filled, bar));
        Fill(context, AlphaEmptyBrush, new Rect(bounds.X + filled, top, bounds.Width - filled, bar));
    }

    private static void Fill(DrawingContext context, IBrush? brush, Rect area)
    {
        if (brush is not null && area.Width > 0 && area.Height > 0)
            context.FillRectangle(brush, area);
    }
}
