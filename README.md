# ArxisStudio.Controls

Библиотека контролов ArxisStudio — виджеты, из которых строится интерфейс студии и её
плагинов. Аналог `Avalonia.Controls` в экосистеме Avalonia: плагины ArxisStudio строят
UI **только из Ax\*-контролов** (layout-панели Avalonia — Grid, StackPanel, DockPanel,
Border — разрешены как есть), как в Unity, где редакторный UI строится только из
контролов Unity.

Контролы **lookless**: шаблоны и цвета живут в теме — [`ArxisStudio.Themes.Arxis`](../ArxisStudio.Themes.Arxis/),
репозитории разрабатываются парой и ожидают друг друга рядом (sibling checkout).

## В разметке

Библиотека объявляет адрес пространства имён XAML — тот же, что и остальные
библиотеки ArxisStudio:

```xml
<AxUserControl xmlns="https://github.com/Arxis-Team/ArxisStudio"
               xmlns:a="https://github.com/avaloniaui"
               xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
  <a:StackPanel Spacing="8">
    <AxButton Appearance="Primary" Content="Готово"/>
    <a:TextBlock AxText.Tone="Secondary" Text="Изменения сохранены"/>
  </a:StackPanel>
</AxUserControl>
```

Один `xmlns` открывает столько, на сколько сборок ArxisStudio сослался проект.
Родные виджеты Avalonia под этот адрес не попадают намеренно: они пишутся с
префиксом — и видно, чего в наборе `Ax*` ещё нет.

## Вид — свойствами, а не классами

Вид контрола задают перечисления, и опечатку в них ловит компилятор разметки. Классов
вида тема не знает с SDK 6.0: класс с опечаткой молча давал контрол по умолчанию.

| Что | Свойство | Значения |
|---|---|---|
| вид кнопки | `AxButton.Appearance`, `AxToggleButton.Appearance`, `AxSplitButton.Appearance` | `Default`, `Primary`, `Subtle`, `Danger`, `Toolbar` |
| размер | `Size` у кнопки, переключателя, поля, вектора, жетонов и списка | `Normal`, `Compact` |
| роль и тон текста | `AxText.Role`, `AxText.Tone` у `TextBlock` | `Body`, `Small`, `Caption`, `Title`, `Section`, `Code`; `Primary`, `Secondary`, `Tertiary`, `Disabled`, `Accent`, `Error`, `Warning`, `Success` |
| проверка поля | `AxValidation.State` | `None`, `Warning`, `Error` |
| чип | `AxChip.Kind` | `Default`, `Accent`, `Key` |
| аватар | `AxAvatar.Shape`, `AxAvatar.Tint` | `Tile`, `Circle`; `Accent`, `Orange`, `Green`, `Purple`, `Red` |
| лоадер | `AxSpinner.Size` | `Normal`, `Large` |
| вкладки панели | `AxTabStrip.Kind` | `Document`, `ToolWindow` |
| пункт удаления | `AxMenuItem.IsDestructive` | `True` |

Тема видит значение псевдоклассом — `:primary`, `:compact`, `:tone-secondary`, — и значение
по умолчанию псевдокласса не несёт.

## Состав (M0)

