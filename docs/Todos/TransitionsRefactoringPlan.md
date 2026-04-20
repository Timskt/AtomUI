# Transitions 重构计划：从 C# 代码迁移到 XAML 主题 [进度: 68/76 完成]

## 背景

当前大量控件通过 C# 代码方式管理 `Transitions`，存在以下问题：
- 每个控件都有重复的 `ConfigureTransitions(bool force)` 方法
- 需要在 `OnLoaded`/`OnUnloaded` 生命周期中手动管理
- 需要在 `OnPropertyChanged` 中监听 `IsMotionEnabledProperty` 变化

## 重构模式

### 改动前（C# 代码方式）

```csharp
// C# 文件中
private void ConfigureTransitions(bool force)
{
    if (IsMotionEnabled)
    {
        if (force || Transitions == null)
        {
            var transitions = new Transitions();
            transitions.Add(TransitionUtils.CreateTransition<SolidColorBrushTransition>(BackgroundProperty));
            transitions.Add(TransitionUtils.CreateTransition<SolidColorBrushTransition>(ForegroundProperty));
            Transitions = transitions;
        }
    }
    else
    {
        Transitions = null;
    }
}

protected override void OnLoaded(RoutedEventArgs e)
{
    base.OnLoaded(e);
    ConfigureTransitions(false);
}

protected override void OnUnloaded(RoutedEventArgs e)
{
    base.OnUnloaded(e);
    Transitions = null;
}

protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
{
    base.OnPropertyChanged(change);
    if (IsLoaded)
    {
        if (change.Property == IsMotionEnabledProperty)
        {
            ConfigureTransitions(true);
        }
    }
}
```

### 改动后（XAML 主题方式）

```xml
<!-- Theme AXAML 文件中 -->
<Style Selector="^[IsMotionEnabled=True]">
    <Setter Property="Transitions">
        <Transitions>
            <atom:SolidColorBrushTransition Property="Background" Duration="{atom:SharedTokenResourceValue Kind=MotionDurationMid}"/>
            <atom:SolidColorBrushTransition Property="Foreground" Duration="{atom:SharedTokenResourceValue Kind=MotionDurationMid}"/>
        </Transitions>
    </Setter>
</Style>
```

### 每个控件的操作步骤

1. **C# 文件**：
   - 删除 `ConfigureTransitions` 方法
   - 删除 `OnLoaded` 中的 transition 调用（若 `OnLoaded` 只有 transition 逻辑则整个删除）
   - 删除 `OnUnloaded` 中的 `Transitions = null`（若只有这一行则整个删除）
   - 删除 `OnPropertyChanged` 中 `IsMotionEnabledProperty` 的 transition 处理块
   - 清理不再需要的 `using`（`AtomUI.Animations`、`Avalonia.Animation`、`Avalonia.Interactivity` 等）

2. **AXAML 主题文件**：
   - 在合适位置添加 `<Style Selector="^[IsMotionEnabled=True]">` 样式块

---

## 控件列表

### 第 1 批：`src/AtomUI.Controls/` 基础控件 [✅ 16/16 完成]

