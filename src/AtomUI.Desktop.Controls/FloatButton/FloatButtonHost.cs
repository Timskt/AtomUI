using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

public class FloatButtonHost : TemplatedControl, IMotionAwareControl
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
    
    public static readonly StyledProperty<bool> IsBadgeEnabledProperty =
        FloatButton.IsBadgeEnabledProperty.AddOwner<FloatButtonHost>();
    
    public static readonly StyledProperty<bool> IsDotBadgeProperty =
        FloatButton.IsDotBadgeProperty.AddOwner<FloatButtonHost>();
    
    public static readonly StyledProperty<int> BadgeCountProperty =
        FloatButton.BadgeCountProperty.AddOwner<FloatButtonHost>();
    
    public static readonly StyledProperty<string?> BadgeColorProperty =
        FloatButton.BadgeColorProperty.AddOwner<FloatButtonHost>();
    
    public static readonly StyledProperty<Point> BadgeOffsetProperty =
        FloatButton.BadgeOffsetProperty.AddOwner<FloatButtonHost>();

    public static readonly StyledProperty<int> BadgeOverflowCountProperty =
        FloatButton.BadgeOverflowCountProperty.AddOwner<FloatButtonHost>();
    
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
    
    public bool IsBadgeEnabled
    {
        get => GetValue(IsBadgeEnabledProperty);
        set => SetValue(IsBadgeEnabledProperty, value);
    }

    public bool IsDotBadge
    {
        get => GetValue(IsDotBadgeProperty);
        set => SetValue(IsDotBadgeProperty, value);
    }
    
    public int BadgeCount
    {
        get => GetValue(BadgeCountProperty);
        set => SetValue(BadgeCountProperty, value);
    }
    
    public string? BadgeColor
    {
        get => GetValue(BadgeColorProperty);
        set => SetValue(BadgeColorProperty, value);
    }
    
    public Point BadgeOffset
    {
        get => GetValue(BadgeOffsetProperty);
        set => SetValue(BadgeOffsetProperty, value);
    }

    public int BadgeOverflowCount
    {
        get => GetValue(BadgeOverflowCountProperty);
        set => SetValue(BadgeOverflowCountProperty, value);
    }
    #endregion

    private protected FloatButton? FloatButton;
    private protected CompositeDisposable? Disposables;
    private ScopeAwareOverlayLayer? _overlayLayer;
    
    public FloatButtonHost()
    {
        this.RegisterTokenResourceScope(FloatButtonToken.ScopeProvider);
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        PrepareScopedOverlayLayer();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        CleanupScopedOverlayLayer();
    }

    private void PrepareScopedOverlayLayer()
    {
        _overlayLayer = ScopeAwareOverlayLayer.GetLayer(this);
        Disposables?.Dispose();
        Disposables =   new CompositeDisposable();
        FloatButton ??= NotifyCreateFloatButton(Disposables);
        _overlayLayer?.Children.Add(FloatButton);
    }

    private void CleanupScopedOverlayLayer()
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

        floatButton[!FloatButton.IconProperty]               = this[!IconProperty];
        floatButton[!FloatButton.TooltipProperty]            = this[!TooltipProperty];
        floatButton[!FloatButton.TooltipColorProperty]       = this[!TooltipColorProperty];
        floatButton[!FloatButton.ButtonTypeProperty]         = this[!ButtonTypeProperty];
        floatButton[!FloatButton.ShapeProperty]              = this[!ShapeProperty];
        floatButton[!FloatButton.HrefProperty]               = this[!HrefProperty];
        floatButton[!FloatButton.IsMotionEnabledProperty]    = this[!IsMotionEnabledProperty];
        floatButton[!FloatButton.PlacementProperty]          = this[!PlacementProperty];
        floatButton[!FloatButton.FloatOffsetXProperty]       = this[!FloatOffsetXProperty];
        floatButton[!FloatButton.FloatOffsetYProperty]       = this[!FloatOffsetYProperty];
        floatButton[!FloatButton.ContentProperty]            = this[!DescriptionProperty];
        floatButton[!FloatButton.ContentTemplateProperty]    = this[!DescriptionTemplateProperty];
        floatButton[!FloatButton.IsBadgeEnabledProperty]     = this[!IsBadgeEnabledProperty];
        floatButton[!FloatButton.IsDotBadgeProperty]         = this[!IsDotBadgeProperty];
        floatButton[!FloatButton.BadgeCountProperty]         = this[!BadgeCountProperty];
        floatButton[!FloatButton.BadgeColorProperty]         = this[!BadgeColorProperty];
        floatButton[!FloatButton.BadgeOffsetProperty]        = this[!BadgeOffsetProperty];
        floatButton[!FloatButton.BadgeOverflowCountProperty] = this[!BadgeOverflowCountProperty];
    
        return floatButton;
    }
}