| Контрол | База | Назначение |
|---|---|---|
| `AxButton` | `Button` | кнопка; вид `Appearance`, размер `Size` |
| `AxToggleButton` | `ToggleButton` | кнопка, которая остаётся нажатой: инструмент, фильтр, пункт навигации |
| `AxTextBox` | `TextBox` | однострочное поле ввода |
| `AxSearchField` | `AxTextBox` | поле поиска со значком-лупой |
| `AxCheckBox` | `CheckBox` | флажок 16×16 |
| `AxToggleSwitch` | `ToggleButton` | тумблер 30×17 |
| `AxComboBox` / `AxComboBoxItem` | `ComboBox` | выпадающий список |
| `AxListBox` / `AxListBoxItem` | `ListBox` | список с выделением строк; `IsDropTarget` отмечает строку, в которую ляжет перетаскиваемое |
| `AxSegmentedControl` / `AxSegmentItem` | `ListBox` | сегментный переключатель (Design/XAML/Split) |
| `AxBadge` | `ContentControl` | бейдж-счётчик |
| `AxChip` | `ContentControl` | чип-метка; вид `Kind` |
| `AxCard` | `ContentControl` | карточка-контейнер |
| `AxProgressBar` | `ProgressBar` | тонкий индикатор (4px) |
| `AxAvatar` | `TemplatedControl` | плитка с инициалами; `Shape` и `Tint` |
| `AxTextArea` | `AxTextBox` | многострочное поле |
| `AxLink` | `Button` | ссылка; состояние «посещённая» |
| `AxRadioButton` | `RadioButton` | выбор одного варианта |
| `AxDivider` | `Control` | линия в пиксель, горизонтальная или вертикальная; цвет `Fill` |
| `AxSplitter` | `GridSplitter` | граница областей: та же линия, но за неё можно взяться мышью и двигать стрелками; конец хода — `Moved`, от мыши и от стрелки вдоль оси |
| `AxGroupHeader` | `ContentControl` | заголовок секции с линией или полосой компонента, как у Unity (`Appearance`: `Rule`, `Band`) — со значком (`Icon`) и действиями справа (`Actions`) |
| `AxBanner` | `ContentControl` | сообщение: информация, успех, предупреждение, ошибка |
| `AxTabStrip` / `AxTabItem` | `ListBox` | вкладки документов и панелей (`Kind`): значок, метка правок, закрытие; диктору — набор вкладок и вкладка, а не список |
| `AxTreeView` / `AxTreeViewItem` | `TreeView` | дерево иерархии и файлов; значок — путь `Icon` и цвет `IconBrush`; диктор слышит раскрытие узла и раскрывает его сам |
| `AxBreadcrumb` / `AxBreadcrumbItem` | `ItemsControl` / `Button` | путь от корня до текущего места: сегмент — переход (`Navigated`), последний — текущий, ведущие при нехватке места уходят в меню |
| `AxSlider` | `Slider` | ползунок значения; протяжка — сеансом, как у числового поля: на ходу `ValueChanging`, одна фиксация при отпускании `ValueCommitted`, Esc — `ScrubCancelled` |
| `AxToolWindow` | `ContentControl` | панель инструментов: шапка с заголовком, вкладками и действиями |
| `AxUserControl` | `UserControl` | корень разметки: с него начинается панель, написанная на `.axaml` |
| `AxTitleBar` | `ContentControl` | полоса заголовка окна: перетаскивание, двойной щелчок, кнопки окна |
| `AxWindowControls` | `TemplatedControl` | свернуть, развернуть, закрыть; на macOS прячется |
| `AxWindow` | `Window` | окно студии: своя полоса заголовка при системной рамке, покрашенной в цвет темы; не открывается больше рабочей области экрана |

## Присоединённые свойства

Не контролы, а свойства, которые вешают на чужие: тема видит их псевдоклассами.

| Класс | Свойства | Назначение |
|---|---|---|
| `AxText` | `Role`, `Tone`, `LineHeightRatio` | роль, тон и высота строки у `TextBlock` |
| `AxValidation` | `State` | что проверка сказала о значении поля, флажка или списка |
| `AxToolTip` | `Gesture` | жест команды рядом с подсказкой; наследуемое — подсказку рисует отдельное окно |
| `AxTrimmedTip` | `IsEnabled` | подсказка с полным текстом у подписи, сокращённой многоточием |
| `AxSelectionScope` | `IsActive` | область выделения: строки внутри неё горят полным цветом, пока клавиатура в ней |

## Просмотр кода

`AxCodeView` показывает текст и не разбирает его: куски с ролями — `AxCodeSpan` из смещения, длины и
`AxCodeRole` — отдаёт хозяин, который знает формат. Отметка `Highlight` — отрезок, который показывает
хозяин, например элемент, выбранный на холсте; выделение — то, что отметил человек, его копирует
Ctrl+C. Каретку, поставленную человеком, сообщает `CaretMoved`; каретка из кода события не поднимает,
и хозяин, отвечающий на него отметкой, не ходит по кругу. `ScrollIntoView` показывает место: далёкое —
посередине экрана, видное не трогает.

```csharp
view.Text = xaml;
view.Spans = classifier.Classify(xaml);           // формат знает хозяин
view.Highlight = new AxCodeRange(start, length);  // выбранный элемент
view.ScrollIntoView(view.Highlight.Value);
view.CaretMoved += (_, e) => SelectElementAt(e.Offset);
```

Строится только видимое: документ в десятки тысяч строк стоит столько же, сколько экран. Для
сниппета в пару строк есть `AxCodeBlock`.

## Поля инспектора

