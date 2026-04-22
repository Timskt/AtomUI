# AtomUI Gallery — 风险点、技术债与迭代注意事项

> **文档版本**：2026-04-15

---

## 1. 技术债与已知问题

### 1.1 ⚠️ 文件命名不规范

以下文件使用了非标准扩展名，可能导致 IDE 关联错误或构建问题：

| 文件 | 问题 | 建议 |
|---|---|---|
| `FormShowCase.axmal.cs` | `.axmal` 拼写错误（应为 `.axaml`） | 重命名为 `FormShowCase.axaml.cs` |
| `RateShowCase.axmal.cs` | 同上 | 重命名为 `RateShowCase.axaml.cs` |
| `TourShowCase.axmal.cs` | 同上 | 重命名为 `TourShowCase.axaml.cs` |
| `ResultShowCase.cs.axaml` | `.cs.axaml` 顺序反转（应为 `.axaml` 单独文件） | 重命名为 `ResultShowCase.axaml`，代码后置改为 `ResultShowCase.axaml.cs` |
| `CustomizeThemeShowCase.cs` | 代码后置直接 `.cs`（对应 `.axaml` 存在） | 重命名为 `CustomizeThemeShowCase.axaml.cs` |

> **风险**：在某些 IDE 或工具链中，文件关联可能断裂，导致 XAML 设计器无法正常工作。

### 1.2 ⚠️ IActivatableViewModel 使用不一致

在 67 个 ShowCase ViewModel 中：
- **仅 8 个**实现了 `IActivatableViewModel`（AboutUs、Button、OsInfo、Avatar、Badge、AutoComplete、Cascader、CheckBox）
- **其余 59 个**未实现

然而，**所有 View** 都在构造函数中调用了 `this.WhenActivated(disposables => { ... })`。

**影响**：
- 对于未实现 `IActivatableViewModel` 的 ViewModel，`WhenActivated` 仍然可以在 View 端正常工作（因为 `ReactiveUserControl` 自身支持激活）
- 但如果未来需要在 ViewModel 端监听激活/反激活事件，需要逐一添加 `IActivatableViewModel`

**建议**：统一规范 — 要么所有 ViewModel 都实现 `IActivatableViewModel`，要么都不实现。

### 1.3 ⚠️ 双重注册机制

每新增一个 ShowCase 需要在 **三个位置** 手动注册：
1. `ShowCaseViewModule.RegisterViews()` — VM → View 映射
2. `CaseNavigationViewModel.RegisterShowCaseViewModels()` — VM 工厂
3. `CaseNavigation.axaml` — 导航菜单项

加上本地化资源（`en_US.cs` + `zh_CN.cs`），总计 **5 个文件** 需要同步修改。

**风险**：遗漏任何一处都会导致运行时错误或功能缺失。

**建议**：未来可考虑使用源代码生成器或反射注册自动化此流程。但鉴于当前 AOT 兼容性要求，显式注册是正确选择。

### 1.4 ⚠️ Roots.xml 中的过时引用

`AtomUIGallery.Desktop/Roots.xml` 中包含过时的程序集名称：

```xml
<assembly fullname="AtomUI.Demo.Desktop" preserve="All"/>  <!-- 应为 AtomUIGallery.Desktop -->
<assembly fullname="AtomUI.IconPkg" preserve="All"/>       <!-- 应为 AtomUI.Icons.Shared 或已不存在 -->
```

**建议**：修正为正确的程序集名称。

### 1.5 ⚠️ LinuxDistributionDetector 代码量过大

`LinuxDistributionDetector.cs` 共 **380 行**，包含大量平台特定检测逻辑。该工具类仅被 OsInfo ShowCase 使用。

**建议**：考虑是否可以简化或使用第三方库替代。

### 1.6 ⚠️ CommunityToolkit.Mvvm 使用不充分

项目引用了 `CommunityToolkit.Mvvm` 包（8.4.2），但实际使用较少，核心 MVVM 依赖 ReactiveUI。

**建议**：
- 如果不需要 CommunityToolkit.Mvvm 的功能，移除引用以减少依赖
- 如果需要，明确其使用场景并统一规范

---

## 2. 架构风险点

### 2.1 反射使用（AOT 兼容性）

以下位置使用了反射，在 AOT/NativeAOT 环境下需要特别注意：

| 位置 | 反射用途 | AOT 风险 |
|---|---|---|
| `IconGallery.ReLoadIcons()` | 通过 `CachedLoadedAssemblyTypeScanner` 扫描 `AtomUI.Icons.AntDesign` 程序集中所有 `Icon` 子类 | ⚠️ 高 — 需要 Trimmer 保留 |
| `GalleryIconProvider.GetTypeForKind()` | `Type.GetType()` + `Assembly.GetType()` 反射加载图标类型 | ⚠️ 高 |
| `EnumExtension.ProvideValue()` | `Enum.GetValuesAsUnderlyingType()` | 低风险 |

**缓解措施**：`Roots.xml` 中已标记相关程序集 `preserve="All"`，但需确保列表准确。

