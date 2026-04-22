# AtomUI Gallery — ShowCase 完整清单

> **文档版本**：2026-04-15  
> 共 **6 大分类、67 个 ShowCase**

---

## 概述

每个 ShowCase 由以下三部分组成：
1. **ViewModel** — 继承 `ReactiveObject`，实现 `IRoutableViewModel`（部分额外实现 `IActivatableViewModel`）
2. **View** — 继承 `ReactiveUserControl<TViewModel>`
3. **注册** — 在 `ShowCaseViewModule.RegisterViews()` 和 `CaseNavigationViewModel.RegisterShowCaseViewModels()` 中双重注册

### 命名约定

| 层 | 命名模式 | 示例 |
|----|---------|------|
| ViewModel | `{ControlName}ViewModel` | `ButtonViewModel` |
| View (ShowCase) | `{ControlName}ShowCase` | `ButtonShowCase` |
| View (Page) | `{ControlName}Page` | `AboutUsPage`, `OsInfoPage` |
| ViewModel ID | `EntityKey` 静态字段 | `ButtonViewModel.ID = "Button"` |

> **注意**：`AboutUsPage` 和 `OsInfoPage` 使用 `Page` 后缀而非 `ShowCase`，因为它们是信息页面而非控件演示。

---

## 1. General（通用）— 9 个

| # | ID | ViewModel | View | 实现 IActivatableViewModel |
|---|---|---|---|---|
| 1 | `AboutUs` | `AboutUsViewModel` | `AboutUsPage` | ✅ |
| 2 | `Button` | `ButtonViewModel` | `ButtonShowCase` | ✅ |
| 3 | `FloatButton` | `FloatButtonViewModel` | `FloatButtonShowCase` | ❌ |
| 4 | `CustomizeTheme` | `CustomizeThemeViewModel` | `CustomizeThemeShowCase` | ❌ |
| 5 | `Icon` | `IconViewModel` | `IconShowCase` | ❌ |
| 6 | `OsInfo` | `OsInfoViewModel` | `OsInfoPage` | ✅ |
| 7 | `Palette` | `PaletteViewModel` | `PaletteShowCase` | ❌ |
| 8 | `Separator` | `SeparatorViewModel` | `SeparatorShowCase` | ❌ |
| 9 | `SplitButton` | `SplitButtonViewModel` | `SplitButtonShowCase` | ❌ |

---

## 2. Layout（布局）— 5 个

| # | ID | ViewModel | View | 实现 IActivatableViewModel |
|---|---|---|---|---|
| 1 | `BoxPanel` | `BoxPanelViewModel` | `BoxPanelShowCase` | ❌ |
| 2 | `FlexPanel` | `FlexPanelViewModel` | `FlexPanelShowCase` | ❌ |
| 3 | `Grid` | `GridViewModel` | `GridShowCase` | ❌ |
| 4 | `Space` | `SpaceViewModel` | `SpaceShowCase` | ❌ |
| 5 | `Splitter` | `SplitterViewModel` | `SplitterShowCase` | ❌ |

> ⚠️ **BoxPanel** 标记为 `Deprecated`（废弃），导航菜单中附带红色 `废弃` 标签。

---

## 3. Navigation（导航）— 8 个

| # | ID | ViewModel | View | 实现 IActivatableViewModel |
|---|---|---|---|---|
| 1 | `Breadcrumb` | `BreadcrumbViewModel` | `BreadcrumbShowCase` | ❌ |
| 2 | `ButtonSpinner` | `ButtonSpinnerViewModel` | `ButtonSpinnerShowCase` | ❌ |
| 3 | `ComboBox` | `ComboBoxViewModel` | `ComboBoxShowCase` | ❌ |
| 4 | `DropdownButton` | `DropdownButtonViewModel` | `DropdownButtonShowCase` | ❌ |
| 5 | `Menu` | `MenuViewModel` | `MenuShowCase` | ❌ |
| 6 | `Pagination` | `PaginationViewModel` | `PaginationShowCase` | ❌ |
| 7 | `Steps` | `StepsViewModel` | `StepsShowCase` | ❌ |
| 8 | `TabControl` | `TabControlViewModel` | `TabControlShowCase` | ❌ |

---

## 4. Data Entry（数据录入）— 18 个

