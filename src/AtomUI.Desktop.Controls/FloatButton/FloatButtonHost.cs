using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Data;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

public class FloatButtonHost : TemplatedControl,
                               IControlSharedTokenResourcesHost,
                               IMotionAwareControl
{
    #region 公共属性定义
    public static readonly StyledProperty<FloatButtonPlacement> PlacementProperty =
        FloatButton.PlacementProperty.AddOwner<FloatButtonHost>();
    
    public static readonly StyledProperty<double> FloatOffsetXProperty =
        FloatButton.FloatOffsetXProperty.AddOwner<FloatButtonHost>();
    
    public static readonly StyledProperty<double> FloatOffsetYProperty =
        FloatButton.FloatOffsetYProperty.AddOwner<FloatButtonHost>();
    
    public static readonly StyledProperty<PathIcon?> IconProperty =
        FloatButton.IconProperty.AddOwner<FloatButtonHost>();
    
    public static readonly StyledProperty<string?> TooltipProperty =
        FloatButton.TooltipProperty.AddOwner<FloatButtonHost>();
    
    public static readonly StyledProperty<Color?> TooltipColorProperty =
        FloatButton.TooltipColorProperty.AddOwner<FloatButtonHost>();
    
    public static readonly StyledProperty<object?> DescriptionProperty =
        AvaloniaProperty.Register<FloatButtonHost, object?>(nameof(Description));
    
    public static readonly StyledProperty<IDataTemplate?> DescriptionTemplateProperty =
        AvaloniaProperty.Register<FloatButtonHost, IDataTemplate?>(nameof(DescriptionTemplate));
    
    public static readonly StyledProperty<FloatButtonType> ButtonTypeProperty =
        FloatButton.ButtonTypeProperty.AddOwner<FloatButtonHost>();
    
    public static readonly StyledProperty<FloatButtonShape> ShapeProperty =
        FloatButton.ShapeProperty.AddOwner<FloatButtonHost>();
    
    public static readonly StyledProperty<Uri?> HrefProperty =
        FloatButton.HrefProperty.AddOwner<FloatButtonHost>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        FloatButton.IsMotionEnabledProperty.AddOwner<FloatButtonHost>();
    
    public FloatButtonPlacement Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }
    
    public double FloatOffsetX
    {
        get => GetValue(FloatOffsetXProperty);
        set => SetValue(FloatOffsetXProperty, value);
    }

    public double FloatOffsetY
    {
        get => GetValue(FloatOffsetYProperty);
        set => SetValue(FloatOffsetYProperty, value);
    }
    
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    public string? Tooltip
    {
        get => GetValue(TooltipProperty);
        set => SetValue(TooltipProperty, value);
    }
    
    public Color? TooltipColor
    {
        get => GetValue(TooltipColorProperty);
        set => SetValue(TooltipColorProperty, value);
    }
    
    [DependsOn(nameof(DescriptionTemplate))]
    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
    
    public IDataTemplate? DescriptionTemplate
    {
        get => GetValue(DescriptionTemplateProperty);
        set => SetValue(DescriptionTemplateProperty, value);
    }
    
    public FloatButtonType ButtonType
    {
        get => GetValue(ButtonTypeProperty);
        set => SetValue(ButtonTypeProperty, value);
    }
    
    public FloatButtonShape Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }
    
    public Uri? Href
    {
        get => GetValue(HrefProperty);
        set => SetValue(HrefProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    #endregion
    
    #region 内部属性定义

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => FloatButtonToken.ID;

    #endregion

    private protected FloatButton? FloatButton;
    private protected CompositeDisposable? Disposables;
    private ScopeAwareOverlayLayer? _overlayLayer;
    
    public FloatButtonHost()
    {
        this.RegisterResources();
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        PrepareScopedAdornerLayer();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        CleanupScopedAdornerLayer();
    }

    private void PrepareScopedAdornerLayer()
    {
        _overlayLayer = ScopeAwareOverlayLayer.GetLayer(this);
        Disposables?.Dispose();
        Disposables =   new CompositeDisposable();
        FloatButton ??= NotifyCreateFloatButton(Disposables);
        _overlayLayer?.Children.Add(FloatButton);
    }

    private void CleanupScopedAdornerLayer()
    {
        if (FloatButton != null)
        {
            _overlayLayer = ScopeAwareOverlayLayer.FindLayer(this);
            _overlayLayer?.Children.Remove(FloatButton);
            Disposables?.Dispose();
            Disposables = null;
            FloatButton = null;
        }
    }
    
    protected virtual FloatButton NotifyCreateFloatButton(CompositeDisposable disposables)
    {
        var floatButton = new FloatButton();
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
        return floatButton;
    }
}