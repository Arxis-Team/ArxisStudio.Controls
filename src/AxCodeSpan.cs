namespace ArxisStudio.Controls;

/// <summary>
/// Кусок кода с ролью: <see cref="Length"/> знаков начиная с <see cref="Start"/>.
/// </summary>
/// <param name="Start">Смещение первого знака — в знаках UTF-16 от начала текста.</param>
/// <param name="Length">Число знаков.</param>
/// <param name="Role">Что это за кусок.</param>
/// <remarks>
/// Куски приходят от хозяина, который знает формат, — контрол текст не разбирает. Порядок любой,
/// пересечения недопустимы: кусок, зашедший на предыдущий, начинается там, где тот кончился.
/// </remarks>
public readonly record struct AxCodeSpan(int Start, int Length, AxCodeRole Role)
{
    /// <summary>Смещение за последним знаком куска.</summary>
    public int End => Start + Length;
}
