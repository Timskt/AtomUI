using System.Reactive.Disposables;
using AtomUI.Controls;
using Avalonia;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaScrollViewer = Avalonia.Controls.ScrollViewer;

public class BackTopFloatButtonHost : FloatButtonHost
{
    #region 公共属性定义

    public static readonly StyledProperty<TimeSpan> ToTopDurationProperty =
        BackTopFloatButton.ToTopDurationProperty.AddOwner<BackTopFloatButtonHost>();
    
    public static readonly StyledProperty<AvaScrollViewer?> TargetProperty =
        BackTopFloatButton.TargetProperty.AddOwner<BackTopFloatButtonHost>();
    
    public static readonly StyledProperty<double> VisibilityHeightProperty =
        BackTopFloatButton.VisibilityHeightProperty.AddOwner<BackTopFloatButtonHost>();
    
    public static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<BackTopFloatButtonHost>();

    public TimeSpan ToTopDuration
    {
        get => GetValue(ToTopDurationProperty);
        set => SetValue(ToTopDurationProperty, value);
    }
    
    public AvaScrollViewer? Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }
    
    public double VisibilityHeight
    {
        get => GetValue(VisibilityHeightProperty);
        set => SetValue(VisibilityHeightProperty, value);
    }
    
    public TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }
    #endregion
    
    protected override FloatButton NotifyCreateFloatButton(CompositeDisposable disposables)
    {
        var floatButton = new BackTopFloatButton();
        
        floatButton[!BackTopFloatButton.IconProperty]             = this[!IconProperty];
        floatButton[!BackTopFloatButton.TooltipProperty]          = this[!TooltipProperty];
        floatButton[!BackTopFloatButton.TooltipColorProperty]     = this[!TooltipColorProperty];
        floatButton[!BackTopFloatButton.ButtonTypeProperty]       = this[!ButtonTypeProperty];
        floatButton[!BackTopFloatButton.ShapeProperty]            = this[!ShapeProperty];
        floatButton[!BackTopFloatButton.HrefProperty]             = this[!HrefProperty];
        floatButton[!BackTopFloatButton.IsMotionEnabledProperty]  = this[!IsMotionEnabledProperty];
        floatButton[!BackTopFloatButton.PlacementProperty]        = this[!PlacementProperty];
        floatButton[!BackTopFloatButton.FloatOffsetXProperty]     = this[!FloatOffsetXProperty];
        floatButton[!BackTopFloatButton.FloatOffsetYProperty]     = this[!FloatOffsetYProperty];
        floatButton[!BackTopFloatButton.ToTopDurationProperty]    = this[!ToTopDurationProperty];
        floatButton[!BackTopFloatButton.TargetProperty]           = this[!TargetProperty];
        floatButton[!BackTopFloatButton.VisibilityHeightProperty] = this[!VisibilityHeightProperty];
        floatButton[!BackTopFloatButton.MotionDurationProperty]   = this[!MotionDurationProperty];
        
        return floatButton;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Target ??= this.FindAncestorOfType<AvaScrollViewer>();
    }
}