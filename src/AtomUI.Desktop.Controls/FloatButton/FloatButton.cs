using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
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
                           IMotionAwareControl,
                           IControlSharedTokenResourcesHost
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

    #endregion

    #region 内部属性定义
    internal static readonly DirectProperty<FloatButton, bool> IsEmbedModeProperty =
        AvaloniaProperty.RegisterDirect<FloatButton, bool>(
            nameof(IsEmbedMode),
            o => o.IsEmbedMode,
            (o, v) => o.IsEmbedMode = v);

    private bool _isEmbedMode;

    internal bool IsEmbedMode
    {
        get => _isEmbedMode;
        set => SetAndRaise(IsEmbedModeProperty, ref _isEmbedMode, value);
    }
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => FloatButtonToken.ID;

    #endregion
    
    ScopeAwareOverlayLayer? _overlayLayer;
    
    public FloatButton()
    {
        this.RegisterResources();
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
            CalculatePosition(_overlayLayer.DesiredSize);
        }
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
        }
        else if (change.Property == FloatOffsetXProperty ||
                 change.Property == FloatOffsetYProperty)
        {
            if (_overlayLayer != null)
            {
                CalculatePosition(_overlayLayer.DesiredSize);
            }
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
        CalculatePosition(e.NewSize);
    }

    private void CalculatePosition(Size layerSize)
    {
        var width   = DesiredSize.Width;
        var height  = DesiredSize.Height;
        var offsetX = 0.0d;
        var offsetY = 0.0d;
        if (Placement == FloatButtonPlacement.Left)
        {
            offsetY =  (layerSize.Height - height) / 2;
            offsetX += FloatOffsetX;
            offsetY += FloatOffsetY;
        }
        else if (Placement == FloatButtonPlacement.Right)
        {
            offsetX =  (layerSize.Width - width);
            offsetY =  (layerSize.Height - height) / 2;
            offsetX -= FloatOffsetX;
            offsetY += FloatOffsetY;
        }
        else if (Placement == FloatButtonPlacement.Top)
        {
            offsetX =  (layerSize.Width - width) / 2;
            offsetX += FloatOffsetX;
            offsetY += FloatOffsetY;
        }
        else if (Placement == FloatButtonPlacement.Bottom)
        {
            offsetX =  (layerSize.Width - width) / 2;
            offsetY =  (layerSize.Height - height);
            offsetX += FloatOffsetX;
            offsetY -= FloatOffsetY;
        }
        else if (Placement == FloatButtonPlacement.TopLeft)
        {
            offsetX =  0.0d;
            offsetY =  0.0d;
            offsetX += FloatOffsetX;
            offsetY += FloatOffsetY;
        }
        else if (Placement == FloatButtonPlacement.TopRight)
        {
            offsetX =  (layerSize.Width - width);
            offsetY =  0.0d;
            offsetX -= FloatOffsetX;
            offsetY += FloatOffsetY;
        }
        else if (Placement == FloatButtonPlacement.BottomLeft)
        {
            offsetX =  0.0d;
            offsetY =  (layerSize.Height - height);
            offsetX += FloatOffsetX;
            offsetY -= FloatOffsetY;
        }
        else if (Placement == FloatButtonPlacement.BottomRight)
        {
            offsetX =  (layerSize.Width - width);
            offsetY =  (layerSize.Height - height);
            offsetX -= FloatOffsetX;
            offsetY -= FloatOffsetY;
        }
        Canvas.SetLeft(this, offsetX);
        Canvas.SetTop(this, offsetY);
    }
}