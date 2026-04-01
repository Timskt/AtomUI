using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

using AvaloniaButton = Avalonia.Controls.Button;
using ToolTipControl = AtomUI.Desktop.Controls.ToolTip;

public enum FloatButtonType
{
    Default,
    Primary
}

public enum FloatButtonShape
{
    Circle,
    Square
}

[PseudoClasses(ButtonPseudoClass.IconOnly)]
public class FloatButton : AvaloniaButton,
                           IMotionAwareControl
{
    #region 公共属性定义
    public static readonly StyledProperty<FloatButtonPlacement> PlacementProperty =
        AvaloniaProperty.Register<FloatButton, FloatButtonPlacement>(nameof(Placement));
    
    public static readonly StyledProperty<double> FloatOffsetXProperty =
        AvaloniaProperty.Register<FloatButton, double>(nameof(FloatOffsetX));
    
    public static readonly StyledProperty<double> FloatOffsetYProperty =
        AvaloniaProperty.Register<FloatButton, double>(nameof(FloatOffsetY));
    
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<FloatButton, PathIcon?>(nameof(Icon));
    
    public static readonly StyledProperty<string?> TooltipProperty =
        AvaloniaProperty.Register<FloatButton, string?>(nameof(Tooltip));
    
    public static readonly StyledProperty<Color?> TooltipColorProperty =
        AvaloniaProperty.Register<FloatButton, Color?>(nameof(TooltipColor));
    
    public static readonly StyledProperty<FloatButtonType> ButtonTypeProperty =
        AvaloniaProperty.Register<FloatButton, FloatButtonType>(nameof(ButtonType), FloatButtonType.Default);
    
    public static readonly StyledProperty<FloatButtonShape> ShapeProperty =
        AvaloniaProperty.Register<FloatButton, FloatButtonShape>(nameof(Shape), FloatButtonShape.Circle);
    
    public static readonly StyledProperty<Uri?> HrefProperty =
        AvaloniaProperty.Register<FloatButton, Uri?>(nameof(Href));
    
    public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
        Border.BoxShadowProperty.AddOwner<FloatButton>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<FloatButton>();
    
    public static readonly StyledProperty<bool> IsBadgeEnabledProperty =
        AvaloniaProperty.Register<FloatButton, bool>(nameof(IsBadgeEnabled));
    
    public static readonly StyledProperty<bool> IsDotBadgeProperty =
        AvaloniaProperty.Register<FloatButton, bool>(nameof(IsDotBadge));
    
    public static readonly StyledProperty<int> BadgeCountProperty =
        AvaloniaProperty.Register<FloatButton, int>(nameof(BadgeCount));
    
    public static readonly StyledProperty<string?> BadgeColorProperty =
        AvaloniaProperty.Register<FloatButton, string?>(
            nameof(BadgeColor));
    
    public static readonly StyledProperty<Point> BadgeOffsetProperty =
        AvaloniaProperty.Register<FloatButton, Point>(nameof(BadgeOffset));

    public static readonly StyledProperty<int> BadgeOverflowCountProperty =
        AvaloniaProperty.Register<FloatButton, int>(nameof(BadgeOverflowCount), 99,
            coerce: (o, v) => Math.Max(0, v));
    
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
    
    public BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
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

    #region 内部属性定义
    internal static readonly DirectProperty<FloatButton, bool> IsEmbedModeProperty =
        AvaloniaProperty.RegisterDirect<FloatButton, bool>(
            nameof(IsEmbedMode),
            o => o.IsEmbedMode,
            (o, v) => o.IsEmbedMode = v);
    
    internal static readonly StyledProperty<IBrush?> BadgeEffectiveColorProperty =
        AvaloniaProperty.Register<FloatButton, IBrush?>(
            nameof(BadgeEffectiveColor));

    private bool _isEmbedMode;

    internal bool IsEmbedMode
    {
        get => _isEmbedMode;
        set => SetAndRaise(IsEmbedModeProperty, ref _isEmbedMode, value);
    }
    
    internal IBrush? BadgeEffectiveColor
    {
        get => GetValue(BadgeEffectiveColorProperty);
        set => SetValue(BadgeEffectiveColorProperty, value);
    }

    #endregion
    
    private ScopeAwareOverlayLayer? _overlayLayer;
    private Canvas? _badgeLayout;
    private Control? _badge;
    
    public FloatButton()
    {
        this.RegisterTokenResourceScope(FloatButtonToken.ScopeProvider);
        UpdatePseudoClasses();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        if (Shape == FloatButtonShape.Circle)
        {
            SetCurrentValue(CornerRadiusProperty, new CornerRadius(e.NewSize.Height / 2));
        }

        if (IsEmbedMode && _overlayLayer != null)
        {
            CalculatePosition(this, _overlayLayer.Bounds.Size, Placement, FloatOffsetX, FloatOffsetY);
        }
        CalculateBadgePosition();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _badgeLayout = e.NameScope.Find<Canvas>("BadgeLayout");
        ConfigureBadge();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var size = base.MeasureOverride(availableSize);
        var effectSize = Math.Max(size.Width, size.Height);
        return new Size(effectSize, effectSize);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IconProperty ||
            change.Property == ContentProperty ||
            change.Property == ShapeProperty)
        {
            UpdatePseudoClasses();
        }

        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(true);
            }
        }

        if (change.Property == TooltipProperty)
        {
            ToolTipControl.SetTip(this, Tooltip);
        }
        else if (change.Property == TooltipColorProperty)
        {
            ToolTipControl.SetColor(this, TooltipColor);
        }
        else if (change.Property == IsEmbedModeProperty ||
                 change.Property == ParentProperty)
        {
            SetupParentLayer(Parent);
            if (_overlayLayer != null)
            {
                CalculatePosition(this, _overlayLayer.Bounds.Size, Placement, FloatOffsetX, FloatOffsetY);
            }
        }
        else if (change.Property == FloatOffsetXProperty ||
                 change.Property == FloatOffsetYProperty ||
                 change.Property == PlacementProperty)
        {
            if (_overlayLayer != null)
            {
                CalculatePosition(this, _overlayLayer.Bounds.Size, Placement, FloatOffsetX, FloatOffsetY);
            }
        }
        else if (change.Property == IsBadgeEnabledProperty ||
                 change.Property == IsDotBadgeProperty)

        {
            ConfigureBadge();
        }
        else if (change.Property == BadgeOffsetProperty)
        {
            CalculateBadgePosition();
        }
        else if (change.Property == BadgeColorProperty)
        {
            SetCurrentValue(BadgeEffectiveColorProperty, BadgeColorUtils.CalculateColor(BadgeColor));
        }
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ConfigureDefaultIcon();
    }

    private void UpdatePseudoClasses()
    {
        if (Shape == FloatButtonShape.Square)
        {
            PseudoClasses.Set(ButtonPseudoClass.IconOnly, Icon is not null && Content is null);
        }
        else
        {
            PseudoClasses.Set(ButtonPseudoClass.IconOnly, true);
        }
    }
    
    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions = [TransitionUtils.CreateTransition<SolidColorBrushTransition>(BackgroundProperty)];
            }
        }
        else
        {
            Transitions = null;
        }
    }
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureTransitions(false);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Transitions = null;
    }

    protected virtual void ConfigureDefaultIcon()
    {
        if ((Shape == FloatButtonShape.Circle && Icon == null) ||
            (Shape == FloatButtonShape.Square && Content == null && Icon == null))
        {
            SetCurrentValue(IconProperty, new FileTextOutlined());
        }
    }

    private void SetupParentLayer(StyledElement? parent)
    {
        if (_overlayLayer != null)
        {
            _overlayLayer.SizeChanged -= HandleLayerSizeChanged;
        }
        if (!IsEmbedMode)
        {
            if (parent is ScopeAwareOverlayLayer scopeAwareOverlayLayer)
            {
                _overlayLayer                      =  scopeAwareOverlayLayer;
                scopeAwareOverlayLayer.SizeChanged += HandleLayerSizeChanged;
            }
        }
        else
        {
            _overlayLayer = null;
        }
        
    }

    private void HandleLayerSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        CalculatePosition(this, e.NewSize, Placement, FloatOffsetX, FloatOffsetY);
    }

    private void ConfigureBadge()
    {
        if (_badgeLayout != null)
        {
            if (IsBadgeEnabled)
            {
                if (IsDotBadge)
                {
                    var dotBadge = new DotBadgeAdorner();
                    dotBadge[!DotBadgeAdorner.BadgeDotColorProperty]   = this[!BadgeEffectiveColorProperty];
                    dotBadge[!DotBadgeAdorner.IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
                    _badge                                             = dotBadge;
                }
                else
                {
                    var countBadge = new CountBadgeAdorner();
                    countBadge[!CountBadgeAdorner.BadgeColorProperty]      = this[!BadgeEffectiveColorProperty];
                    countBadge[!CountBadgeAdorner.IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
                    countBadge[!CountBadgeAdorner.CountProperty]           = this[!BadgeCountProperty];
                    countBadge[!CountBadgeAdorner.OverflowCountProperty]   = this[!BadgeOverflowCountProperty];
                    _badge                                                 = countBadge;
                }
                
                _badgeLayout.Children.Add(_badge);
                CalculateBadgePosition();
            }
            else
            {
                if (_badge != null)
                {
                    _badgeLayout.Children.Remove(_badge);
                }
                _badge = null;
            }
        }
    }

    private void CalculateBadgePosition()
    {
        if (IsBadgeEnabled && _badge != null)
        {
            var targetOffsetX = DesiredSize.Width;
            var targetOffsetY = 0.0d;
            targetOffsetX += BadgeOffset.X;
            targetOffsetY += BadgeOffset.Y;
            Canvas.SetLeft(_badge, targetOffsetX);
            Canvas.SetTop(_badge, targetOffsetY);
        }
    }

    internal static void CalculatePosition(Control floatControl, 
                                           Size layerSize, 
                                           FloatButtonPlacement placement,
                                           double offsetX,
                                           double offsetY)
    {
        var width   = floatControl.DesiredSize.Width;
        var height  = floatControl.DesiredSize.Height;
        var targetOffsetX = 0.0d;
        var targetOffsetY = 0.0d;
        if (placement == FloatButtonPlacement.Left)
        {
            targetOffsetY =  (layerSize.Height - height) / 2;
            targetOffsetX += offsetX;
            targetOffsetY += offsetY;
        }
        else if (placement == FloatButtonPlacement.Right)
        {
            targetOffsetX =  (layerSize.Width - width);
            targetOffsetY =  (layerSize.Height - height) / 2;
            targetOffsetX -= offsetX;
            targetOffsetY += offsetY;
        }
        else if (placement == FloatButtonPlacement.Top)
        {
            targetOffsetX =  (layerSize.Width - width) / 2;
            targetOffsetX += offsetX;
            targetOffsetY += offsetY;
        }
        else if (placement == FloatButtonPlacement.Bottom)
        {
            targetOffsetX =  (layerSize.Width - width) / 2;
            targetOffsetY =  (layerSize.Height - height);
            targetOffsetX += offsetX;
            targetOffsetY -= offsetY;
        }
        else if (placement == FloatButtonPlacement.TopLeft)
        {
            targetOffsetX =  0.0d;
            targetOffsetY =  0.0d;
            targetOffsetX += offsetX;
            targetOffsetY += offsetY;
        }
        else if (placement == FloatButtonPlacement.TopRight)
        {
            targetOffsetX =  (layerSize.Width - width);
            targetOffsetY =  0.0d;
            targetOffsetX -= offsetX;
            targetOffsetY += offsetY;
        }
        else if (placement == FloatButtonPlacement.BottomLeft)
        {
            targetOffsetX =  0.0d;
            targetOffsetY =  (layerSize.Height - height);
            targetOffsetX += offsetX;
            targetOffsetY -= offsetY;
        }
        else if (placement == FloatButtonPlacement.BottomRight)
        {
            targetOffsetX =  (layerSize.Width - width);
            targetOffsetY =  (layerSize.Height - height);
            targetOffsetX -= offsetX;
            targetOffsetY -= offsetY;
        }
        else if (placement == FloatButtonPlacement.Center)
        {
            targetOffsetX =  (layerSize.Width - width) / 2;
            targetOffsetY =  (layerSize.Height - height) / 2;
            targetOffsetX += offsetX;
            targetOffsetY += offsetY;
        }
        Canvas.SetLeft(floatControl, targetOffsetX);
        Canvas.SetTop(floatControl, targetOffsetY);
    }
}