| # | ID | ViewModel | View | 实现 IActivatableViewModel |
|---|---|---|---|---|
| 1 | `AutoComplete` | `AutoCompleteViewModel` | `AutoCompleteShowCase` | ✅ |
| 2 | `Cascader` | `CascaderViewModel` | `CascaderShowCase` | ✅ |
| 3 | `CheckBox` | `CheckBoxViewModel` | `CheckBoxShowCase` | ✅ |
| 4 | `ColorPicker` | `ColorPickerViewModel` | `ColorPickerShowCase` | ❌ |
| 5 | `DatePicker` | `DatePickerViewModel` | `DatePickerShowCase` | ❌ |
| 6 | `Form` | `FormViewModel` | `FormShowCase` | ❌ |
| 7 | `LineEdit` | `LineEditViewModel` | `LineEditShowCase` | ❌ |
| 8 | `Mentions` | `MentionsViewModel` | `MentionsShowCase` | ❌ |
| 9 | `NumberUpDown` | `NumberUpDownViewModel` | `NumberUpDownShowCase` | ❌ |
| 10 | `RadioButton` | `RadioButtonViewModel` | `RadioButtonShowCase` | ❌ |
| 11 | `Rate` | `RateViewModel` | `RateShowCase` | ❌ |
| 12 | `Select` | `SelectViewModel` | `SelectShowCase` | ❌ |
| 13 | `Slider` | `SliderViewModel` | `SliderShowCase` | ❌ |
| 14 | `TimePicker` | `TimePickerViewModel` | `TimePickerShowCase` | ❌ |
| 15 | `ToggleSwitch` | `ToggleSwitchViewModel` | `ToggleSwitchShowCase` | ❌ |
| 16 | `Transfer` | `TransferViewModel` | `TransferShowCase` | ❌ |
| 17 | `TreeSelect` | `TreeSelectViewModel` | `TreeSelectShowCase` | ❌ |
| 18 | `Upload` | `UploadViewModel` | `UploadShowCase` | ❌ |

---

## 5. Data Display（数据展示）— 22 个

| # | ID | ViewModel | View | 实现 IActivatableViewModel |
|---|---|---|---|---|
| 1 | `Avatar` | `AvatarViewModel` | `AvatarShowCase` | ✅ |
| 2 | `Badge` | `BadgeViewModel` | `BadgeShowCase` | ✅ |
| 3 | `Calendar` | `CalendarViewModel` | `CalendarShowCase` | ❌ |
| 4 | `Card` | `CardViewModel` | `CardShowCase` | ❌ |
| 5 | `Carousel` | `CarouselViewModel` | `CarouselShowCase` | ❌ |
| 6 | `Collapse` | `CollapseViewModel` | `CollapseShowCase` | ❌ |
| 7 | `DataGrid` | `DataGridViewModel` | `DataGridShowCase` | ❌ |
| 8 | `Descriptions` | `DescriptionsViewModel` | `DescriptionsShowCase` | ❌ |
| 9 | `Empty` | `EmptyViewModel` | `EmptyShowCase` | ❌ |
| 10 | `Expander` | `ExpanderViewModel` | `ExpanderShowCase` | ❌ |
| 11 | `GroupBox` | `GroupBoxViewModel` | `GroupBoxShowCase` | ❌ |
| 12 | `ImagePreviewer` | `ImagePreviewerViewModel` | `ImagePreviewerShowCase` | ❌ |
| 13 | `InfoFlyout` | `InfoFlyoutViewModel` | `InfoFlyoutShowCase` | ❌ |
| 14 | `List` | `ListViewModel` | `ListShowCase` | ❌ |
| 15 | `QRCode` | `QRCodeViewModel` | `QRCodeShowCase` | ❌ |
| 16 | `Segmented` | `SegmentedViewModel` | `SegmentedShowCase` | ❌ |
| 17 | `Statistic` | `StatisticViewModel` | `StatisticShowCase` | ❌ |
| 18 | `Tag` | `TagViewModel` | `TagShowCase` | ❌ |
| 19 | `Timeline` | `TimelineViewModel` | `TimelineShowCase` | ❌ |
| 20 | `Tooltip` | `TooltipViewModel` | `TooltipShowCase` | ❌ |
| 21 | `Tour` | `TourViewModel` | `TourShowCase` | ❌ |
| 22 | `TreeView` | `TreeViewViewModel` | `TreeViewShowCase` | ❌ |

---

## 6. Feedback（反馈）— 11 个