### 2.2 每次导航创建新 View 实例

`ShowCaseViewModule` 中的工厂每次都创建新的 View：
```csharp
locator.Map<ButtonViewModel, ButtonShowCase>(() => new ButtonShowCase());
```

同时 `CaseNavigationViewModel.NavigateTo()` 每次都创建新的 ViewModel。

**影响**：
- 内存：旧 View/ViewModel 依赖 GC 回收
- 性能：复杂 ShowCase（如 DataGrid、IconGallery）每次切换都需重新初始化
- 状态丢失：切换回已访问的 ShowCase 时所有状态重置

**建议**：对于性能敏感的 ShowCase，可考虑添加 ViewModel/View 缓存策略。

### 2.3 事件处理与 Dispose 不一致

部分 ShowCase View（如 `ModalShowCase`）在 `WhenActivated` 中正确使用 `CompositeDisposable` 管理事件订阅生命周期：

```csharp
this.WhenActivated(disposables =>
{
    BasicOpenModalButton.Click += HandleBasicModalButtonClick;
    disposables.Add(Disposable.Create(() => BasicOpenModalButton.Click -= HandleBasicModalButtonClick));
});
```

但其他 ShowCase View 虽然调用了 `WhenActivated`，却只是空回调 `disposables => { }`。

**风险**：如果 View 中直接在构造函数外订阅事件且未取消，可能导致内存泄漏。

---

## 3. 构建与发布注意事项

### 3.1 Debug vs Release 依赖差异

- **Debug**：使用 `<ProjectReference>` 直接引用本地 AtomUI 源码（便于调试）
- **Release**：使用 `<PackageReference>` 引用 NuGet 包（发布独立可用）

修改 AtomUI 控件源码后，必须确保 Debug 和 Release 都能正常编译。

### 3.2 源代码生成器输出

- `GeneratedFiles/` 目录会自动由 Roslyn 生成器填充
- `<Compile Remove="$(CompilerGeneratedFilesOutputPath)/**/*.cs"/>` 确保不会重复编译
- **禁止手动编辑** `GeneratedFiles/` 下的文件

### 3.3 安装包构建

`AtomUIGallery.Desktop` 包含多平台安装包配置：
- **macOS**：DMG（`InstallerConfig.dmg.xml`）
- **Windows**：WiX MSI（`InstallerConfig.wix.xml`）
- **Linux**：AppImage（`InstallerConfig.appimage.xml`）

发布脚本：`scripts/PublishToLocal.ps1`

---

## 4. 性能注意事项

### 4.1 图标画廊加载

`IconGallery` 使用反射扫描 + `FrozenSet` 缓存，首次加载时会扫描整个 `AtomUI.Icons.AntDesign` 程序集。虽然有 5 秒缓存超时机制，但首次加载可能较慢。

### 4.2 自动遍历测试

F5 自动遍历以 300ms 间隔切换页面，由于每次创建新 View/ViewModel，长时间运行可能导致内存增长。这是预期行为（用于压力测试），但需关注 GC 表现。

---

## 5. 迭代开发检查清单

新增或修改 ShowCase 时，务必完成以下检查：

- [ ] ViewModel 正确实现 `IRoutableViewModel`，`ID` 唯一
- [ ] View 继承 `ReactiveUserControl<TViewModel>`，构造函数中调用 `WhenActivated` + `InitializeComponent`
- [ ] `ShowCaseViewModule.RegisterViews()` 中注册 VM → View 映射
- [ ] `CaseNavigationViewModel` 中注册 VM 工厂
- [ ] `CaseNavigation.axaml` 中添加导航菜单项
- [ ] `en_US.cs` 和 `zh_CN.cs` 中添加本地化字符串
- [ ] 亮色主题下正常显示
- [ ] 暗色主题下正常显示
- [ ] 紧凑模式下正常显示
- [ ] 中文/英文语言切换正常
- [ ] F5 自动遍历不崩溃
- [ ] `ShowCaseItem` 的 `Title` 和 `Description` 准确描述功能
- [ ] 文件命名遵循 `.axaml` + `.axaml.cs` 标准（不要写错为 `.axmal`）

---

## 6. 未来改进建议

| 优先级 | 建议 | 说明 |
|---|---|---|
| 🔴 高 | 修复文件命名错误 | 修正 `.axmal` 拼写错误和 `.cs.axaml` 顺序反转 |
| 🔴 高 | 修正 `Roots.xml` 过时引用 | 确保 Trimmer 配置正确 |
| 🟡 中 | 统一 `IActivatableViewModel` 使用 | 要么全部实现，要么全部移除 |
| 🟡 中 | 考虑 ViewModel/View 缓存策略 | 避免重复创建复杂控件 |
| 🟢 低 | 添加 ShowCase 自动注册 | 减少手动维护成本（需平衡 AOT 兼容性） |
| 🟢 低 | 简化 `LinuxDistributionDetector` | 考虑使用第三方库或精简逻辑 |


