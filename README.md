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
    <AxButton Classes="accent" Content="Готово"/>
  </a:StackPanel>
</AxUserControl>
```

Один `xmlns` открывает столько, на сколько сборок ArxisStudio сослался проект.
Родные виджеты Avalonia под этот адрес не попадают намеренно: они пишутся с
префиксом — и видно, чего в наборе `Ax*` ещё нет.

## Состав (M0)

| Контрол | База | Назначение |
|---|---|---|
| `AxButton` | `Button` | классы: `accent`, `ghost`, `icon`, `danger` |
| `AxTextBox` | `TextBox` | однострочное поле ввода |
| `AxSearchField` | `AxTextBox` | поле поиска со значком-лупой |
| `AxCheckBox` | `CheckBox` | флажок 16×16 |
| `AxToggleSwitch` | `ToggleButton` | тумблер 30×17 |
| `AxComboBox` / `AxComboBoxItem` | `ComboBox` | выпадающий список |
| `AxListBox` / `AxListBoxItem` | `ListBox` | список с выделением строк |
| `AxSegmentedControl` / `AxSegmentItem` | `ListBox` | сегментный переключатель (Design/XAML/Split) |
| `AxBadge` | `ContentControl` | бейдж-счётчик |
| `AxChip` | `ContentControl` | чип-метка; классы `accent`, `kbd` |
| `AxCard` | `ContentControl` | карточка-контейнер |
| `AxProgressBar` | `ProgressBar` | тонкий индикатор (4px) |
| `AxAvatar` | `TemplatedControl` | плитка с инициалами; класс `round` |
| `AxIcon` | `TemplatedControl` | контурная иконка 16×16; пути — в `AxIcons` |
| `AxTextArea` | `AxTextBox` | многострочное поле |
| `AxLink` | `Button` | ссылка; состояние «посещённая» |
| `AxRadioButton` | `RadioButton` | выбор одного варианта |
| `AxDivider` | `TemplatedControl` | линия в пиксель, горизонтальная или вертикальная |
| `AxGroupHeader` | `ContentControl` | заголовок секции с линией |
| `AxBanner` | `ContentControl` | сообщение: информация, успех, предупреждение, ошибка |
| `AxTabStrip` / `AxTabItem` | `ListBox` | вкладки документов: значок, метка правок, закрытие |
| `AxTreeView` / `AxTreeViewItem` | `TreeView` | дерево иерархии и файлов |
| `AxSlider` | `Slider` | ползунок значения |
| `AxToolWindow` | `ContentControl` | панель инструментов: шапка с заголовком, вкладками и действиями |
| `AxUserControl` | `UserControl` | корень разметки: с него начинается панель, написанная на `.axaml` |
| `AxTitleBar` | `ContentControl` | полоса заголовка окна: перетаскивание, двойной щелчок, кнопки окна |
| `AxWindowControls` | `TemplatedControl` | свернуть, развернуть, закрыть; на macOS прячется |

## Меню, диалоги, всплывающие

| Контрол | База | Назначение |
|---|---|---|
| `AxSplitButton` | `SplitButton` | кнопка с меню: действие слева, варианты справа |
| `AxDropDownButton` | `DropDownButton` | кнопка выбора с шевроном |
| `AxMenuFlyout` / `AxMenuItem` | `MenuFlyout` / `MenuItem` | контекстное меню: колонка иконок 16, шорткат справа |
| `AxDialog` | `Window` | диалог без системной рамки на тени `AxAbShadow` |
| `AxTeachingTip` | `ContentControl` | подсказка «Понятно» со счётчиком шагов |
| `AxNotificationCard` | `ContentControl` | уведомление: событие, которое исчезает |
| `AxQuickSearch` | `TemplatedControl` | попап поиска: запрос, результаты, подсказки клавиш |
| `AxBreadcrumbBar` / `AxBreadcrumbItem` | `ItemsControl` | хлебные крошки; классы `current`, `error` |
| `AxToolBar` | `TemplatedControl` | главный тулбар: слоты слева, по центру и справа |
| `AxSpinner` | `TemplatedControl` | лоадер 16: оборот за 0.8 с линейно |
| `AxCodeBlock` | `TemplatedControl` | блок кода с подсветкой кистями `AxCode*` |
| `AxDataGrid` | `AxListBox` | таблица-список со строкой заголовков |

Метрики и состояния — по **Int UI**, дизайн-системе IntelliJ: строка 24, контур
фокуса 2px, скругление 4, кнопка 28 высотой. У каждого интерактивного контрола
есть наведение, нажатие, фокус и выключенное состояние; у полей ввода — ещё
классы `error` и `warning`. Единые классы вида у всех: `accent`, `ghost`, `icon`,
`danger`, `compact`.

Набор растёт по потребностям экранов студии (план M0–M7 — в `docs/plan.md`
главного репозитория ArxisStudio).

## Иконки

`AxIcons` — два семейства контурных путей в системе координат 16×16 для `AxIcon`:
действия и объекты интерфейса (раскрытие, правка, поиск, файлы, запуск, статусы,
дизайнер, окно) и вложенный класс `AxIcons.Toolbox` — глифы контролов для палитры
дизайнера форм, где имя глифа равно имени контрола Avalonia. Обводка 1.2, дуги
настоящие, цвет только от `Foreground`: своих цветов у иконки нет. Заливка — у
силуэтов (`Play`, `Stop`, `Pause`, точки меню) свойством `IsFilled`.

## Галерея

`samples/Controls.Gallery` — приложение со всеми карточками страницы «Контролы»
дизайн-проекта: светлая и тёмная половины рядом, переключатель прячет одну из
них. Наведение, нажатие и проход `Tab` — живые:

```bash
dotnet run --project samples/Controls.Gallery
```

Галерея ожидает репозиторий `ArxisStudio.Themes.Arxis` рядом с этим. По ней
сверяются с `design/20 Controls.dc.html` дизайн-проекта — на одном экране.

## Стек

net8.0 (библиотека) / net10.0 (галерея), Avalonia 12.1.x, центральное управление
версиями пакетов, сборка с 0 предупреждений.
