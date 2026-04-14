# 04 - 控件体系（深度分析）

## 控件总览

AtomUI 共包含 **77 个控件目录**，分布在 `AtomUI.Controls`、`AtomUI.Desktop.Controls`、`AtomUI.Desktop.Controls.ColorPicker`、`AtomUI.Desktop.Controls.DataGrid` 四个项目中。

### 控件完整清单

| # | 控件名 | 目录 | CS文件数 | AXAML文件数 | 控件基类 | Token基类 | 复杂度 |
|---|--------|------|----------|-------------|----------|-----------|--------|
| 1 | **AdornerLayer** | AdornerLayer | 1 | 1 | — | AbstractControlDesignToken | ⭐ |
| 2 | **Alert** | Alert | 3 | 1 | TemplatedControl | AbstractControlDesignToken | ⭐⭐ |
| 3 | **AutoComplete** | AutoComplete | 20 | 6 | TemplatedControl | AbstractControlDesignToken | ⭐⭐⭐⭐⭐ |
| 4 | **Avatar** | Avatar | 3 | 3 | AbstractAvatar | AbstractControlDesignToken | ⭐⭐ |
| 5 | **AvatarGroup** | Avatar | — | — | — | (共享AvatarToken) | ⭐⭐ |
| 6 | **CountBadge** | Badge | 7 | 5 | AbstractCountBadge | AbstractControlDesignToken | ⭐⭐⭐ |
| 7 | **DotBadge** | Badge | — | — | AbstractDotBadge | (共享BadgeToken) | ⭐⭐ |
| 8 | **RibbonBadge** | Badge | — | — | AbstractRibbonBadge | (共享BadgeToken) | ⭐⭐ |
| 9 | **HBoxPanel** | BoxPanel | 6 | 0 | BoxPanel | N/A | ⭐⭐ |
| 10 | **VBoxPanel** | BoxPanel | — | — | BoxPanel | N/A | ⭐⭐ |
| 11 | **Breadcrumb** | Breadcrumb | 7 | 3 | — | AbstractControlDesignToken | ⭐⭐⭐ |
| 12 | **Button** | Buttons | 15 | 7 | ContentControl (Avalonia.Button) | AbstractControlDesignToken | ⭐⭐⭐ |
| 13 | **IconButton** | Buttons | — | — | AbstractIconButton | (共享ButtonToken) | ⭐⭐⭐ |
| 14 | **HyperLinkButton** | Buttons | — | — | Avalonia.Button | (共享ButtonToken) | ⭐⭐ |
| 15 | **DropdownButton** | Buttons | — | — | Avalonia.Button | (共享ButtonToken) | ⭐⭐⭐ |
| 16 | **SplitButton** | Buttons | — | — | Avalonia.Button | (共享ButtonToken) | ⭐⭐⭐ |
| 17 | **ToggleIconButton** | Buttons | — | — | AbstractIconButton | (共享ButtonToken) | ⭐⭐⭐ |
| 18 | **ButtonSpinner** | ButtonSpinner | 7 | 4 | Spinner | LineEditToken | ⭐⭐⭐ |
| 19 | **Calendar** | Calendar | 14 | 6 | Avalonia.Button | AbstractControlDesignToken | ⭐⭐⭐⭐ |
| 20 | **Card** | Card | 10 | 8 | ItemsControl | AbstractControlDesignToken | ⭐⭐⭐⭐ |
| 21 | **Carousel** | Carousel | 9 | 6 | IconButton | AbstractControlDesignToken | ⭐⭐⭐⭐ |
| 22 | **Cascader** | Cascader | 33 | 7 | — | AbstractControlDesignToken | ⭐⭐⭐⭐⭐ |
| 23 | **CheckBox** | CheckBox | 3 | 5 | AbstractCheckBoxGroup | AbstractControlDesignToken | ⭐⭐⭐ |
| 24 | **CheckBoxGroup** | CheckBox | — | — | — | (共享CheckBoxToken) | ⭐⭐ |
| 25 | **WindowTitleBar** | Chrome | 10 | 5 | TemplatedControl | AbstractControlDesignToken | ⭐⭐⭐ |
| 26 | **CaptionButton** | Chrome | — | — | TemplatedControl | (共享ChromeToken) | ⭐⭐ |
| 27 | **Collapse** | Collapse | 6 | 3 | SelectingItemsControl | AbstractControlDesignToken | ⭐⭐⭐ |
| 28 | **ComboBox** | ComboBox | 6 | 4 | Avalonia.ComboBox | ButtonSpinnerToken | ⭐⭐⭐⭐ |
| 29 | **DatePicker** | DatePicker | 26 | 13 | DatePickerFlyout | AbstractControlDesignToken | ⭐⭐⭐⭐⭐ |
| 30 | **RangeDatePicker** | DatePicker | — | — | — | (共享DatePickerToken) | ⭐⭐⭐⭐⭐ |
| 31 | **Descriptions** | Descriptions | 9 | 5 | — | AbstractControlDesignToken | ⭐⭐⭐ |
| 32 | **Dialog** | Dialog | 41 | 9 | — | AbstractControlDesignToken | ⭐⭐⭐⭐⭐ |
| 33 | **Drawer** | Drawer | 6 | 3 | ContentControl | AbstractControlDesignToken | ⭐⭐⭐ |
| 34 | **Empty** | Empty | 2 | 1 | AbstractEmpty | AbstractControlDesignToken | ⭐ |
| 35 | **Expander** | Expander | 4 | 1 | Avalonia.Expander | AbstractControlDesignToken | ⭐⭐⭐ |
| 36 | **FlexPanel** | FlexPanel | 10 | 0 | — | N/A | ⭐⭐⭐ |
| 37 | **FloatButton** | FloatButton | 10 | 8 | TemplatedControl | AbstractControlDesignToken | ⭐⭐⭐ |
| 38 | **Flyout** | Flyouts | 14 | 4 | Avalonia.Flyout | AbstractControlDesignToken | ⭐⭐⭐⭐ |
| 39 | **Form** | Form | 8 | 7 | TemplatedControl | AbstractControlDesignToken | ⭐⭐⭐⭐ |
| 40 | **Grid** | Grid | 9 | 0 | TypeConverter | N/A | ⭐⭐ |
| 41 | **GroupBox** | GroupBox | 3 | 1 | ContentControl | AbstractControlDesignToken | ⭐⭐ |
| 42 | **HeaderedContentControl** | HeaderedContentControl | 1 | 1 | — | N/A | ⭐ |
| 43 | **ImagePreviewer** | ImagePreviewer | 17 | 9 | ContentControl | AbstractControlDesignToken | ⭐⭐⭐⭐ |
| 44 | **LineEdit** | Input | 21 | 11 | TextBox | AbstractControlDesignToken | ⭐⭐⭐⭐⭐ |
| 45 | **TextArea** | Input | — | — | TextBox | (共享InputToken) | ⭐⭐⭐⭐ |
| 46 | **SearchEdit** | Input | — | — | — | (共享InputToken) | ⭐⭐⭐ |
| 47 | **ListBox** | ListBox | 8 | 3 | — | AbstractControlDesignToken | ⭐⭐⭐ |
| 48 | **ListView** | ListView | 15 | 3 | ContentControl | AbstractControlDesignToken | ⭐⭐⭐⭐ |
| 49 | **MarqueeLabel** | MarqueeLabel | 2 | 0 | AbstractMarqueeLabel | AbstractControlDesignToken | ⭐⭐ |
| 50 | **Mentions** | Mentions | 16 | 3 | — | AbstractControlDesignToken | ⭐⭐⭐⭐ |
| 51 | **Menu** | Menu | 13 | 6 | IMenuItemData | AbstractControlDesignToken | ⭐⭐⭐⭐ |
| 52 | **Message** | Message | 9 | 3 | — | AbstractControlDesignToken | ⭐⭐⭐⭐ |
| 53 | **MessageBox** | MessageBox | 10 | 4 | IMessageBoxActionResult | AbstractControlDesignToken | ⭐⭐⭐⭐ |
| 54 | **NavMenu** | NavMenu | 23 | 7 | — | AbstractControlDesignToken | ⭐⭐⭐⭐⭐ |
| 55 | **Notification** | Notifications | 13 | 4 | ContentControl | AbstractControlDesignToken | ⭐⭐⭐⭐ |
| 56 | **NumericUpDown** | NumericUpDown | 3 | 1 | Avalonia.NumericUpDown | ButtonSpinnerToken | ⭐⭐⭐ |
| 57 | **OptionButtonGroup** | OptionButtonGroup | 3 | 3 | AbstractOptionButtonGroup | AbstractControlDesignToken | ⭐⭐⭐ |
| 58 | **Pagination** | Pagination | 13 | 6 | SelectingItemsControl | AbstractControlDesignToken | ⭐⭐⭐⭐ |
| 59 | **Popup** | Popup | 12 | 6 | — | AbstractControlDesignToken | ⭐⭐⭐⭐ |
| 60 | **PopupConfirm** | PopupConfirm | 6 | 3 | FlyoutHost | AbstractControlDesignToken | ⭐⭐⭐ |
| 61 | **Primitives** | Primitives | 31 | 18 | Canvas | AbstractControlDesignToken | ⭐⭐⭐⭐⭐ |
| 62 | **ProgressBar** | ProgressBar | 8 | 8 | AbstractProgressBar | AbstractControlDesignToken | ⭐⭐⭐⭐ |
| 63 | **QRCode** | QRCode | 4 | 1 | AbstractQRCode | AbstractControlDesignToken | ⭐⭐⭐ |
| 64 | **RadioButton** | RadioButton | 3 | 4 | AbstractRadioButtonGroup | AbstractControlDesignToken | ⭐⭐⭐ |
| 65 | **Rate** | Rate | 2 | 4 | AbstractRate | AbstractControlDesignToken | ⭐⭐⭐ |
| 66 | **Result** | Result | 2 | 1 | AbstractResult | AbstractControlDesignToken | ⭐⭐ |
| 67 | **ScrollViewer** | ScrollViewer | 6 | 7 | AbstractScrollBarThumb | AbstractControlDesignToken | ⭐⭐⭐ |
| 68 | **Segmented** | Segmented | 3 | 3 | AbstractSegmented | AbstractControlDesignToken | ⭐⭐⭐ |
| 69 | **Select** | Select | 26 | 11 | — | AbstractControlDesignToken | ⭐⭐⭐⭐⭐ |
| 70 | **Separator** | Separator | 2 | 1 | AbstractSeparator | AbstractControlDesignToken | ⭐ |
| 71 | **Skeleton** | Skeleton | 16 | 11 | AbstractSkeleton | AbstractControlDesignToken | ⭐⭐⭐⭐ |
| 72 | **Slider** | Slider | 7 | 4 | TemplatedControl | AbstractControlDesignToken | ⭐⭐⭐ |
| 73 | **Space** | Space | 10 | 3 | — | AbstractControlDesignToken | ⭐⭐⭐ |
| 74 | **Spin** | Spin | 3 | 3 | AbstractSpin | AbstractControlDesignToken | ⭐⭐ |
| 75 | **SplitView** | SplitView | 2 | 1 | Avalonia.SplitView | AbstractControlDesignToken | ⭐⭐ |
| 76 | **Splitter** | Splitter | 7 | 3 | TemplatedControl | AbstractControlDesignToken | ⭐⭐⭐ |
| 77 | **Statistic** | Statistic | 7 | 5 | AbstractStatistic | AbstractControlDesignToken | ⭐⭐⭐ |
| 78 | **Steps** | Steps | 6 | 4 | TemplatedControl | AbstractControlDesignToken | ⭐⭐⭐ |
| 79 | **ToggleSwitch** | Switch | 2 | 3 | AbstractToggleSwitch | AbstractControlDesignToken | ⭐⭐⭐ |
| 80 | **TabControl** | TabControl | 25 | 15 | BaseOverflowMenuItem | AbstractControlDesignToken | ⭐⭐⭐⭐⭐ |
| 81 | **Tag** | Tag | 3 | 2 | AbstractTag | AbstractControlDesignToken | ⭐⭐⭐ |
| 82 | **TextBlock** | TextBlock | 6 | 1 | Avalonia.TextBlock | N/A | ⭐⭐ |
| 83 | **TimePicker** | TimePicker | 11 | 6 | Flyout | AbstractControlDesignToken | ⭐⭐⭐⭐ |
| 84 | **Timeline** | Timeline | 4 | 4 | AbstractTimelineItem | AbstractControlDesignToken | ⭐⭐⭐ |
| 85 | **ToolTip** | Tooltip | 4 | 1 | ContentControl | AbstractControlDesignToken | ⭐⭐ |
| 86 | **Tour** | Tour | 14 | 6 | TourIndicator | AbstractControlDesignToken | ⭐⭐⭐⭐ |
| 87 | **Transfer** | Transfer | 25 | 11 | TreeView | AbstractControlDesignToken | ⭐⭐⭐⭐⭐ |
| 88 | **TreeSelect** | TreeSelect | 8 | 4 | TreeView | AbstractControlDesignToken | ⭐⭐⭐⭐ |
| 89 | **TreeView** | TreeView | 33 | 5 | ToggleButton | AbstractControlDesignToken | ⭐⭐⭐⭐⭐ |
| 90 | **Upload** | Upload | 30 | 19 | — | AbstractControlDesignToken | ⭐⭐⭐⭐⭐ |
| 91 | **Window** | Window | 10 | 3 | — | AbstractControlDesignToken | ⭐⭐⭐⭐ |
| 92 | **ColorPicker** | ColorPicker | — | — | AbstractColorPicker | AbstractControlDesignToken | ⭐⭐⭐⭐⭐ |
| 93 | **DataGrid** | DataGrid | — | — | — | — | ⭐⭐⭐⭐ |