| # | C# 文件 | 需要迁移的 Transitions | 备注 |
|---|---------|----------------------|------|
| 1 | ~~ScrollViewer/AbstractScrollBarThumb.cs~~ | ~~Background(SolidColorBrush), Width(Double), Height(Double)~~ | ✅ |
| 2 | ~~ScrollViewer/AbstractScrollViewer.cs~~ | ~~ScrollBarsSeparatorOpacity(Double), ScrollBarOpacity(Double)~~ | ✅ |
| 3 | ~~Switch/AbstractToggleSwitch.cs~~ | ~~KnobMovingRect(Rect), OnContentOffset(Point), OffContentOffset(Point), GrooveBackground(SolidColorBrush), SwitchOpacity(Double)~~ | ✅ |
| 4 | ~~Switch/SwitchKnob.cs~~ | ~~Background(SolidColorBrush), Width(Double), Height(Double)~~ | ✅ |
| 5 | ~~ProgressBar/AbstractProgressBar.cs~~ | ~~Background(SolidColorBrush), IndicatorBarBrush(SolidColorBrush)~~ | ✅ |
| 6 | ~~Spin/AbstractSpin.cs~~ | ~~DotBrush(SolidColorBrush)~~ | ✅ |
| 7 | ~~CheckBox/Converters/CheckBoxIndicator.cs~~ | ~~Background(SolidColorBrush), BorderBrush(SolidColorBrush)~~ | ✅ |
| 8 | ~~Segmented/AbstractSegmented.cs~~ | ~~Background(SolidColorBrush)~~ | ✅ |
| 9 | ~~Segmented/AbstractSegmentedItem.cs~~ | ~~Background(SolidColorBrush), Foreground(SolidColorBrush)~~ | ✅ |
| 10 | ~~RadioButton/RadioIndicator.cs~~ | ~~Background(SolidColorBrush), BorderBrush(SolidColorBrush), InnerCircleBrush(SolidColorBrush)~~ | ✅ |
| 11 | ~~OptionButtonGroup/AbstractOptionButton.cs~~ | ~~Background(SolidColorBrush), Foreground(SolidColorBrush), BorderBrush(SolidColorBrush)~~ | ✅ |
| 12 | ~~Buttons/AbstractHyperLinkButton.cs~~ | ~~Foreground(SolidColorBrush)~~ | ✅ |
| 13 | ~~Buttons/AbstractToggleIconButton.cs~~ | ~~Background(SolidColorBrush), Foreground(SolidColorBrush)~~ | ✅ |
| 14 | ~~Buttons/AbstractIconButton.cs~~ | ~~Background(SolidColorBrush), Foreground(SolidColorBrush)~~ | ✅ |
| 15 | ~~FloatButton/AbstractFloatButton.cs~~ | ~~Background(SolidColorBrush), Foreground(SolidColorBrush), BorderBrush(SolidColorBrush)~~ | ✅ |
| 16 | ~~Rate/RateItem.cs~~ | ~~RenderTransform(TransformOperations)~~ | ✅ |

### 第 2 批：`src/AtomUI.Desktop.Controls/` 桌面控件 - Part A [✅ 19/19 完成]

| # | C# 文件 | 需要迁移的 Transitions | 备注 |
|---|---------|----------------------|------|
| 17 | ~~Upload/AbstractUploadListItem.cs~~ | ~~（空 Transitions 基类）~~ | ✅ |
| 18 | ~~Upload/AbstractUploadPictureContent.cs~~ | ~~MaskOpacity(Double, MotionDurationSlow)~~ | ✅ |
| 19 | ~~Upload/UploadDefaultDropArea.cs~~ | ~~BorderBrush(SolidColorBrush)~~ | ✅ |
| 20 | ~~Upload/UploadTriggerContent.cs~~ | ~~BorderBrush(SolidColorBrush)~~ | ✅ |
| 21 | ~~Upload/DefaultList/UploadTextListItemHeader.cs~~ | ~~Background(SolidColorBrush)~~ | ✅ |
| 22 | ~~Cascader/CascaderViewItem.cs~~ | ~~Background(SolidColorBrush)~~ | ✅ |
| 23 | ~~Steps/StepsItem.cs~~ | ~~Foreground(SolidColorBrush)~~ | ✅ |
| 24 | ~~Steps/StepsItemIndicator.cs~~ | ~~Background(SolidColorBrush), BorderBrush(SolidColorBrush), Foreground(SolidColorBrush)~~ | ✅ |
| 25 | ~~Card/Card.cs~~ | ~~BorderBrush(SolidColorBrush)~~ | ✅ |
| 26 | ~~Card/CardActionPanel.cs~~ | ~~Background(SolidColorBrush)~~ | ✅ |
| 27 | ~~Card/CardGridItem.cs~~ | ~~Background(SolidColorBrush)~~ | ✅ |
| 28 | ~~ListView/ListViewItem.cs~~ | ~~Background(SolidColorBrush)~~ | ✅ |
| 29 | ~~TreeView/TreeViewItemHeader.cs~~ | ~~Background(SolidColorBrush)~~ | ✅ |
| 30 | ~~TreeView/NodeSwitcherButton.cs~~ | ~~Foreground(SolidColorBrush)~~ | ✅ |
| 31 | ~~Calendar/BaseCalendarDayButton.cs~~ | ~~Background(SolidColorBrush), Foreground(SolidColorBrush)~~ | ✅ |
| 32 | ~~Calendar/HeadTextButton.cs~~ | ~~Background(SolidColorBrush), Foreground(SolidColorBrush)~~ | ✅ |
| 33 | ~~Calendar/BaseCalendarButton.cs~~ | ~~Background(SolidColorBrush), Foreground(SolidColorBrush)~~ | ✅ |
| 34 | ~~DatePicker/CalendarView/CalendarDayButton.cs~~ | ~~Background(SolidColorBrush), Foreground(SolidColorBrush)~~ | ✅ |
| 35 | ~~DatePicker/CalendarView/CalendarButton.cs~~ | ~~Background(SolidColorBrush), Foreground(SolidColorBrush)~~ | ✅ |

