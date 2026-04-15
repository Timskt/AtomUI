# AtomUI Gallery — 完整目录树

> **生成时间**：2026-04-15  
> 排除了 `GeneratedFiles/` 目录（由 Roslyn 源代码生成器自动生成，禁止手动编辑）和二进制图片资源。

```
controlgallery/
├── AtomUIGallery/                              # 主程序库项目
│   ├── AtomUIGallery.csproj                    # 项目文件（多目标框架）
│   ├── BaseGalleryApplication.axaml            # Application XAML 定义
│   ├── BaseGalleryApplication.axaml.cs         # Application 基类（创建 WorkspaceWindow）
│   ├── ThemeManagerBuilderExtensions.cs        # UseGalleryControls() 扩展方法
│   │
│   ├── Properties/
│   │   └── AssemblyInfo.cs                     # XmlnsDefinition 注册 + LanguageSgMetaInfo
│   │
│   ├── Assets/                                 # 静态资源（图标、图片等）
│   │   ├── ATOMUI-LOGO.ico / .png / .svg       # AtomUI Logo（多尺寸）
│   │   ├── AtomUIGallery.ico                   # Gallery 应用图标
│   │   ├── AtomUIOSS-release-banner.png        # 开源发布 Banner
│   │   ├── gallery-logo.png                    # 窗口标题栏 Logo
│   │   ├── AvatarShowCase/                     # Avatar 演示用头像图片
│   │   ├── CardShowCase/                       # Card 演示用封面图片
│   │   ├── EmptyShowCase/                      # Empty 演示用空状态 SVG
│   │   ├── ImagePreviewerShowCase/             # ImagePreviewer 演示用图片
│   │   ├── OSLogos/                            # 操作系统 Logo SVG
│   │   ├── TourShowCase/                       # Tour 演示用封面图片
│   │   └── *.png                               # 其他品牌/二维码/水印图片
│   │
│   ├── Controls/                               # Gallery 专用自定义控件
│   │   ├── ColorItemControl.cs                 # 单个颜色色块展示控件
│   │   ├── ColorItemControlTheme.axaml         # ColorItemControl 主题
│   │   ├── ColorListControl.cs                 # 调色板颜色列表控件
│   │   ├── ColorListControlTheme.axaml         # ColorListControl 主题
│   │   ├── IconGallery.axaml.cs                # 图标画廊控件（含反射扫描缓存）
│   │   ├── IconGalleryTheme.axaml              # IconGallery 主题
│   │   ├── IconInfoItem.axaml.cs               # 单个图标信息展示控件
│   │   ├── IconInfoItemTheme.axaml             # IconInfoItem 主题
│   │   ├── ShowCaseItem.axaml.cs               # 单个 ShowCase 区块控件
│   │   ├── ShowCaseItemTheme.axaml             # ShowCaseItem 主题
│   │   ├── ShowCasePanel.axaml.cs              # ShowCase 双列网格布局面板
│   │   ├── ShowCasePanelTheme.axaml            # ShowCasePanel 主题
│   │   ├── GalleryControlThemesProvider.cs     # 控件主题提供器（代码）
│   │   └── GalleryControlThemesProvider.axaml  # 控件主题提供器（XAML 注册）
│   │
│   ├── Models/                                 # 数据模型
│   │   ├── PaletteColorInfo.cs                 # 调色板颜色信息 record
│   │   └── PackageIconItem.cs                  # 图标信息 record
│   │
│   ├── Utils/                                  # 工具类
│   │   ├── EnumExtension.cs                    # XAML 枚举值绑定 MarkupExtension
│   │   └── LinuxDistributionDetector.cs        # Linux 发行版检测工具（380行）
│   │
│   ├── Workspace/                              # 主工作区（窗口 + 导航）
│   │   ├── ViewModels/
│   │   │   ├── WorkspaceWindowViewModel.cs     # 主窗口 VM（IScreen，持有 RoutingState）
│   │   │   └── CaseNavigationViewModel.cs      # 左侧导航 VM（ViewModel 工厂注册 + 路由导航）
│   │   ├── Views/
│   │   │   ├── WorkspaceWindow.axaml           # 主窗口 XAML（Menu + CaseNavigation + RoutedViewHost）
│   │   │   ├── WorkspaceWindow.axaml.cs        # 主窗口代码（菜单事件处理）
│   │   │   ├── CaseNavigation.axaml            # 左侧导航菜单 XAML（NavMenu 嵌套结构）
│   │   │   └── CaseNavigation.axaml.cs         # 导航代码（LogicalTree 附加 + 路由 + 快捷键）
│   │   └── Localization/
│   │       ├── CaseNavigationLang/
│   │       │   ├── en_US.cs                    # 导航菜单英文语言资源
│   │       │   └── zh_CN.cs                    # 导航菜单中文语言资源
│   │       └── WorkspaceWindowLang/
│   │           ├── en_US.cs                    # 窗口菜单英文语言资源
│   │           └── zh_CN.cs                    # 窗口菜单中文语言资源
│   │
│   ├── ShowCases/                              # ShowCase 展示系统
│   │   ├── ShowCaseRegister.cs                 # AOT 兼容的 VM→View 映射注册（IViewModule）
│   │   │
│   │   ├── ViewModels/                         # 所有 ShowCase ViewModel（按分类组织）
│   │   │   ├── General/                        # 通用类
│   │   │   │   ├── AboutUsViewModel.cs
│   │   │   │   ├── ButtonViewModel.cs
│   │   │   │   ├── CustomizeThemeViewModel.cs
│   │   │   │   ├── FloatButtonViewModel.cs
│   │   │   │   ├── IconViewModel.cs
│   │   │   │   ├── OsInfoViewModel.cs
│   │   │   │   ├── PaletteViewModel.cs
│   │   │   │   ├── SeparatorViewModel.cs
│   │   │   │   └── SplitButtonViewModel.cs
│   │   │   ├── Layout/                         # 布局类
│   │   │   │   ├── BoxPanelViewModel.cs
│   │   │   │   ├── FlexPanelViewModel.cs
│   │   │   │   ├── GridViewModel.cs
│   │   │   │   ├── SpaceViewModel.cs
│   │   │   │   └── SplitterViewModel.cs
│   │   │   ├── Navigation/                     # 导航类
│   │   │   │   ├── BreadcrumbViewModel.cs
│   │   │   │   ├── ButtonSpinnerViewModel.cs
│   │   │   │   ├── ComboBoxViewModel.cs
│   │   │   │   ├── DropdownButtonViewModel.cs
│   │   │   │   ├── MenuViewModel.cs
│   │   │   │   ├── PaginationViewModel.cs
│   │   │   │   ├── StepsViewModel.cs
│   │   │   │   └── TabControlViewModel.cs
│   │   │   ├── DataEntry/                      # 数据录入类
│   │   │   │   ├── AutoCompleteViewModel.cs
│   │   │   │   ├── CascaderViewModel.cs
│   │   │   │   ├── CheckBoxViewModel.cs
│   │   │   │   ├── ColorPickerViewModel.cs
│   │   │   │   ├── DatePickerViewModel.cs
│   │   │   │   ├── FormViewModel.cs
│   │   │   │   ├── LineEditViewModel.cs
│   │   │   │   ├── MentionsViewModel.cs
│   │   │   │   ├── NumberUpDownViewModel.cs
│   │   │   │   ├── RadioButtonViewModel.cs
│   │   │   │   ├── RateViewModel.cs
│   │   │   │   ├── SelectViewModel.cs
│   │   │   │   ├── SliderViewModel.cs
│   │   │   │   ├── TimePickerViewModel.cs
│   │   │   │   ├── ToggleSwitchViewModel.cs
│   │   │   │   ├── TransferViewModel.cs
│   │   │   │   ├── TreeSelectViewModel.cs
│   │   │   │   └── UploadViewModel.cs
│   │   │   ├── DataDisplay/                    # 数据展示类
│   │   │   │   ├── AvatarViewModel.cs
│   │   │   │   ├── BadgeViewModel.cs
│   │   │   │   ├── CalendarViewModel.cs
│   │   │   │   ├── CardViewModel.cs
│   │   │   │   ├── CarouselViewModel.cs
│   │   │   │   ├── CollapseViewModel.cs
│   │   │   │   ├── DataGridViewModel.cs
│   │   │   │   ├── DescriptionsViewModel.cs
│   │   │   │   ├── EmptyViewModel.cs
│   │   │   │   ├── ExpanderViewModel.cs
│   │   │   │   ├── GroupBoxViewModel.cs
│   │   │   │   ├── ImagePreviewerViewModel.cs
│   │   │   │   ├── InfoFlyoutViewModel.cs
│   │   │   │   ├── ListViewModel.cs
│   │   │   │   ├── QRCodeViewModel.cs
│   │   │   │   ├── SegmentedViewModel.cs
│   │   │   │   ├── StatisticViewModel.cs
│   │   │   │   ├── TagViewModel.cs
│   │   │   │   ├── TimelineViewModel.cs
│   │   │   │   ├── TooltipViewModel.cs
│   │   │   │   ├── TourViewModel.cs
│   │   │   │   └── TreeViewViewModel.cs
│   │   │   └── Feedback/                       # 反馈类
│   │   │       ├── AlertViewModel.cs
│   │   │       ├── DrawerViewModel.cs
│   │   │       ├── MessageViewModel.cs
│   │   │       ├── ModalUserControlViewModel.cs
│   │   │       ├── ModalViewModel.cs
│   │   │       ├── NotificationViewModel.cs
│   │   │       ├── PopupConfirmViewModel.cs
│   │   │       ├── ProgressBarViewModel.cs
│   │   │       ├── ResultViewModel.cs
│   │   │       ├── SkeletonViewModel.cs
│   │   │       ├── SpinViewModel.cs
│   │   │       └── WatermarkViewModel.cs
│   │   │
│   │   ├── Views/                              # 所有 ShowCase View（按分类组织）
│   │   │   ├── General/                        # 每个 View 包含 .axaml + .axaml.cs
│   │   │   ├── Layout/
│   │   │   ├── Navigation/
│   │   │   ├── DataEntry/
│   │   │   ├── DataDisplay/
│   │   │   └── Feedback/
│   │   │
│   │   └── ShowCaseControls/                   # ShowCase 专用自定义控件
│   │       ├── ShowCaseControlsThemesProvider.cs
│   │       ├── ShowCaseControlsThemesProvider.axaml
│   │       └── Form/                           # Form ShowCase 专用控件
│   │           ├── Captcha.cs + CaptchaTheme.axaml
│   │           ├── Donation.cs + DonationTheme.axaml
│   │           ├── PhoneNumber.cs + PhoneNumberTheme.axaml
│   │           ├── PriceInput.cs + PriceInputTheme.axaml
│   │           └── FormThemes.axaml
│   │
│   └── GeneratedFiles/                         # ⚠️ 自动生成，禁止手动编辑
│       ├── Avalonia.Generators/                # Avalonia x:Name 生成器输出
│       └── AtomUI.Generator/                   # AtomUI Token/Language 生成器输出
│
├── AtomUIGallery.Desktop/                      # 桌面平台启动项目
│   ├── AtomUIGallery.Desktop.csproj            # 项目文件（WinExe 输出）
│   ├── Program.cs                              # 程序入口（Main → AppBuilder → 启动）
│   ├── GalleryApplication.cs                   # 继承 BaseGalleryApplication（主题/语言配置）
│   ├── Roots.xml                               # Trimmer 根描述符
│   ├── app.manifest                            # Windows 应用清单
│   ├── Assets/
│   │   ├── Images/                             # 平台图标（icns/ico/png）+ 安装包资源
│   │   └── License.rtf                         # 安装包许可证
│   ├── configs/                                # 安装包配置（AppImage/DMG/WiX）
│   │   ├── InstallerConfig.appimage.xml
│   │   ├── InstallerConfig.dmg.xml
│   │   └── InstallerConfig.wix.xml
│   └── scripts/
│       └── PublishToLocal.ps1                  # 本地发布脚本
│
└── AtomUIGallery.Icons.Desktop/                # Gallery 专用图标项目
    ├── AtomUIGallery.Icons.Desktop.csproj      # 项目文件
    ├── GalleryIconProvider.cs                  # IconProvider<DesktopIconKind> 实现
    ├── Properties/
    │   └── launchSettings.json
    └── Assets/
        └── Svg/Filled/                         # SVG 图标源文件
            └── alert.svg
```

---

## 统计摘要

| 类别 | 数量 |
|---|---|
| 项目总数 | 3 |
| ViewModel 文件 | 63（含 WorkspaceWindowViewModel + CaseNavigationViewModel + 61 个 ShowCase VM） |
| View 文件 | 约 67 对（.axaml + .cs） |
| 自定义 Control 文件 | 12（6 个控件，各含 .cs + Theme.axaml） |
| 本地化语言文件 | 4（CaseNavigationLang + WorkspaceWindowLang × 2 种语言） |
| ShowCase 专用控件 | 4 个 Form 控件（Captcha / Donation / PhoneNumber / PriceInput） |
| 静态资源文件 | ~40+ 张图片/SVG |

