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
| размер | `Size` у кнопки, переключателя, поля и списка | `Normal`, `Compact` |
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
| `AxListBox` / `AxListBoxItem` | `ListBox` | список с выделением строк |
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
| `AxSplitter` | `GridSplitter` | граница областей: та же линия, но за неё можно взяться мышью |
| `AxGroupHeader` | `ContentControl` | заголовок секции с линией |
| `AxBanner` | `ContentControl` | сообщение: информация, успех, предупреждение, ошибка |
| `AxTabStrip` / `AxTabItem` | `ListBox` | вкладки документов и панелей (`Kind`): значок, метка правок, закрытие |
| `AxTreeView` / `AxTreeViewItem` | `TreeView` | дерево иерархии и файлов; значок — путь `Icon` и цвет `IconBrush` |
| `AxBreadcrumb` / `AxBreadcrumbItem` | `ItemsControl` / `Button` | путь от корня до текущего места: сегмент — переход (`Navigated`), последний — текущий, ведущие при нехватке места уходят в меню |
| `AxSlider` | `Slider` | ползунок значения |
| `AxToolWindow` | `ContentControl` | панель инструментов: шапка с заголовком, вкладками и действиями |
| `AxUserControl` | `UserControl` | корень разметки: с него начинается панель, написанная на `.axaml` |
| `AxTitleBar` | `ContentControl` | полоса заголовка окна: перетаскивание, двойной щелчок, кнопки окна |
| `AxWindowControls` | `TemplatedControl` | свернуть, развернуть, закрыть; на macOS прячется |
| `AxWindow` | `Window` | окно студии: своя полоса заголовка при системной рамке, покрашенной в цвет темы |

## Присоединённые свойства

Не контролы, а свойства, которые вешают на чужие: тема видит их псевдоклассами.

| Класс | Свойства | Назначение |
|---|---|---|
| `AxText` | `Role`, `Tone`, `LineHeightRatio` | роль, тон и высота строки у `TextBlock` |
| `AxValidation` | `State` | что проверка сказала о значении поля, флажка или списка |
| `AxToolTip` | `Gesture` | жест команды рядом с подсказкой; наследуемое — подсказку рисует отдельное окно |
| `AxTrimmedTip` | `IsEnabled` | подсказка с полным текстом у подписи, сокращённой многоточием |
| `AxSelectionScope` | `IsActive` | область выделения: строки внутри неё горят полным цветом, пока клавиатура в ней |

## Меню, диалоги, всплывающие

| Контрол | База | Назначение |
|---|---|---|
| `AxSplitButton` | `SplitButton` | кнопка с меню: действие слева, варианты справа |
| `AxDropDownButton` | `DropDownButton` | кнопка выбора с шевроном |
| `AxMenuFlyout` / `AxMenuItem` | `MenuFlyout` / `MenuItem` | контекстное меню: колонка иконок 16, шорткат справа |
| `AxDialog` | `Window` | диалог без системной рамки на тени `AxShadowModal` |
| `AxQuickSearch` | `TemplatedControl` | попап поиска: запрос, результаты, подсказки клавиш |
| `AxToolBar` | `TemplatedControl` | главный тулбар: слоты слева, по центру и справа |
| `AxSpinner` | `TemplatedControl` | лоадер 16 или крупный (`Size`): оборот за 0.8 с линейно |
| `AxCodeBlock` | `TemplatedControl` | блок кода с подсветкой кистями `AxCode*` |
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
светлая и тёмная половины рядом, переключатель прячет одну из них. Наведение, нажатие и проход `Tab` — живые:

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
