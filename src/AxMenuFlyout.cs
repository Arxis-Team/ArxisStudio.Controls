using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace ArxisStudio.Controls;

/// <summary>
/// Контекстное меню студии: попап на тени <c>AxShadow</c>, пункты — родные
/// <see cref="MenuItem"/> или <see cref="AxMenuItem"/>, обоим тему даёт
/// ArxisTheme. Показывается штатно: <c>ContextFlyout</c> контрола либо
/// <c>ShowAt</c>.
/// </summary>
/// <remarks>
/// Карточка меню стоит в попапе не вплотную: вокруг неё тема оставляет поле, на котором
/// рисуется тень, — окно попапа обрезает всё, что за его краем, и без поля тень кончалась бы
/// прямой ступенькой. Место же попап отмеряет от своего края, а не от карточки, и меню,
/// поставленное «под кнопкой по левому краю», вставало бы на ширину поля правее и ниже. Поэтому
/// меню сдвигает попап на поле в сторону якоря — туда, откуда отмерено место, — и карточка
/// встаёт там, где место сказано.
/// <para>
/// Поле меню берёт у темы ключом <see cref="ShadowRoomKey"/>; нет ключа — сдвига нет. Смещения,
/// заданные меню снаружи, остаются как были: попап получает их вместе с поправкой, а свойства
/// после показа возвращаются к своим значениям — живьём попап их не перечитывает.
/// </para>
/// </remarks>
public class AxMenuFlyout : MenuFlyout
{
    /// <summary>Ключ поля под тень, которое тема оставляет вокруг карточки меню.</summary>
    public const string ShadowRoomKey = "AxPopupShadowRoom";

    /// <inheritdoc/>
    protected override bool ShowAtCore(Control placementTarget, bool showAtPointer = false)
    {
        ArgumentNullException.ThrowIfNull(placementTarget);

        var horizontal = HorizontalOffset;
        var vertical = VerticalOffset;

        if (placementTarget.TryFindResource(ShadowRoomKey, placementTarget.ActualThemeVariant, out var found) &&
            found is Thickness room)
        {
            var (x, y) = Compensation(showAtPointer ? PlacementMode.Pointer : Placement, room);

            HorizontalOffset = horizontal + x;
            VerticalOffset = vertical + y;
        }

        try
        {
            return base.ShowAtCore(placementTarget, showAtPointer);
        }
        finally
        {
            HorizontalOffset = horizontal;
            VerticalOffset = vertical;
        }
    }

    /// <summary>
    /// На сколько сдвинуть попап, чтобы карточка встала по месту, а не на поле дальше.
    /// </summary>
    /// <param name="placement">Место попапа.</param>
    /// <param name="room">Поле вокруг карточки.</param>
    /// <returns>Сдвиг по горизонтали и по вертикали.</returns>
    /// <remarks>
    /// По оси, по которой попап отмерен от края якоря, сдвиг — поле этой стороны навстречу якорю.
    /// По оси, по которой он выровнен по центру, — половина разницы полей, то есть ноль у
    /// симметричного поля. Место, которое задаёт не сторона, а якорь с направлением или чужой
    /// расчёт, не трогается: какой стороной карточка обращена к якорю, там неизвестно.
    /// </remarks>
    public static (double X, double Y) Compensation(PlacementMode placement, Thickness room)
    {
        var centreX = (room.Right - room.Left) / 2;
        var centreY = (room.Bottom - room.Top) / 2;

        return placement switch
        {
            PlacementMode.Pointer => (-room.Left, -room.Top),
            PlacementMode.Bottom => (centreX, -room.Top),
            PlacementMode.BottomEdgeAlignedLeft => (-room.Left, -room.Top),
            PlacementMode.BottomEdgeAlignedRight => (room.Right, -room.Top),
            PlacementMode.Top => (centreX, room.Bottom),
            PlacementMode.TopEdgeAlignedLeft => (-room.Left, room.Bottom),
            PlacementMode.TopEdgeAlignedRight => (room.Right, room.Bottom),
            PlacementMode.Right => (-room.Left, centreY),
            PlacementMode.RightEdgeAlignedTop => (-room.Left, -room.Top),
            PlacementMode.RightEdgeAlignedBottom => (-room.Left, room.Bottom),
            PlacementMode.Left => (room.Right, centreY),
            PlacementMode.LeftEdgeAlignedTop => (room.Right, -room.Top),
            PlacementMode.LeftEdgeAlignedBottom => (room.Right, room.Bottom),
            _ => (0, 0),
        };
    }
}
