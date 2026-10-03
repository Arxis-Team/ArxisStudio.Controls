namespace ArxisStudio.Controls;

/// <summary>
/// Отрезок текста кода: <see cref="Length"/> знаков начиная с <see cref="Start"/>.
/// </summary>
/// <param name="Start">Смещение первого знака — в знаках UTF-16 от начала текста.</param>
/// <param name="Length">Число знаков; пустой отрезок стоит между двумя знаками.</param>
public readonly record struct AxCodeRange(int Start, int Length)
{
    /// <summary>Смещение за последним знаком отрезка.</summary>
    public int End => Start + Length;

    /// <summary>В отрезке нет ни одного знака.</summary>
    public bool IsEmpty => Length <= 0;
}