---

## 按功能分类

### 通用按钮族（Buttons/）

| 控件 | 基类 | 关键属性 | 说明 |
|------|------|----------|------|
| **Button** | `Avalonia.Button` | ButtonType, SizeType, IsDanger, IsGhost, IsLoading, Icon, IsIconVisible, Shape | 主按钮，支持 Default/Primary/Dashed/Link/Text 五种类型 |
| **IconButton** | `AbstractIconButton → Avalonia.Button` | Icon, IconSize, IsIconCircle, IsDanger | 纯图标按钮 |
| **HyperLinkButton** | `Avalonia.Button` | NavigateUri, IsDanger | 超链接按钮 |
| **DropdownButton** | `Avalonia.Button` | (继承Button属性) + Flyout | 带下拉菜单的按钮 |
| **SplitButton** | `Avalonia.Button` | (继承Button属性) + Flyout | 分割按钮（主操作+下拉） |
| **ToggleIconButton** | `AbstractIconButton → Avalonia.Button` | IsChecked | 可切换的图标按钮 |

**Button 枚举类型：**
```csharp
public enum ButtonType { Default, Primary, Dashed, Link, Text }
public enum SizeType { Small, Middle, Large }
public enum Shape { Default, Circle, Round }
```

**Button 伪类：** `:danger`, `:loading`, `:icononly`, `:ghost`