| Контрол | База | Назначение |
|---|---|---|
| `AxNumberField` | `AxTextBox` | число: выражения и запятая, инвариантная запись, «авто» (`AllowsAuto`), разные значения (`IsMixed`), шаг стрелками; одна фиксация — `ValueCommitted` |
| `AxScrubLabel` | `ContentControl` | подпись, за которую тянут число (`Target`): видно на ходу — `ValueChanging`, фиксация при отпускании, Esc возвращает |
| `AxVectorField` | `TemplatedControl` | несколько чисел под общим именем — отступ, точка, размер; правка выходит вектором (`ValuesCommitted`), связанные (`IsLinked`) меняются разом |
| `AxRangeField` | `TemplatedControl` | число в границах ползунком и полем рядом, как `Range` у Unity; ползунок идёт делениями шага (`Step`), события — те же, что у `AxNumberField` |
| `AxPropertyRow` | `ContentControl` | строка свойства: подпись колонкой по правилу Unity, поле, кнопки (`Actions`), заметка (`Note`), отметка «задано здесь» (`IsOverridden`) |
| `AxTokenField` | `TemplatedControl` | слова чипами и строка ввода: классы стиля, теги; правка — `TokensChanged` со всем списком |
| `AxColorField` | `TemplatedControl` | цвет, как у Unity: полоса цвета с полоской прозрачности открывает окно «Цвет» — сеанс, который пишет одно значение, — пипетка (`ShowsEyedropper`) берёт пиксель экрана; «нет цвета» (`IsNone`), разные значения (`IsMixed`); код — `ColorText`, вид каналов окна — `PickerMode` |
| `AxColorSwatch` | `Control` | образец цвета, который рисует себя сам: цвет с полоской прозрачности или на шахматке (`AlphaDisplay`), кисть как есть (`Brush`), «нет цвета» (`IsNone`) |
| `AxColorPicker` | `TemplatedControl` | выбор цвета, как окно Color у Unity: пипетка, образцы «стало \| было» (`OriginalColor`), кольцо оттенков с квадратом, каналы в трёх видах (`Mode`: RGB 0…255, RGB 0…1, HSV) ползунками с числами, код из шести цифр, встроенная палитра (`Defaults`) и образцы хозяина (`PresetStore`); источник — модель HSV |
| `AxColorWheel` | `TemplatedControl` | кольцо оттенков: красный справа, оттенок растёт против часовой; внутри стоит квадрат; тянут мышью, шагают стрелками |
| `AxColorSpectrum` | `TemplatedControl` | насыщенность и яркость при оттенке; тянут мышью, шагают стрелками |
| `AxColorSlider` | `TemplatedControl` | один канал: оттенок, насыщенность, яркость, красный, зелёный, синий, непрозрачность (`Channel`), дорожка — градиент канала при остальных; диктору — диапазон |

Хозяин — инспектор или своё окно свойств — правит значение одной записью: протяжка и связанные ячейки
показывают значение на ходу, а фиксируют одно. Колонка подписей одна на список — её ставят списку
наследуемым `AxPropertyRow.LabelWidth`, — и вложенная строка (`Indent`) сужает подпись, а не сдвигает
поле. Тесно — поле уходит под подпись само.

```csharp
var width = new AxNumberField { AllowsAuto = true, Minimum = 0, Size = AxControlSize.Compact };
var row = new AxPropertyRow { Label = "Width", Content = width, ScrubTarget = width, IsOverridden = true };

width.ValueChanging += (_, e) => preview.Show(AxNumberField.Format(e.Value));   // протяжка видна
width.ValueCommitted += (_, e) => Write("Width", AxNumberField.Format(e.Value)); // одна правка
width.ScrubCancelled += (_, _) => preview.Dispose();                             // Esc
```

Чип снимается крестиком и клавишей (`AxChip.IsRemovable`, `RemoveRequested`) — снимает его хозяин.

Пипетка — `AxEyedropper.PickAsync`: снимок экрана застывает, у курсора лупа, щелчок отдаёт цвет пикселя. На
Windows снимаются все мониторы (GDI), на других системах — окно, из которого позвали.

Цвет и его части шлют одни события — `AxColorPicker.ColorChangingEvent`, `ColorCommittedEvent`, `ColorCancelledEvent`, а оттенок серого и чёрного держит модель HSV выбора. Образцы выбора цвета — библиотеки хранилища хозяина (`IAxColorPresetStore`, `AxColorPresetLibrary`, `AxColorPreset`), которое ставят наследуемым `AxColorPicker.PresetStore`: последняя ячейка — текущий цвет, щелчок сохраняет его с именем; правая кнопка — «Заменить» и «Удалить»; тяга и Alt+стрелка меняют порядок; ⋮ — вид сеткой или списком, библиотеки и новая — в профиле или в решении. Без хранилища раздела образцов нет, палитра остаётся. Поле цвета открывает окно «Цвет» — одно на процесс, рядом с полем: всё, что в нём меняется, поле показывает на ходу, а фиксирует одно значение, когда окно закрывают крестиком или Enter и когда оно переходит к другому полю; Esc возвращает цвет начала.

