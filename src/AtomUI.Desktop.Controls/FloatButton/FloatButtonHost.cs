using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

public class FloatButtonHost : Control,
                               IControlSharedTokenResourcesHost,
                               IMotionAwareControl
{
    #region 公共属性定义
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<FloatButtonHost>();
    
    public static readonly StyledProperty<Control?> ContentProperty =
        AvaloniaProperty.Register<StyledElement, Control?>(nameof(Content));
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    [Content]
    public Control? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
    #endregion
    
    #region 内部属性定义

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => FloatButtonToken.ID;

    #endregion

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ContentProperty)
        {
            if (Content != null)
            {
                if (Content is not FloatButton && Content is not FloatButtonGroup)
                {
                    throw new Exception("Content must be a FloatButtonGroup or FloatButton.");
                }
            }
        }
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        PrepareScopedAdornerLayer();
    }
    
    private void PrepareScopedAdornerLayer()
    {
        
    }
}