# 08 - 风险与注意事项

## 🔴 高风险项

### 1. Source Generator 缓存问题

**风险等级**: 🔴 高  
**影响范围**: 所有 Token 属性修改、新增控件

**问题描述**:  
Source Generator 的输出缓存在 `obj/` 目录中。修改 Token 属性后，如果 IDE 或增量编译未正确触发重新生成，会导致：
- XAML 中引用的 Token 资源键不存在
- 编译错误或运行时样式缺失
- 新增 Token 属性不生效

**缓解措施**:
- 修改 Token 属性后执行 `dotnet clean && dotnet build`
- 删除 `obj/` 目录彻底清理
- 在 CI 中使用 `--no-incremental` 标志

### 2. 跨项目 Partial Class 一致性

**风险等级**: 🔴 高  
**影响范围**: 所有控件

**问题描述**:  
控件类在 `AtomUI.Controls`（抽象定义）和 `AtomUI.Desktop.Controls`（实现）之间使用 `partial class` 拆分。如果两个项目中的类名、命名空间不一致，会导致：
- 编译错误
- 属性定义与实现脱节
- 运行时 MissingMethodException

**缓解措施**:
- 严格遵守命名空间约定：两个项目使用相同命名空间
- 新增属性时先在 Controls 项目定义，再在 Desktop.Controls 实现
- Code Review 重点检查跨项目一致性

### 3. 主题注册遗漏

**风险等级**: 🔴 高  
**影响范围**: 新增控件

**问题描述**:  
新增控件的主题文件必须在 `DesktopControlThemesProvider.axaml` 中注册。遗漏注册会导致控件无样式，显示为空白。

**缓解措施**:
- 新增控件后检查 Provider 注册
- 在 Gallery 中验证新控件渲染
- 考虑添加编译时检查（Analyzer）

---

## 🟡 中风险项

### 4. Avalonia 版本耦合

**风险等级**: 🟡 中  
**影响范围**: 全项目

**问题描述**:  
AtomUI 深度依赖 Avalonia 11.x 的内部 API（如 `PseudoClasses`、`AvaloniaProperty`、`TemplateAppliedEventArgs` 等）。Avalonia 大版本升级可能破坏兼容性。

**缓解措施**:
- 锁定 Avalonia 版本范围
- 升级前全面测试
- 关注 Avalonia 破坏性变更日志
- 避免使用 Avalonia 内部/私有 API

### 5. Token 计算性能

**风险等级**: 🟡 中  
**影响范围**: 主题切换、大量控件场景

**问题描述**:  
主题切换时，所有控件的 Token 都需要重新计算并注入 ResourceDictionary。如果控件数量多，可能导致：
- 主题切换卡顿
- 内存占用增加（每个控件独立 Token 实例）

**缓解措施**:
- Token 计算保持轻量（纯赋值，无复杂计算）
- 避免在 CalculateTokenValues 中创建大量临时对象
- 考虑 Token 实例共享/缓存机制

### 6. GlobalToken 种子值硬编码

**风险等级**: 🟡 中  
**影响范围**: 主题定制

**问题描述**:  
GlobalToken 的默认种子值（如 `#1677FF`）硬编码在代码中。虽然用户可以通过 ThemeBuilder 自定义，但：
- 默认值分散在构造函数中
- 暗色主题的种子值也是硬编码
- 修改默认主题需要改源码

**缓解措施**:
- 将种子默认值集中到常量类
- 支持从配置文件加载种子值
- 文档化所有可自定义的种子 Token

### 7. XAML 资源键命名冲突

**风险等级**: 🟡 中  
**影响范围**: 全项目

**问题描述**:  
Token 资源键格式为 `{TokenClassName}{PropertyName}`（如 `ButtonTokenContentFontSize`）。如果不同控件的 Token 属性名相同，可能产生冲突。

**缓解措施**:
- 严格遵守 `{TokenClassName}{PropertyName}` 命名规则
- Source Generator 自动生成键名，避免手动编写
- 共享 Token 使用原始 Token 类名作为前缀

### 8. 原生互操作 (P/Invoke) 兼容性

**风险等级**: 🟡 中  
**影响范围**: FloatButton、Window 控件、跨平台运行

**问题描述**:  
`AtomUI.Native` 使用 P/Invoke 直接调用平台原生 API：
- **Windows**: `user32.dll` — 相对稳定，但 32/64 位 API 差异（`GetWindowLongPtr` vs `GetWindowLong`）
- **macOS**: Objective-C Runtime (`libobjc.A.dylib`) — Apple 可能更改私有 API 行为
- **Linux**: XCB Shape Extension — 依赖 X11，Wayland 下可能不工作

**缓解措施**:
- 所有原生调用包裹在 try-catch 中
- 使用 `[SupportedOSPlatform]` 标记确保编译时警告
- 运行时检查平台后再调用
- Wayland 环境需要适配新方案

### 9. 动画资源泄漏

**风险等级**: 🟡 中  
**影响范围**: 使用 IMotionAwareControl 的控件

**问题描述**:  
动画（Motion）使用 Avalonia 的 Animation/Transition 系统。如果控件在动画进行中被移除视觉树，可能导致：
- Animation 完成回调访问已释放控件
- DispatcherTimer 未停止
- 内存泄漏

**缓解措施**:
- 控件 DetachedFromVisualTree 时取消动画
- 使用 CancellationToken 控制动画生命周期
- 实现 IDisposable 释放动画资源

---

## 🟢 低风险项

### 10. 图标包体积

**风险等级**: 🟢 低  
**影响范围**: AtomUI.Icons.AntDesign