## Меню, диалоги, всплывающие

| Контрол | База | Назначение |
|---|---|---|
| `AxSplitButton` | `SplitButton` | кнопка с меню: действие слева, варианты справа |
| `AxDropDownButton` | `DropDownButton` | кнопка выбора с шевроном |
| `AxMenuFlyout` / `AxMenuItem` | `MenuFlyout` / `MenuItem` | контекстное меню: колонка иконок 16, шорткат справа |
| `AxSeparator` | `Separator` | черта между группами пунктов меню — и в корне, и в подменю строка сама по себе |
| `AxDialog` | `Window` | диалог без системной рамки на тени `AxShadowModal` |
| `AxQuickSearch` | `TemplatedControl` | попап поиска: запрос, результаты, подсказки клавиш |
| `AxToolBar` | `TemplatedControl` | главный тулбар: слоты слева, по центру и справа |
| `AxSpinner` | `TemplatedControl` | лоадер 16 или крупный (`Size`): оборот за 0.8 с линейно |
| `AxCodeBlock` | `TemplatedControl` | блок кода с подсветкой кистями `AxCode*` |
| `AxCodeView` | `TemplatedControl` | просмотр кода без правки: номера строк, подсветка кусками хозяина, отметка, каретка, выделение; строит только видимые строки |
| `AxDataGrid` | `AxListBox` | таблица-список со строкой заголовков |

Метрики и состояния задаёт тема по дизайн-системе студии
([`docs/design-system.md`](../../docs/design-system.md)): строка 24, контур
фокуса 2px, скругление 4, кнопка 28 высотой. У каждого интерактивного контрола
есть наведение, нажатие, фокус и выключенное состояние; у полей ввода — ещё
состояние проверки `AxValidation.State`.

Уведомление и подсказка-обучение сняты в SDK 6.0: у них не было ни одного
потребителя, и вернутся они добавкой вместе со своей службой. Хлебные крошки
сняты тогда же и вернулись в 7.1 — с первым потребителем, окном проекта студии.

Подпись кнопки или вкладки, которой не хватает ширины, кончается многоточием и
целиком читается в подсказке — только пока она сокращена. Вкладка при этом не
бывает шире своей полосы. Подсказку показывает `AxTrimmedTip`: тема включает его
сама, а на своём контроле его включают свойством `AxTrimmedTip.IsEnabled` вместе
с пустой подсказкой — без неё Avalonia не спрашивает, открывать ли.

Набор растёт по потребностям экранов студии (план M0–M7 — в `docs/plan.md`
главного репозитория ArxisStudio).

## Иконки

Набора здесь нет: контрол `AxIcon` и пути `AxIcons` живут отдельной библиотекой —
[`ArxisStudio.Icons`](../ArxisStudio.Icons/). Ни один контрол этого набора иконки не зовёт:
в шаблоны их ставит тема, в интерфейс — само приложение, и библиотеке контролов набор не
нужен. Значок в шаблонах темы называется тем же адресом разметки, что и контролы.

## Галерея

`samples/Controls.Gallery` — приложение с карточкой на каждый контрол:
светлая и тёмная половины рядом, переключатель прячет одну из них. Наведение, нажатие и проход `Tab` — живые.
Поля инспектора — отдельным файлом карточек, `InspectorFieldCards.axaml`, с маленьким инспектором внизу;
`check-coverage.py` читает все карточки папки:

```bash
dotnet run --project samples/Controls.Gallery
```

Галерея ожидает репозиторий `ArxisStudio.Themes.Arxis` рядом с этим. На ней обе
темы и все состояния видны на одном экране. В Debug она поднимает
[AvaDevTools](https://pavel-zheltiakov.github.io/AvaDevTools/) на порту 5172 — дерево,
стили и снимок любой карточки, — а два корня окна (светлый и тёмный) считает отдельно,
а не одним пропавшим.

## Стек

net8.0 (библиотека) / net10.0 (галерея), Avalonia 12.1.x, центральное управление
версиями пакетов, сборка с 0 предупреждений.
