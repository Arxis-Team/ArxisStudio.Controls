using System.Collections.ObjectModel;
using Avalonia.Media;

namespace ArxisStudio.Controls;

/// <summary>Образец цвета: имя и цвет, как пресет в окне Color у Unity.</summary>
/// <param name="Name">Имя — подсказка ячейки и подпись строки в виде списком.</param>
/// <param name="Color">Цвет.</param>
public sealed record AxColorPreset(string Name, Color Color);

/// <summary>Где лежит библиотека образцов.</summary>
public enum AxColorPresetLocation
{
    /// <summary>У человека: образцы ходят с ним из решения в решение.</summary>
    User,

    /// <summary>У решения: образцы едут вместе с ним в репозитории — палитра команды.</summary>
    Project,
}

/// <summary>Как стоят образцы: сеткой ячеек или списком с именами.</summary>
public enum AxColorPresetView
{
    /// <summary>Сеткой ячеек; имя — в подсказке.</summary>
    Grid,

    /// <summary>Списком: ячейка и имя в строке.</summary>
    List,
}

/// <summary>Библиотека образцов: имя, место и образцы по порядку.</summary>
/// <remarks>
/// Образцы правит выбор цвета — добавляет, заменяет, удаляет, переставляет, — а сохраняет хранилище
/// (<see cref="IAxColorPresetStore.Save"/>): библиотека сама о диске не знает.
/// </remarks>
/// <param name="name">Имя библиотеки.</param>
/// <param name="location">Где она лежит.</param>
/// <param name="presets">Образцы, с которых она начинается.</param>
public sealed class AxColorPresetLibrary(string name, AxColorPresetLocation location, IEnumerable<AxColorPreset>? presets = null)
{
    /// <summary>Имя библиотеки.</summary>
    public string Name { get; } = name;

    /// <summary>Где лежит библиотека.</summary>
    public AxColorPresetLocation Location { get; } = location;

    /// <summary>Образцы по порядку — так их показывает выбор цвета.</summary>
    public ObservableCollection<AxColorPreset> Presets { get; } = new(presets ?? []);
}

/// <summary>
/// Хранилище образцов цвета: библиотеки, текущая из них и вид, — то, что выбор цвета (<see cref="AxColorPicker"/>)
/// показывает в разделе «Образцы» и куда он пишет.
/// </summary>
/// <remarks>
/// Хранилище даёт хозяин — студия держит библиотеки файлами в профиле человека и рядом с решением — и ставит его
/// наследуемым свойством <see cref="AxColorPicker.PresetStoreProperty"/>. Без хранилища раздела образцов нет.
/// </remarks>
public interface IAxColorPresetStore
{
    /// <summary>Библиотеки, которые можно выбрать.</summary>
    IReadOnlyList<AxColorPresetLibrary> Libraries { get; }

    /// <summary>Текущая библиотека — её образцы стоят в разделе; хранилище держит её всегда.</summary>
    AxColorPresetLibrary Current { get; }

    /// <summary>Как стоят образцы.</summary>
    AxColorPresetView View { get; set; }

    /// <summary>Можно ли завести библиотеку в этом месте: у решения — только пока оно открыто.</summary>
    /// <param name="location">Место.</param>
    bool CanCreate(AxColorPresetLocation location);

    /// <summary>Делает библиотеку текущей.</summary>
    /// <param name="library">Библиотека из <see cref="Libraries"/>.</param>
    void Select(AxColorPresetLibrary library);

    /// <summary>Заводит пустую библиотеку и делает её текущей.</summary>
    /// <param name="name">Имя.</param>
    /// <param name="location">Место.</param>
    /// <returns>Заведённая библиотека.</returns>
    AxColorPresetLibrary Create(string name, AxColorPresetLocation location);

    /// <summary>Сохраняет образцы библиотеки — после каждой правки.</summary>
    /// <param name="library">Библиотека.</param>
    void Save(AxColorPresetLibrary library);

    /// <summary>Библиотеки, текущая или вид сменились.</summary>
    event EventHandler? Changed;
}