### 输入控件族（Input/）

| 控件 | 基类 | 关键属性 | 说明 |
|------|------|----------|------|
| **LineEdit** | `TextBox` | SizeType, StyleVariant, Status, LeftAddOn, RightAddOn, ContentLeftAddOn, ContentRightAddOn, IsShowClearButton, IsShowCharacterCount, MaxCharacterCount | 单行输入框 |
| **TextArea** | `TextBox` | (继承LineEdit属性) + AutoSize | 多行文本框 |
| **SearchEdit** | `TextBox` | SearchButton | 搜索输入框 |
| **NumericUpDown** | `Avalonia.NumericUpDown → ButtonSpinnerToken` | Format, Increment, Minimum, Maximum | 数值调节器 |

**Input 枚举类型：**
```csharp
public enum InputControlStyleVariant { Filled, Outlined, Borderless }
public enum InputControlStatus { Normal, Warning, Error }
```

**Input 关键接口：**
- `IInputControlStatusAware` — 状态感知
- `IInputControlStyleVariantAware` — 样式变体感知
- `ISizeTypeAware` — 尺寸感知
- `ICompactSpaceAware` — 紧凑空间感知

### 选择控件族

| 控件 | 基类 | 关键属性 | 说明 |
|------|------|----------|------|
| **Select** | — | ItemsSource, SelectedItem, SelectedValue, IsMultiSelect, IsShowSearch, MaxSelectCount, SizeType, StyleVariant | 下拉选择器 |
| **ComboBox** | `Avalonia.ComboBox → ButtonSpinnerToken` | LeftAddOn, RightAddOn, SizeType, StyleVariant | 组合框 |
| **Cascader** | — | ItemsSource, SelectedValue, IsMultipleSelect, IsShowSearch, SizeType, StyleVariant, ItemDataLoader | 级联选择器 |
| **TreeSelect** | `TreeView` | ItemsSource, SelectedValue, CheckedStrategy, IsMultipleSelect, SizeType, StyleVariant | 树选择器 |
| **AutoComplete** | `TemplatedControl` | Text, ItemsSource, IsShowSearch, CompleteOptionLoader | 自动完成 |
| **Mentions** | — | ItemsSource, Prefix, Split, Position | @提及控件 |
| **CheckBox** | `AbstractCheckBoxGroup` | IsChecked, IsIndeterminate, SizeType | 复选框 |
| **RadioButton** | `AbstractRadioButtonGroup` | IsChecked, SizeType | 单选按钮 |
| **OptionButtonGroup** | `AbstractOptionButtonGroup` | SelectedIndex, SizeType | 选项按钮组 |
| **Segmented** | `AbstractSegmented → SelectingItemsControl` | SelectedIndex, SelectedItem, SizeType | 分段控制器 |
| **Switch/ToggleSwitch** | `AbstractToggleSwitch` | IsChecked, SizeType, IsMotionEnabled | 开关 |

