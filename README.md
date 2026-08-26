# ArxisStudio.Controls

Библиотека контролов ArxisStudio — виджеты, из которых строится интерфейс студии и её
плагинов. Аналог `Avalonia.Controls` в экосистеме Avalonia: плагины ArxisStudio строят
UI **только из Ax\*-контролов** (layout-панели Avalonia — Grid, StackPanel, DockPanel,
Border — разрешены как есть), как в Unity, где редакторный UI строится только из
контролов Unity.

Контролы **lookless**: шаблоны и цвета живут в теме — [`ArxisStudio.Themes.Arxis`](../ArxisStudio.Themes.Arxis/),
репозитории разрабатываются парой и ожидают друг друга рядом (sibling checkout).

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
| `AxTitleBar` | `ContentControl` | полоса заголовка окна: перетаскивание, двойной щелчок, кнопки окна |
| `AxWindowControls` | `TemplatedControl` | свернуть, развернуть, закрыть; на macOS прячется |

Метрики и состояния — по **Int UI**, дизайн-системе IntelliJ: строка 24, контур
фокуса 2px, скругление 4, кнопка 28 высотой. У каждого интерактивного контрола
есть наведение, нажатие, фокус и выключенное состояние; у полей ввода — ещё
классы `error` и `warning`.

Набор растёт по потребностям экранов студии (план M0–M7 — в `docs/plan.md`
главного репозитория ArxisStudio).

## Галерея

`samples/Controls.Gallery` — приложение со всеми контролами в теме Arxis и
переключателем Dark/Light:

```bash
dotnet run --project samples/Controls.Gallery
```

Галерея ожидает репозиторий `ArxisStudio.Themes.Arxis` рядом с этим.

## Стек

net8.0 (библиотека) / net10.0 (галерея), Avalonia 12.1.x, центральное управление
версиями пакетов, сборка с 0 предупреждений.