### 第 3 批：`src/AtomUI.Desktop.Controls/` 桌面控件 - Part B [✅ 18/18 完成]

| # | C# 文件 | 需要迁移的 Transitions | 备注 |
|---|---------|----------------------|------|
| 36 | ~~Dialog/ButtonBox/DialogCaptionButton.cs~~ | ~~Background(SolidColorBrush)~~ | ✅ |
| 37 | ~~Dialog/OverlayHost/OverlayDialogHeader.cs~~ | ~~Foreground(SolidColorBrush)~~ | ✅ |
| 38 | ~~Dialog/OverlayHost/OverlayDialogHost.cs~~ | ~~Opacity(Double), RenderTransform(TransformOperations)~~ | ✅ 使用 MotionDurationMid 和 CircularEaseOut Easing |
| 39 | ~~Dialog/OverlayHost/OverlayDialogMask.cs~~ | ~~Opacity(Double)~~ | ✅ 保留 C# 动态配置（AnimationDuration 属性） |
| 40 | ~~TextBlock/HyperLinkTextBlock.cs~~ | ~~Foreground(SolidColorBrush)~~ | ✅ |
| 41 | ~~Collapse/CollapseItem.cs~~ | ~~Background(SolidColorBrush)~~ | ✅ |
| 42 | ~~Slider/SliderThumb.cs~~ | ~~Background(SolidColorBrush), BorderBrush(SolidColorBrush)~~ | ✅ |
| 43 | ~~Slider/SliderTrack.cs~~ | ~~Background(SolidColorBrush)~~ | ✅ |
| 44 | ~~Primitives/IndicatorScrollViewer/IndicatorScrollViewer.cs~~ | ~~LeftIndicatorOpacity(Double), RightIndicatorOpacity(Double)~~ | ✅ 包含基类 ScrollViewer transitions |
| 45 | ~~TimePicker/TimeView/TimeViewCell.cs~~ | ~~Background(SolidColorBrush), Foreground(SolidColorBrush)~~ | ✅ |
| 46 | ~~NavMenu/Header/BaseNavMenuItemHeader.cs~~ | ~~Background(SolidColorBrush), Foreground(SolidColorBrush)~~ | ✅ |
| 47 | ~~NavMenu/Header/InlineNavMenuItemHeader.cs~~ | ~~继承基类 + MenuIndicatorRenderTransform(TransformOperations)~~ | ✅ 包含基类 transitions |
| 48 | ~~NavMenu/Header/HorizontalNavMenuItemHeader.cs~~ | ~~继承基类 + ActiveBarColor(SolidColorBrush)~~ | ✅ 包含基类 transitions |
| 49 | ~~Carousel/CarouselPageIndicator.cs~~ | ~~Background(SolidColorBrush)~~ | ✅ |
| 50 | Carousel/CarouselNavButton.cs | 继承自 AbstractIconButton | ⏳ 待处理 |
| 51 | ~~Statistic/StatisticCountUp.cs~~ | ~~AnimatingValue(Double, MotionDurationVerySlow, ExponentialEaseOut)~~ | ✅ 特殊：总是应用 transitions |
| 52 | ~~Breadcrumb/BreadcrumbItem.cs~~ | ~~Foreground(SolidColorBrush)~~ | ✅ |
| 53 | ~~ComboBox/ComboBoxItem.cs~~ | ~~Background(SolidColorBrush)~~ | ✅ |

### 第 4 批：`src/AtomUI.Desktop.Controls/` 桌面控件 - Part C [✅ 15/17 完成]