### 日期时间控件族（DatePicker/, TimePicker/）

| 控件 | 基类 | 关键属性 | 说明 |
|------|------|----------|------|
| **DatePicker** | `DatePickerFlyout` | SelectedDateTime, Format, IsShowTime, IsNeedConfirm, IsShowNow, ClockIdentifier, SizeType | 日期选择器 |
| **RangeDatePicker** | — | RangeStartSelectedDate, RangeEndSelectedDate, IsShowTime, Format | 范围日期选择器 |
| **TimePicker** | `Flyout` | SelectedTime, Format, ClockIdentifier, SizeType | 时间选择器 |
| **RangeTimePicker** | — | (继承TimePicker属性) | 范围时间选择器 |
| **Calendar** | `Avalonia.Button` | SelectedDate, DisplayMode, SelectionMode, FirstDayOfWeek, IsTodayHighlighted, DisplayDate, DisplayDateStart, DisplayDateEnd | 日历控件 |

**DatePicker 内部组件：**
- `CalendarItem` — 日历项
- `CalendarDayButton` — 日历日按钮
- `CalendarButton` — 日历月/年按钮
- `DualMonthCalendarItem` — 双月日历项
- `RangeCalendar` / `RangeCalendarItem` — 范围日历
- `DatePickerPresenter` — 日期选择器展示器
- `RangeDatePickerPresenter` — 范围日期选择器展示器

