请帮我全面分析 `controlgallery` 这个项目的所有源码（它是 AtomUI 的演示程序）。分析的目标是为了后续能基于这份文档安全、高效地迭代 Gallery，因此需要详细、准确的信息。

请按以下步骤执行：

1. **参考资料**
   - 你需要仔细参考，.referenceprojects/Avalonia AtomUI 项目的底层技术
   - .referenceprojects/avalonia-docs Avalonia 文档
   - .referenceprojects/ReactiveUI/src ReactiveUI 框架源码，Gallery 整体使用的 mvvm 框架，非常重要
   - .referenceprojects/ReactiveUI/docs ReactiveUI 框架文档，非常重要，有助于你分析 Gallery 整体的代码结构。

2. **扫描文件结构**
   - 项目的源码在 controlgallery 下，一共三个 .NET 项目
   - 递归扫描该目录下的所有文件（包括隐藏文件、配置文件、源代码、资源文件等），列出完整的目录树。

3. **分析项目架构**
   - 模块划分与依赖关系
   - 主要的命名空间/包结构
   - 核心类/组件及其职责
   - 数据流与控制流（例如 MVVM、MVP 等模式）
   - 外部依赖（NuGet 包、第三方库、本地引用等）
   - 构建与运行方式（目标框架、启动项目、配置文件等）
   - 你可以** Mermaid 格式的架构图**，并附上文字说明。

4. **识别技术栈**
   - 编程语言及版本（如 C# 10.0）
   - 框架与运行时（.NET / .NET Core / .NET Framework、WPF / WinUI / MAUI 等）
   - UI 技术（XAML、样式系统、控件库）
   - 构建工具（MSBuild、dotnet CLI）
   - 测试框架（如果有）
   - 日志、依赖注入、序列化等常用库

5. **归纳编码规范**
   - 命名约定（PascalCase, camelCase, 私有字段前缀等）
   - 代码组织方式（文件结构、区域、分部类）
   - 注释与文档规范
   - 设计模式使用情况（例如工厂、单例、观察者等）
   - 异常处理与资源管理习惯
   - 任何明显遵循的内部或公共编码标准（如 Microsoft 官方建议）

6. **输出要求**
   - 我需要你尽可能的输出文档，越详细越好
   - 将所有分析结果整理成 **Markdown 文档**, 你需要按照需要进行拆分。
   - 文档保存路径：`docs/Gallery` 文件夹下。
   - 如果路径无写入权限，请输出到当前会话的最终回答中。
   - 文档必须包含：目录树、架构图（Mermaid 代码块）、技术栈清单、编码规范摘要，以及**任何潜在风险点或后续迭代注意事项**（如硬编码、性能瓶颈、遗留技术债等）。

请开始分析，如果遇到无法访问的文件或需要更多权限，请明确告知我。