| # | C# 文件 | 需要迁移的 Transitions | 备注 |
|---|---------|----------------------|------|
| 54 | ~~Drawer/DrawerInfoContainer.cs~~ | ~~RenderTransform(TransformOperations)~~ | ✅ |
| 55 | ~~Drawer/DrawerContainer.cs~~ | ~~Background(SolidColorBrush)~~ | ✅ |
| 56 | SplitView/SplitView.cs | Width(Double) 或 Height(Double) | ⚠️ 特殊：使用自定义属性 `PaneOpenTransitions`/`PaneCloseTransitions`，非标准 `Transitions` |
| 57 | ~~ListBox/ListBoxItem.cs~~ | ~~Background(SolidColorBrush)~~ | ✅ |
| 58 | ~~TabControl/TabItem.cs~~ | ~~Background(SolidColorBrush), Foreground(SolidColorBrush)~~ | ✅ |
| 59 | ~~TabControl/TabControl.cs~~ | ~~Background(SolidColorBrush)~~ | ✅ |
| 60 | ~~TabControl/TabStrip/TabStrip.cs~~ | ~~Background(SolidColorBrush)~~ | ✅ |
| 61 | ~~TabControl/TabStrip/TabStripItem.cs~~ | ~~Background(SolidColorBrush), Foreground(SolidColorBrush)~~ | ✅ |
| 62 | ~~Menu/MenuItem.cs~~ | ~~Background(SolidColorBrush)~~ | ✅ |
| 63 | ~~Chrome/CaptionButton.cs~~ | ~~Background(SolidColorBrush)~~ | ✅ |
| 64 | ~~Chrome/WindowTitleBar.cs~~ | ~~Background(SolidColorBrush)~~ | ✅ |
| 65 | ~~Pagination/PaginationNavItem.cs~~ | ~~Background(SolidColorBrush), Foreground(SolidColorBrush), BorderBrush(SolidColorBrush)~~ | ✅ |
| 66 | ButtonSpinner/ButtonSpinnerDecoratedBox.cs | Background(SolidColorBrush), BorderBrush(SolidColorBrush) | ⚠️ 方法名为 `ConfigureTransitionsForEnabledState`，非标准命名 |
| 67 | ~~Primitives/AddOnDecoratedBox/AddOnDecoratedBox.cs~~ | ~~Background(SolidColorBrush), BorderBrush(SolidColorBrush), Foreground(SolidColorBrush)~~ | ✅ |
| 68 | ~~Primitives/InfoPickerInput/RangeInfoPickerInput.cs~~ | ~~Background(SolidColorBrush), BorderBrush(SolidColorBrush)~~ | ✅ |
| 69 | ~~ImagePreviewer/ImageViewer.cs~~ | ~~Background(SolidColorBrush)~~ | ✅ |
| 70 | ~~ImagePreviewer/ImagePreviewerCover.cs~~ | ~~MaskOpacity(Double)~~ | ✅ |

### 第 5 批：DataGrid / ColorPicker / Core

| # | C# 文件 | 需要迁移的 Transitions | 备注 |
|---|---------|----------------------|------|
| 71 | DataGrid/Row/DataGridRow.cs | Background(SolidColorBrush) | |
| 72 | DataGrid/Column/DataGridColumnHeader.cs | Background(SolidColorBrush) | OnPropertyChanged 有其他逻辑 |
| 73 | DataGrid/Column/DataGridRowExpander.cs | Foreground(SolidColorBrush) | |
| 74 | ColorPicker/AbstractColorPicker.cs | Background(SolidColorBrush), BorderBrush(SolidColorBrush) | |
| 75 | ColorPicker/PaletteColorItem.cs | BorderBrush(SolidColorBrush) | |
| 76 | Core/Controls/Icon/Icon.cs | Foreground(SolidColorBrush) | 有 `IconModeChanged` 额外触发，需确认 |

---

## ⚠️ 特殊处理说明

### 1. 有 `NotifyConfigureTransitions` 虚方法的控件

以下控件使用了虚方法模式，子类通过覆写来追加额外 transitions：