### 数据展示控件族

| 控件 | 基类 | 关键属性 | 说明 |
|------|------|----------|------|
| **Card** | `ItemsControl` | StyleVariant, SizeType, IsLoading, IsInnerMode, IsHoverable, Cover, Avatar, Title, Extra | 卡片 |
| **Collapse** | `SelectingItemsControl` | IsAccordion, IsGhostStyle, IsBorderless, TriggerType, ExpandIconPosition, SizeType | 折叠面板 |
| **Expander** | `Avalonia.Expander` | ExpandDirection, SizeType | 展开器 |
| **Descriptions** | — | ItemsSource, ColumnCount, IsBordered, SizeType | 描述列表 |
| **Statistic** | `AbstractStatistic → HeaderedContentControl` | Value, Title, Prefix, Suffix, Precision, GroupSeparator | 统计数值 |
| **TimerStatistic** | `AbstractStatistic` | (继承) + IsCountdown, StartTime, Format | 倒计时统计 |
| **Tag** | `AbstractTag → TemplatedControl` | IsClosable, Color, IsCheckable, IsChecked | 标签 |
| **Badge** | 多种基类 | Count, IsDot, IsRibbon, Text, Status, Color | 徽标 |
| **Avatar** | `AbstractAvatar → TemplatedControl` | Source, Initials, SizeType, Shape | 头像 |
| **AvatarGroup** | — | MaxCount, SizeType | 头像组 |
| **Tooltip** | `ContentControl` | Content, Placement, IsOpen | 工具提示 |
| **Timeline** | `AbstractTimeline → ItemsControl` | ItemsSource, Mode | 时间线 |
| **Carousel** | `IconButton` | ItemsSource, AutoPlay, Interval, IsLoop, Effect | 走马灯 |
| **ImagePreviewer** | `ContentControl` | Images, CurrentIndex, IsVisible | 图片预览 |
| **QRCode** | `AbstractQRCode` | Value, Size, Color, IsShowIcon | 二维码 |
| **Empty** | `AbstractEmpty → TemplatedControl` | Description, Image, IsSimple | 空状态 |
| **Result** | `AbstractResult → ContentControl` | Status, Title, SubTitle, Icon, Extra | 结果页 |
| **Skeleton** | `AbstractSkeleton → TemplatedControl` | IsLoading, IsRound, Active, Avatar, Title, Paragraph | 骨架屏 |
| **ProgressBar** | `AbstractProgressBar → RangeBase` | IsShowText, StrokeWidth, Status, SizeType, IsMotionEnabled | 进度条 |
| **ListView** | `ContentControl` | ItemsSource, SelectedItem, IsMultiSelect | 列表视图 |
| **ListBox** | — | ItemsSource, SelectedItem, SelectionMode | 列表框 |

### 导航控件族

| 控件 | 基类 | 关键属性 | 说明 |
|------|------|----------|------|
| **NavMenu** | — | ItemsSource, SelectedItem, Mode, IsCollapsed, IsInlineCollapsed, OpenKeys, SelectedKey | 导航菜单 |
| **Menu** | `IMenuItemData` | ItemsSource, IsContextMenu | 菜单 |
| **Breadcrumb** | — | ItemsSource, Separator, IsAutoCollapsed | 面包屑 |
| **Pagination** | `AbstractPagination → SelectingItemsControl` | CurrentPage, TotalCount, PageSize, IsShowQuickJumper, SizeType | 分页 |
| **Steps** | `TemplatedControl` | ItemsSource, Current, Status, SizeType, Direction | 步骤条 |
| **Tour** | `TourIndicator` | Steps, IsOpen, CurrentStep, Placement | 漫游引导 |
| **TabControl** | `BaseTabControl` | ItemsSource, SelectedIndex, TabStripPlacement, SizeType | 标签页 |
| **TabItem** | — | Header, IsSelected, IsClosable | 标签页项 |

