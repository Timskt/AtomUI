using Avalonia;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

internal interface ICompactSpaceAware
{
    void NotifyPositionChange(SpaceItemPosition? position);
    void NotifyOrientationChange(Orientation orientation);

    bool IsAlwaysActiveZIndex() => false;
    bool IgnoreZIndexChange() => false;

    double GetBorderThickness();
}

internal abstract class CompactSpaceAwareControlProperty
{
    // 附加属性没有对应的 CLR 属性声明，只能使用硬编码字符串注册
    public static readonly StyledProperty<SpaceItemPosition?> CompactSpaceItemPositionProperty = 
        AvaloniaProperty.Register<StyledElement, SpaceItemPosition?>("CompactSpaceItemPosition");
    
    // 附加属性没有对应的 CLR 属性声明，只能使用硬编码字符串注册
    public static readonly StyledProperty<Orientation> CompactSpaceOrientationProperty = 
        AvaloniaProperty.Register<StyledElement, Orientation>("CompactSpaceOrientation");
    
    // 附加属性没有对应的 CLR 属性声明，只能使用硬编码字符串注册
    public static readonly StyledProperty<bool> IsUsedInCompactSpaceProperty = 
        AvaloniaProperty.Register<StyledElement, bool>("IsUsedInCompactSpace", defaultValue: false);
}