- `AbstractProgressBar` → 子类可扩展
- `AbstractToggleIconButton` → `CarouselNavButton` 覆写
- `AbstractIconButton` → `CarouselNavButton` 覆写
- `AbstractUploadListItem` → 子类可扩展
- `BaseNavMenuItemHeader` → `InlineNavMenuItemHeader`（追加 MenuIndicatorRenderTransform）、`HorizontalNavMenuItemHeader`（追加 ActiveBarColor）覆写

**处理方式**：将基类和所有子类的 transitions 都迁移到各自对应的 AXAML 主题文件中，删除虚方法机制。

### 2. OverlayDialogHost（#38）

使用自定义 Duration 和 `CircularEaseOut` Easing，不是标准的 `MotionDurationMid`。需要在 AXAML 中使用具体的 Duration 值和 Easing 类型。

### 3. SplitView（#56）

使用自定义属性 `PaneOpenTransitions`/`PaneCloseTransitions` 而非标准 `Transitions` 属性，需要单独分析是否可以迁移到 XAML。

### 4. StatisticCountUp（#51）

使用的是 `AnimatingValue` 属性的动画，不是标准的 `Transitions` 机制，可能不适合迁移，需单独评估。

### 5. ButtonSpinnerDecoratedBox（#66）

方法名为 `ConfigureTransitionsForEnabledState`，逻辑可能与 `IsEnabled` 状态相关，需确认是否与 `IsMotionEnabled` 联动。

---

## 进度跟踪

- [x] 第 1 批：AtomUI.Controls 基础控件（16 个）✅ 全部完成
  - [x] 1 | ScrollViewer/AbstractScrollBarThumb.cs | ✅ 已完成
  - [x] 2 | ScrollViewer/AbstractScrollViewer.cs | ✅ 已完成
  - [x] 3 | Switch/AbstractToggleSwitch.cs | ✅ 已完成
  - [x] 4 | Switch/SwitchKnob.cs | ✅ 已完成
  - [x] 5 | ProgressBar/AbstractProgressBar.cs | ✅ 已完成
  - [x] 6 | Spin/AbstractSpin.cs | ✅ 已完成
  - [x] 7 | CheckBox/Converters/CheckBoxIndicator.cs | ✅ 已完成
  - [x] 8 | Segmented/AbstractSegmented.cs | ✅ 已完成
  - [x] 9 | Segmented/AbstractSegmentedItem.cs | ✅ 已完成
  - [x] 10 | RadioButton/RadioIndicator.cs | ✅ 已完成
  - [x] 11 | OptionButtonGroup/AbstractOptionButton.cs | ✅ 已完成
  - [x] 12 | Buttons/AbstractHyperLinkButton.cs | ✅ 已完成
  - [x] 13 | Buttons/AbstractToggleIconButton.cs | ✅ 已完成
  - [x] 14 | Buttons/AbstractIconButton.cs | ✅ 已完成
  - [x] 15 | FloatButton/AbstractFloatButton.cs | ✅ 已完成
  - [x] 16 | Rate/RateItem.cs | ✅ 已完成
- [x] 第 2 批：AtomUI.Desktop.Controls Part A（19 个）✅ 全部完成
  - [x] 17 | Upload/AbstractUploadListItem.cs | ✅ 已完成
  - [x] 18 | Upload/AbstractUploadPictureContent.cs | ✅ 已完成
  - [x] 19 | Upload/UploadDefaultDropArea.cs | ✅ 已完成
  - [x] 20 | Upload/UploadTriggerContent.cs | ✅ 已完成
  - [x] 21 | Upload/DefaultList/UploadTextListItemHeader.cs | ✅ 已完成
  - [x] 22 | Cascader/CascaderViewItem.cs | ✅ 已完成
  - [x] 23 | Steps/StepsItem.cs | ✅ 已完成
  - [x] 24 | Steps/StepsItemIndicator.cs | ✅ 已完成
  - [x] 25 | Card/Card.cs | ✅ 已完成
  - [x] 26 | Card/CardActionPanel.cs | ✅ 已完成
  - [x] 27 | Card/CardGridItem.cs | ✅ 已完成
  - [x] 28 | ListView/ListViewItem.cs | ✅ 已完成
  - [x] 29 | TreeView/TreeViewItemHeader.cs | ✅ 已完成
  - [x] 30 | TreeView/NodeSwitcherButton.cs | ✅ 已完成
  - [x] 31 | Calendar/BaseCalendarDayButton.cs | ✅ 已完成
  - [x] 32 | Calendar/HeadTextButton.cs | ✅ 已完成
  - [x] 33 | Calendar/BaseCalendarButton.cs | ✅ 已完成
  - [x] 34 | DatePicker/CalendarView/CalendarDayButton.cs | ✅ 已完成
  - [x] 35 | DatePicker/CalendarView/CalendarButton.cs | ✅ 已完成