### 反馈控件族

| 控件 | 基类 | 关键属性 | 说明 |
|------|------|----------|------|
| **Dialog** | — | Title, Content, IsOpen, IsModal, IsResizable, IsClosable, IsMaximizable, IsMotionEnabled, StandardButtons | 对话框 |
| **Drawer** | `ContentControl` | Content, Placement, IsOpen, IsClosable, SizeType | 抽屉 |
| **MessageBox** | `IMessageBoxActionResult` | Title, Content, MessageSeverity, StandardButtons | 消息框 |
| **PopupConfirm** | `FlyoutHost` | Content, IsOpen, Placement | 确认弹出 |
| **Alert** | `TemplatedControl` | Type, IsShowIcon, IsClosable, Message, Description, ExtraAction | 警告提示 |
| **Message** | — | Content, Type, Duration, IsClosable | 全局消息 |
| **Notification** | `ContentControl` | Title, Content, Type, Duration, Placement, IsClosable | 通知提醒 |
| **Spin** | `AbstractSpin → ContentControl` | IsSpinning, SizeType, Indicator | 加载中 |
| **Popup** | — | Child, IsOpen, Placement, IsShowArrow | 弹出层 |
| **Flyout** | `Avalonia.Flyout` | Content, Placement, IsOpen | 浮出层 |

### 布局控件族

| 控件 | 基类 | 关键属性 | 说明 |
|------|------|----------|------|
| **Space** | — | Direction, SizeType, Align, IsWrap | 间距 |
| **CompactSpace** | — | Direction, SizeType | 紧凑空间 |
| **Splitter** | `TemplatedControl` | Orientation, Panel1Size, Panel2Size, IsCollapsible | 分割器 |
| **SplitView** | `Avalonia.SplitView` | IsPaneOpen, DisplayMode, PanePlacement | 分割视图 |
| **GroupBox** | `ContentControl` | Header, IsCollapsible | 分组框 |
| **ScrollViewer** | `AbstractScrollViewer → Avalonia.ScrollViewer` | HorizontalScrollBarVisibility, VerticalScrollBarVisibility | 滚动视图 |
| **Separator** | `AbstractSeparator → Avalonia.Separator` | Orientation, SizeType | 分隔线 |
| **HBoxPanel** | `BoxPanel` | Spacing, Alignment | 水平盒布局 |
| **VBoxPanel** | `BoxPanel` | Spacing, Alignment | 垂直盒布局 |
| **FlexPanel** | — | Direction, Wrap, JustifyContent, AlignItems, AlignContent, Gap | 弹性布局 |
| **Grid** | `TypeConverter` | RowDefinitions, ColumnDefinitions | 网格布局 |

### 窗口相关控件

| 控件 | 基类 | 关键属性 | 说明 |
|------|------|----------|------|
| **Window** | — | TitleBarMode, IsCustomTitleBar, WindowStartupLocation | 窗口 |
| **ReactiveWindow** | — | (响应式窗口) | 响应式窗口基类 |
| **WindowTitleBar** | `TemplatedControl` | Title, IsCloseButtonEnabled, IsMaximizeButtonEnabled, IsMinimizeButtonEnabled | 标题栏 |
| **CaptionButton** | `TemplatedControl` | — | 窗口标题按钮 |

### 数据录入高级控件

| 控件 | 基类 | 关键属性 | 说明 |
|------|------|----------|------|
| **Upload** | — | Action, Accept, IsMultiple, ListType, MaxCount, IsDirectory, IsDragger | 上传 |
| **Transfer** | `AbstractTransfer → TemplatedControl` | ItemsSource, SelectedItems, IsShowSearch, Titles, ShowSelectAll | 穿梭框 |
| **TreeTransfer** | `AbstractTransfer` | (继承) + TreeView | 树穿梭框 |
| **Form** | `TemplatedControl` | Model, IsShowRequiredMark, LabelCol, WrapperCol, SizeType | 表单 |
| **Rate** | `AbstractRate` | Value, Count, IsAllowHalf, IsDisabled, Character | 评分 |
| **Slider** | `TemplatedControl` | Minimum, Maximum, Value, IsShowTooltip, IsRange, Step | 滑动输入条 |

### 特殊控件

| 控件 | 基类 | 关键属性 | 说明 |
|------|------|----------|------|
| **FloatButton** | `AbstractFloatButton → Avalonia.Button` | Icon, Description, Type, IsBackTop | 悬浮按钮 |
| **MarqueeLabel** | `AbstractMarqueeLabel → TextBlock` | Text, Speed, Direction | 跑马灯 |
| **ColorPicker** | `AbstractColorPicker → Avalonia.Button` | SelectedColor, IsShowAlpha, IsShowPreset | 颜色选择器 |
| **DataGrid** | — | ItemsSource, Columns, SelectedItem | 数据表格 |

