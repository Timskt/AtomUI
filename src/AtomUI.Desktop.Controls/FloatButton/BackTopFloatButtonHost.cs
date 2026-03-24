using System.Reactive.Disposables;
using AtomUI.Data;
using Avalonia;

namespace AtomUI.Desktop.Controls;

public class BackTopFloatButtonHost : FloatButtonHost
{
    #region 公共属性定义

    public static readonly StyledProperty<TimeSpan> DurationProperty =
        BackTopFloatButton.DurationProperty.AddOwner<BackTopFloatButtonHost>();
    
    public static readonly StyledProperty<ScrollViewer?> TargetProperty =
        BackTopFloatButton.TargetProperty.AddOwner<BackTopFloatButtonHost>();
    
    public static readonly StyledProperty<double> VisibilityHeightProperty =
        BackTopFloatButton.VisibilityHeightProperty.AddOwner<BackTopFloatButtonHost>();

    public TimeSpan Duration
    {
        get => GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }
    
    public ScrollViewer? Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }
    
    public double VisibilityHeight
    {
        get => GetValue(VisibilityHeightProperty);
        set => SetValue(VisibilityHeightProperty, value);
    }
    #endregion
    
    protected override FloatButton NotifyCreateFloatButton(CompositeDisposable disposables)
    {
        var floatButton = new BackTopFloatButton();
        disposables.Add(BindUtils.RelayBind(this, IconProperty, floatButton, IconProperty));
        disposables.Add(BindUtils.RelayBind(this, TooltipProperty, floatButton, TooltipProperty));
        disposables.Add(BindUtils.RelayBind(this, TooltipColorProperty, floatButton, TooltipColorProperty));
        disposables.Add(BindUtils.RelayBind(this, ButtonTypeProperty, floatButton, ButtonTypeProperty));
        disposables.Add(BindUtils.RelayBind(this, ShapeProperty, floatButton, ShapeProperty));
        disposables.Add(BindUtils.RelayBind(this, HrefProperty, floatButton, HrefProperty));
        disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, floatButton, IsMotionEnabledProperty));
        disposables.Add(BindUtils.RelayBind(this, PlacementProperty, floatButton, PlacementProperty));
        disposables.Add(BindUtils.RelayBind(this, FloatOffsetXProperty, floatButton, FloatOffsetXProperty));
        disposables.Add(BindUtils.RelayBind(this, FloatOffsetXProperty, floatButton, FloatOffsetXProperty));
        disposables.Add(BindUtils.RelayBind(this, DurationProperty, floatButton, DurationProperty));
        disposables.Add(BindUtils.RelayBind(this, TargetProperty, floatButton, TargetProperty));
        disposables.Add(BindUtils.RelayBind(this, VisibilityHeightProperty, floatButton, VisibilityHeightProperty));
        return floatButton;
    }
}