**问题描述**:  
Ant Design 图标包包含 800+ 图标，每个图标生成一个 C# 类。增加了编译时间和程序集大小。

**缓解措施**:
- 按需加载图标（Lazy Loading）
- 考虑拆分为 Filled/Outlined/Twotone 子包

### 11. 测试覆盖不足

**风险等级**: 🟢 低  
**影响范围**: 全项目

**问题描述**:  
当前测试项目覆盖有限，大量控件逻辑缺少自动化测试。

**缓解措施**:
- 优先为 Token 计算逻辑添加单元测试
- 为关键交互添加无头 UI 测试
- Gallery 手动测试作为补充

### 12. 国际化支持不完整

**风险等级**: 🟢 低  
**影响范围**: 日期/时间控件、表单验证

**问题描述**:  
部分控件的文本硬编码为中文或英文。`ILanguageProvider` 接口已定义但实现不完整。

**缓解措施**:
- 完善语言提供者实现
- 将所有用户可见文本提取到资源文件

---

## 📋 迭代注意事项清单

### 添加新控件时

- [ ] 在 `AtomUI.Controls` 创建抽象基类（如需要）
- [ ] 在 `AtomUI.Desktop.Controls` 创建控件实现
- [ ] 创建 Token 类，所有属性标记 `[TokenResourceKey]`
- [ ] 创建 ThemeConstants 类
- [ ] 创建 ThemeVariantCalculator 类
- [ ] 创建 Theme .axaml 文件
- [ ] 在 `DesktopControlThemesProvider.axaml` 注册主题
- [ ] 执行 `dotnet clean && dotnet build` 触发 Source Generator
- [ ] 验证生成的 `TokenKind` 枚举和 `TokenResourceConst` 类
- [ ] 在 Gallery 中添加展示页面
- [ ] 测试亮色/暗色主题切换
- [ ] 测试 SizeType 变体
- [ ] 测试 StyleVariant 变体（如有）

### 修改 Token 属性时

- [ ] 修改 Token 类中的属性定义
- [ ] 确认 `[TokenResourceKey]` 标记正确
- [ ] 更新 `CalculateTokenValues()` 中的计算逻辑
- [ ] 更新 XAML 中引用该 Token 的所有 Setter
- [ ] 执行 `dotnet clean && dotnet build`
- [ ] 验证 Source Generator 重新生成代码
- [ ] 测试主题切换

### 修改控件属性时

- [ ] 在正确的项目（Controls 或 Desktop.Controls）中修改
- [ ] 更新静态构造函数中的属性变更订阅
- [ ] 更新 PseudoClass 映射
- [ ] 更新 XAML 样式选择器
- [ ] 更新 Gallery 展示

### 升级 Avalonia 版本时

- [ ] 检查 Avalonia Release Notes 中的破坏性变更
- [ ] 更新 `Directory.Packages.props` 中的版本号
- [ ] 全量编译测试
- [ ] 重点测试：StyledProperty、PseudoClasses、ControlTheme、Flyout
- [ ] 测试跨平台运行（Windows/macOS/Linux）

### 修改 Source Generator 时

- [ ] 修改 `AtomUI.SourceGenerators` 项目
- [ ] 执行 `dotnet clean` 清理所有生成文件
- [ ] 重新编译验证生成代码正确
- [ ] 检查增量编译兼容性
- [ ] 运行 `AtomUI.Generator.Tests`

---

## 🔧 技术债清单

| # | 描述 | 优先级 | 预估工作量 |
|---|------|--------|-----------|
| 1 | 测试覆盖不足，核心 Token 计算缺少单元测试 | 中 | 2-3 天 |
| 2 | ILanguageProvider 实现不完整 | 低 | 1-2 天 |
| 3 | 部分控件内部类暴露为 public | 低 | 0.5 天 |
| 4 | 图标包按需加载机制缺失 | 低 | 1 天 |
| 5 | Token 计算中的硬编码魔法数字 | 低 | 0.5 天 |
| 6 | 部分 XAML 文件缺少 Design.PreviewWith | 低 | 0.5 天 |
| 7 | 控件文档注释不完整 | 低 | 持续 |
| 8 | CI/CD 流水线未配置 | 中 | 1-2 天 |

---

## 📊 性能关注点

| 场景 | 潜在瓶颈 | 优化建议 |
|------|----------|----------|
| 主题切换 | 所有 Token 重算 + ResourceDictionary 更新 | 批量更新，减少通知次数 |
| 大量控件初始化 | 每个控件独立 Token 实例 | 共享不可变 Token 实例 |
| 图标渲染 | PathIcon Data 解析 | 缓存 StreamGeometry |
| 动画执行 | 多控件同时动画 | 合并动画，使用 Composition 动画 |
| ScrollViewer | 大列表虚拟化 | 确保 ItemsControl 启用虚拟化 |
| TreeView | 大树展开/折叠 | 延迟加载子节点 |

---

## 🏗️ 架构演进建议

1. **考虑 Token 不可变性**: Token 对象当前为可变，改为不可变 + Builder 模式可提升线程安全性和缓存友好性
2. **统一控件注册机制**: 当前手动在 Provider 中注册，可改为自动扫描程序集
3. **增强编译时检查**: 添加 Roslyn Analyzer 检测常见错误（如遗漏 TokenResourceKey、未注册主题）
4. **完善无头测试**: 利用 Avalonia.Headless 建立控件视觉回归测试
5. **考虑 AOT 兼容性**: .NET 8 AOT 发布可能需要调整 Source Generator 和反射使用