---

## 抽象基类体系

AtomUI 大量使用抽象基类模式，将跨平台共享的定义放在 `AtomUI.Controls` 项目中，桌面实现在 `AtomUI.Desktop.Controls` 中。

### 完整抽象基类清单

| 抽象基类 | 所在项目 | 继承自 | 实现控件 | 关键接口 |
|----------|----------|--------|----------|----------|
| `AbstractAvatar` | Controls | TemplatedControl | Avatar | IMotionAwareControl |
| `AbstractCountBadge` | Controls | Control | CountBadge | IMotionAwareControl |
| `AbstractCountBadgeAdorner` | Controls | TemplatedControl | CountBadgeAdorner | — |
| `AbstractDotBadge` | Controls | Control | DotBadge | IMotionAwareControl |
| `AbstractDotBadgeAdorner` | Controls | TemplatedControl | DotBadgeAdorner | — |
| `AbstractRibbonBadge` | Controls | Control | RibbonBadge | — |
| `AbstractRibbonBadgeAdorner` | Controls | TemplatedControl | RibbonBadgeAdorner | — |
| `AbstractIconButton` | Controls | Avalonia.Button | IconButton, ToggleIconButton | IMotionAwareControl |
| `AbstractEmpty` | Controls | TemplatedControl | Empty | ISizeTypeAware |
| `AbstractFloatButton` | Controls | Avalonia.Button | FloatButton | IMotionAwareControl |
| `AbstractFloatButtonHost` | Controls | TemplatedControl | FloatButtonHost | IMotionAwareControl |
| `AbstractBackTopFloatButton` | Controls | AbstractFloatButton | BackTopFloatButton | — |
| `AbstractBackTopFloatButtonHost` | Controls | AbstractFloatButtonHost | BackTopFloatButtonHost | — |
| `AbstractMarqueeLabel` | Controls | TextBlock | MarqueeLabel | — |
| `AbstractProgressBar` | Controls | RangeBase | ProgressBar | ISizeTypeAware, IMotionAwareControl |
| `AbstractCircleProgress` | Controls | AbstractProgressBar | CircleProgress | — |
| `AbstractLineProgress` | Controls | AbstractProgressBar | LineProgress | — |
| `AbstractResult` | Controls | ContentControl | Result | — |
| `AbstractScrollBar` | Controls | Avalonia.ScrollBar | ScrollBar | IMotionAwareControl |
| `AbstractScrollBarThumb` | Controls | Thumb | ScrollBarThumb | IMotionAwareControl |
| `AbstractScrollViewer` | Controls | Avalonia.ScrollViewer | ScrollViewer | IMotionAwareControl |
| `AbstractSegmented` | Controls | SelectingItemsControl | Segmented | ISizeTypeAware, IMotionAwareControl |
| `AbstractSegmentedItem` | Controls | ContentControl | SegmentedItem | ISelectable |
| `AbstractSeparator` | Controls | Avalonia.Separator | Separator | ISizeTypeAware |
| `AbstractSpin` | Controls | ContentControl | Spin | IMotionAwareControl |
| `AbstractSpinIndicator` | Controls | TemplatedControl | SpinIndicator | ISizeTypeAware |
| `AbstractTag` | Controls | TemplatedControl | Tag | — |
| `AbstractTimeline` | Controls | ItemsControl | Timeline | — |
| `AbstractTimelineItem` | Controls | ContentControl | TimelineItem | — |
| `AbstractArrowDecoratedBox` | Controls | ContentControl | ArrowDecoratedBox | — |
| `AbstractPagination` | Desktop.Controls | TemplatedControl | Pagination | ISizeTypeAware, IMotionAwareControl |
| `AbstractSkeleton` | Desktop.Controls | TemplatedControl | Skeleton | — |
| `AbstractStatistic` | Desktop.Controls | HeaderedContentControl | Statistic, TimerStatistic | — |
| `AbstractTransfer` | Desktop.Controls | TemplatedControl | ListTransfer, TreeTransfer | — |
| `AbstractColorPicker` | Desktop.Controls.ColorPicker | Avalonia.Button | ColorPicker | — |
| `AbstractColorPickerFlyout` | Desktop.Controls.ColorPicker | Flyout | ColorPickerFlyout | — |
| `AbstractColorSlider` | Desktop.Controls.ColorPicker | RangeBase | ColorSlider | — |
| `AbstractColorPickerSliderTrack` | Desktop.Controls.ColorPicker | TemplatedControl | ColorPickerSliderTrack | — |
| `AbstractColorPickerView` | Desktop.Controls.ColorPicker | TemplatedControl | ColorPickerView | IMotionAwareControl |
| `AbstractFormValidator` | Controls | IFormValidator | (自定义验证器) | IFormValidator |