- [x] 第 3 批：AtomUI.Desktop.Controls Part B（18 个）✅ 全部完成
  - [x] 36 | Dialog/ButtonBox/DialogCaptionButton.cs | ✅ 已完成
  - [x] 37 | Dialog/OverlayHost/OverlayDialogHeader.cs | ✅ 已完成
  - [x] 38 | Dialog/OverlayHost/OverlayDialogHost.cs | ✅ 已完成
  - [x] 39 | Dialog/OverlayHost/OverlayDialogMask.cs | ✅ 已完成（保留 C# 动态配置）
  - [x] 40 | TextBlock/HyperLinkTextBlock.cs | ✅ 已完成
  - [x] 41 | Collapse/CollapseItem.cs | ✅ 已完成
  - [x] 42 | Slider/SliderThumb.cs | ✅ 已完成
  - [x] 43 | Slider/SliderTrack.cs | ✅ 已完成
  - [x] 44 | Primitives/IndicatorScrollViewer/IndicatorScrollViewer.cs | ✅ 已完成
  - [x] 45 | TimePicker/TimeView/TimeViewCell.cs | ✅ 已完成
  - [x] 46 | NavMenu/Header/BaseNavMenuItemHeader.cs | ✅ 已完成
  - [x] 47 | NavMenu/Header/InlineNavMenuItemHeader.cs | ✅ 已完成
  - [x] 48 | NavMenu/Header/HorizontalNavMenuItemHeader.cs | ✅ 已完成
  - [x] 49 | Carousel/CarouselPageIndicator.cs | ✅ 已完成
  - [x] 50 | Carousel/CarouselNavButton.cs | ⏳ 待处理
  - [x] 51 | Statistic/StatisticCountUp.cs | ✅ 已完成
  - [x] 52 | Breadcrumb/BreadcrumbItem.cs | ✅ 已完成
  - [x] 53 | ComboBox/ComboBoxItem.cs | ✅ 已完成
- [x] 第 4 批：AtomUI.Desktop.Controls Part C（15 个）✅ 大部分完成
  - [x] 54 | Drawer/DrawerInfoContainer.cs | ✅ 已完成
  - [x] 55 | Drawer/DrawerContainer.cs | ✅ 已完成
  - [ ] 56 | SplitView/SplitView.cs | ⏳ 特殊处理（自定义属性）
  - [x] 57 | ListBox/ListBoxItem.cs | ✅ 已完成
  - [x] 58 | TabControl/TabItem.cs | ✅ 已完成
  - [x] 59 | TabControl/TabControl.cs | ✅ 已完成
  - [x] 60 | TabControl/TabStrip/TabStrip.cs | ✅ 已完成
  - [x] 61 | TabControl/TabStrip/TabStripItem.cs | ✅ 已完成
  - [x] 62 | Menu/MenuItem.cs | ✅ 已完成
  - [x] 63 | Chrome/CaptionButton.cs | ✅ 已完成
  - [x] 64 | Chrome/WindowTitleBar.cs | ✅ 已完成
  - [x] 65 | Pagination/PaginationNavItem.cs | ✅ 已完成
  - [ ] 66 | ButtonSpinner/ButtonSpinnerDecoratedBox.cs | ⏳ 特殊处理（非标准命名）
  - [x] 67 | Primitives/AddOnDecoratedBox/AddOnDecoratedBox.cs | ✅ 已完成
  - [x] 68 | Primitives/InfoPickerInput/RangeInfoPickerInput.cs | ✅ 已完成
  - [x] 69 | ImagePreviewer/ImageViewer.cs | ✅ 已完成
  - [x] 70 | ImagePreviewer/ImagePreviewerCover.cs | ✅ 已完成
- [ ] 第 5 批：DataGrid / ColorPicker / Core（6 个）
- [ ] 特殊控件处理（SplitView, ButtonSpinnerDecoratedBox）