| # | ID | ViewModel | View | 实现 IActivatableViewModel |
|---|---|---|---|---|
| 1 | `Alert` | `AlertViewModel` | `AlertShowCase` | ❌ |
| 2 | `Drawer` | `DrawerViewModel` | `DrawerShowCase` | ❌ |
| 3 | `Message` | `MessageViewModel` | `MessageShowCase` | ❌ |
| 4 | `Modal` | `ModalViewModel` | `ModalShowCase` | ❌ |
| 5 | `Notification` | `NotificationViewModel` | `NotificationShowCase` | ❌ |
| 6 | `PopupConfirm` | `PopupConfirmViewModel` | `PopupConfirmShowCase` | ❌ |
| 7 | `ProgressBar` | `ProgressBarViewModel` | `ProgressBarShowCase` | ❌ |
| 8 | `Result` | `ResultViewModel` | `ResultShowCase` | ❌ |
| 9 | `Skeleton` | `SkeletonViewModel` | `SkeletonShowCase` | ❌ |
| 10 | `Spin` | `SpinViewModel` | `SpinShowCase` | ❌ |
| 11 | `Watermark` | `WatermarkViewModel` | `WatermarkShowCase` | ❌ |

> 注：`ModalShowCase` 额外引用了 `ModalUserControlViewModel` + `ModalUserControlView`（用于演示自定义内容 Dialog）。

---

## 7. ShowCase 专用控件（ShowCaseControls）

部分复杂 ShowCase 需要自定义控件来完成演示。这些控件位于 `ShowCases/ShowCaseControls/` 目录，有自己的 `ShowCaseControlsThemesProvider`。

### Form ShowCase 专用控件

| 控件 | 文件 | 用途 |
|---|---|---|
| `Captcha` | `Captcha.cs` + `CaptchaTheme.axaml` | 验证码输入控件演示 |
| `Donation` | `Donation.cs` + `DonationTheme.axaml` | 捐款金额选择控件演示 |
| `PhoneNumber` | `PhoneNumber.cs` + `PhoneNumberTheme.axaml` | 电话号码输入控件演示 |
| `PriceInput` | `PriceInput.cs` + `PriceInputTheme.axaml` | 价格输入控件演示 |

这些控件通过 `FormThemes.axaml` 统一注册，最终由 `ShowCaseControlsThemesProvider` 提供给主题系统。

---

## 8. 新增 ShowCase 步骤

当需要为新控件添加 ShowCase 时，按以下步骤操作：

### 1. 创建 ViewModel

在 `ShowCases/ViewModels/<Category>/` 下创建 `XxxViewModel.cs`：

```csharp
using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class XxxViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Xxx";
    public IScreen HostScreen { get; }
    public string UrlPathSegment { get; } = ID.ToString();

    public XxxViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}
```

### 2. 创建 View

在 `ShowCases/Views/<Category>/` 下创建 `XxxShowCase.axaml` + `XxxShowCase.axaml.cs`：

```csharp
using AtomUIGallery.ShowCases.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.ShowCases.Views;

public partial class XxxShowCase : ReactiveUserControl<XxxViewModel>
{
    public XxxShowCase()
    {
        this.WhenActivated(disposables => { });
        InitializeComponent();
    }
}
```

### 3. 注册 View 映射

在 `ShowCaseRegister.cs` 的 `ShowCaseViewModule` 中添加：

```csharp
locator.Map<XxxViewModel, XxxShowCase>(() => new XxxShowCase());
```

### 4. 注册 ViewModel 工厂

在 `CaseNavigationViewModel.cs` 对应的 `Register*ViewModels()` 方法中添加：

```csharp
_showCaseViewModelFactories.Add(XxxViewModel.ID, () => new XxxViewModel(HostScreen));
```

### 5. 添加导航菜单项

在 `CaseNavigation.axaml` 对应分类下添加：

```xml
<atom:NavMenuNode Header="{gallery:CaseNavigationLangResource <Category>_Xxx}"
                  ItemKey="{x:Static viewmodels:XxxViewModel.ID}" />
```

### 6. 添加本地化字符串

在 `Workspace/Localization/CaseNavigationLang/` 的 `en_US.cs` 和 `zh_CN.cs` 中分别添加：

```csharp
public const string <Category>_Xxx = "Xxx";        // en_US
public const string <Category>_Xxx = "Xxx 中文名";  // zh_CN
```

### 7. 验证

- 运行 Gallery，确认新 ShowCase 出现在左侧导航中
- 点击导航项，确认 View 正确显示
- 切换 亮色/暗色、紧凑模式、中/英文，确认 ShowCase 无异常
- 按 F5 自动遍历测试，确认不会崩溃