### 关键感知接口

| 接口 | 所在项目 | 作用 | 使用控件 |
|------|----------|------|----------|
| `IMotionAwareControl` | Controls | 标记控件支持动画 | Avatar, Badge, Button, FloatButton, ProgressBar, ScrollBar, Segmented, Spin, ColorPickerView 等 |
| `ISizeTypeAware` | Controls.Shared | 标记控件支持尺寸变体 | Empty, Separator, SpinIndicator, Pagination, AbstractPagination |
| `IInputControlStatusAware` | Controls.Shared | 标记输入控件支持状态 | LineEdit, TextArea, Select, ComboBox 等 |
| `IInputControlStyleVariantAware` | Controls.Shared | 标记输入控件支持样式变体 | LineEdit, TextArea, Select, ComboBox 等 |
| `IOperationSystemAware` | Controls.Shared | 标记控件感知操作系统 | — |
| `IMediaBreakAware` | Controls.Shared | 标记控件响应媒体断点 | — |
| `ICompactSpaceAware` | Controls.Shared | 标记控件支持紧凑空间 | LineEdit, Select, AutoComplete 等 |
| `IWaveSpiritAware` | Controls.Shared | 标记控件支持水波纹 | — |
| `ISelectable` | Avalonia | 标记可选择 | SegmentedItem |

---

## Token 继承体系

### Token 基类层次

```mermaid
graph TB
    IDesignToken["IDesignToken (接口)"]
    AbstractDesignToken["AbstractDesignToken"]
    AbstractControlDesignToken["AbstractControlDesignToken"]
    
    IDesignToken --> AbstractDesignToken
    AbstractDesignToken --> AbstractControlDesignToken
    
    AbstractControlDesignToken --> AlertToken
    AbstractControlDesignToken --> AutoCompleteToken
    AbstractControlDesignToken --> AvatarToken
    AbstractControlDesignToken --> BadgeToken
    AbstractControlDesignToken --> BreadcrumbToken
    AbstractControlDesignToken --> ButtonToken
    AbstractControlDesignToken --> CalendarToken
    AbstractControlDesignToken --> CardToken
    AbstractControlDesignToken --> CascaderToken
    AbstractControlDesignToken --> CheckBoxToken
    AbstractControlDesignToken --> ChromeToken
    AbstractControlDesignToken --> CollapseToken
    AbstractControlDesignToken --> DatePickerToken
    AbstractControlDesignToken --> DescriptionsToken
    AbstractControlDesignToken --> DialogToken
    AbstractControlDesignToken --> DrawerToken
    AbstractControlDesignToken --> EmptyToken
    AbstractControlDesignToken --> ExpanderToken
    AbstractControlDesignToken --> FloatButtonToken
    AbstractControlDesignToken --> FlyoutToken
    AbstractControlDesignToken --> FormToken
    AbstractControlDesignToken --> GroupBoxToken
    AbstractControlDesignToken --> ImagePreviewerToken
    AbstractControlDesignToken --> InputToken
    AbstractControlDesignToken --> ListBoxToken
    AbstractControlDesignToken --> ListViewToken
    AbstractControlDesignToken --> MarqueeLabelToken
    AbstractControlDesignToken --> MentionsToken
    AbstractControlDesignToken --> MenuToken
    AbstractControlDesignToken --> MessageToken
    AbstractControlDesignToken --> MessageBoxToken
    AbstractControlDesignToken --> NavMenuToken
    AbstractControlDesignToken --> NotificationToken
    AbstractControlDesignToken --> OptionButtonGroupToken
    AbstractControlDesignToken --> PaginationToken
    AbstractControlDesignToken --> PopupHostToken
    AbstractControlDesignToken --> PopupConfirmToken
    AbstractControlDesignToken --> ProgressBarToken
    AbstractControlDesignToken --> QRCodeToken
    AbstractControlDesignToken --> RadioButtonToken
    AbstractControlDesignToken --> RateToken
    AbstractControlDesignToken --> ResultToken
    AbstractControlDesignToken --> ScrollViewerToken
    AbstractControlDesignToken --> SegmentedToken
    AbstractControlDesignToken --> SelectToken
    AbstractControlDesignToken --> SeparatorToken
    AbstractControlDesignToken --> SkeletonToken
    AbstractControlDesignToken --> SliderToken
    AbstractControlDesignToken --> SpaceToken
    AbstractControlDesignToken