using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AtomUI.Reflection;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace AtomUI.Data;

internal static class DynamicResourceReflectionExtension
{
    #region 反射信息定义

    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicFields, typeof(DynamicResourceExtension))]
    private static readonly Lazy<FieldInfo> AnchorFieldInfo = new Lazy<FieldInfo>(
        () => typeof(DynamicResourceExtension).GetFieldInfoOrThrow("_anchor",
            BindingFlags.Instance | BindingFlags.NonPublic));

    #endregion

    public static void SetAnchor(this DynamicResourceExtension dynamicResourceExtension, object? anchor)
    {
        AnchorFieldInfo.Value.SetValue(dynamicResourceExtension, anchor);
    }
}