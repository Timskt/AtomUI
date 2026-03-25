using System.Collections.Specialized;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Utils;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

using ControlList = Avalonia.Controls.Controls;

public enum FloatButtonGroupMenuPlacement
{
    Top,
    Bottom,
    Left,
    Right
}

public enum FloatButtonGroupTrigger
{
    Default,
    Click,
    Hover
}

public class FloatButtonGroup : TemplatedControl,
                                IMotionAwareControl,
                                IControlSharedTokenResourcesHost
{
    #region 公共属性定义
    public static readonly StyledProperty<FloatButtonPlacement> PlacementProperty =
        FloatButton.PlacementProperty.AddOwner<FloatButtonGroup>();
    
    public static readonly StyledProperty<double> FloatOffsetXProperty =
        FloatButton.FloatOffsetXProperty.AddOwner<FloatButtonGroup>();
    
    public static readonly StyledProperty<double> FloatOffsetYProperty =
        FloatButton.FloatOffsetYProperty.AddOwner<FloatButtonGroup>();
    
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<FloatButtonGroup, PathIcon?>(nameof(Icon));
    
    public static readonly StyledProperty<PathIcon?> CloseIconProperty =
        AvaloniaProperty.Register<FloatButtonGroup, PathIcon?>(nameof(CloseIcon));
    
    public static readonly StyledProperty<FloatButtonType> ButtonTypeProperty =
        FloatButton.ButtonTypeProperty.AddOwner<FloatButtonGroup>();

    public static readonly StyledProperty<FloatButtonShape> ShapeProperty =
        FloatButton.ShapeProperty.AddOwner<FloatButtonGroup>();
    
    public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
        Border.BoxShadowProperty.AddOwner<FloatButtonGroup>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<FloatButtonGroup>();
    
    public static readonly StyledProperty<FloatButtonGroupMenuPlacement> MenuPlacementProperty =
        AvaloniaProperty.Register<FloatButtonGroup, FloatButtonGroupMenuPlacement>(nameof(MenuPlacement), FloatButtonGroupMenuPlacement.Top);
    
    public static readonly StyledProperty<FloatButtonGroupTrigger> TriggerProperty =
        AvaloniaProperty.Register<FloatButtonGroup, FloatButtonGroupTrigger>(nameof(Trigger), FloatButtonGroupTrigger.Default);
    
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<FloatButtonGroup, bool>(nameof(IsOpen));
    
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
    
    public PathIcon? CloseIcon
    {
        get => GetValue(CloseIconProperty);
        set => SetValue(CloseIconProperty, value);
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
    
    public FloatButtonGroupMenuPlacement MenuPlacement
    {
        get => GetValue(MenuPlacementProperty);
        set => SetValue(MenuPlacementProperty, value);
    }
    
    public FloatButtonGroupTrigger Trigger
    {
        get => GetValue(TriggerProperty);
        set => SetValue(TriggerProperty, value);
    }
    
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    [Content] 
    public ControlList Children { get; } = new ();
    
    #endregion

    #region 公共事件定义
    public static readonly RoutedEvent<RoutedEventArgs> ClickedEvent =
        RoutedEvent.Register<FloatButtonGroup, RoutedEventArgs>(nameof(Clicked), RoutingStrategies.Bubble);
    
    public static readonly RoutedEvent<RoutedEventArgs> ClosedEvent =
        RoutedEvent.Register<FloatButtonGroup, RoutedEventArgs>(nameof(Closed), RoutingStrategies.Bubble);
    
    public static readonly RoutedEvent<RoutedEventArgs> OpenedEvent =
        RoutedEvent.Register<FloatButtonGroup, RoutedEventArgs>(nameof(Opened), RoutingStrategies.Bubble);
    
    public event EventHandler<RoutedEventArgs>? Clicked
    {
        add => AddHandler(ClickedEvent, value);
        remove => RemoveHandler(ClickedEvent, value);
    }
    
    public event EventHandler<RoutedEventArgs>? Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }
    
    public event EventHandler<RoutedEventArgs>? Opened
    {
        add => AddHandler(OpenedEvent, value);
        remove => RemoveHandler(OpenedEvent, value);
    }

    #endregion

    #region 内部属性定义

    internal event EventHandler? OpenRequest;
    internal event EventHandler? CloseRequest;

    #endregion
    
    #region 内部属性定义

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => FloatButtonToken.ID;

    #endregion
    
    private FloatButtonItemsControl? _itemsControl;
    private readonly Dictionary<object, CompositeDisposable> _itemsBindingDisposables = new();
    private bool _initPressed;
    private IDisposable? _clickTriggerDisposable;
    private FloatButton? _triggerButton;
    ScopeAwareOverlayLayer? _overlayLayer;
    
    static FloatButtonGroup()
    {
        AffectsMeasure<FloatButtonGroup>(IsOpenProperty);
    }

    public FloatButtonGroup()
    {
        this.RegisterResources();
        Children.CollectionChanged += NotifyChildrenChanged;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (Icon == null)
        {
            SetCurrentValue(IconProperty, new FileTextOutlined());
        }

        if (CloseIcon == null)
        {
            SetCurrentValue(CloseIconProperty, new CloseOutlined());
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TriggerProperty)
        {
            ConfigureTriggerType();
        }
        else if (change.Property == ParentProperty)
        {
            SetupParentLayer(Parent);
            if (_overlayLayer != null)
            {
                FloatButton.CalculatePosition(this, _overlayLayer.Bounds.Size, Placement, FloatOffsetX, FloatOffsetY);
            }
        }
        else if (change.Property == FloatOffsetXProperty ||
                 change.Property == FloatOffsetYProperty ||
                 change.Property == PlacementProperty)
        {
            if (_overlayLayer != null)
            {
                FloatButton.CalculatePosition(this, _overlayLayer.Bounds.Size, Placement, FloatOffsetX, FloatOffsetY);
            }
        }
        else if (change.Property == IsOpenProperty)
        {
            CalculateItemsControlPosition();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_triggerButton != null)
        {
            _triggerButton.PointerEntered -= HandlePointerEntered;
            _triggerButton.PointerExited  -= HandlePointerExited;
        }
        _itemsControl  = e.NameScope.Find<FloatButtonItemsControl>("ItemsControl");
        _triggerButton = e.NameScope.Find<FloatButton>("Trigger");

        if (_triggerButton != null)
        {
            _triggerButton.PointerEntered += HandlePointerEntered;
            _triggerButton.PointerExited  += HandlePointerExited;
        }
        
        if (_itemsControl != null)
        {
            _itemsControl.IsTriggerMode = Trigger != FloatButtonGroupTrigger.Default;
        }
        
        _itemsControl?.Children.AddRange(Children);
        ConfigureTriggerType();
        foreach (var item in Children)
        {
            if (item is FloatButton floatButton)
            {
                NotifyAddItem(floatButton);
            }
        }
    }

    protected virtual void NotifyChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_itemsControl != null)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var newItems = e.NewItems!.OfType<Control>().ToList();
                    foreach (var item in newItems)
                    {
                        if (item is FloatButton floatButton)
                        {
                            NotifyAddItem(floatButton);
                        }
                    }
                    _itemsControl.Children.InsertRange(e.NewStartingIndex, newItems);
                    break;

                case NotifyCollectionChangedAction.Move:
                    _itemsControl.Children.MoveRange(e.OldStartingIndex, e.OldItems!.Count, e.NewStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    var oldItems = e.OldItems!.OfType<Control>().ToList();
                    foreach (var item in oldItems)
                    {
                        if (item is FloatButton floatButton)
                        {
                            floatButton.SetCurrentValue(FloatButton.IsEmbedModeProperty, false);
                            if (_itemsBindingDisposables.TryGetValue(floatButton, out var disposable))
                            {
                                disposable.Dispose();
                                _itemsBindingDisposables.Remove(floatButton);
                            }
                        }
                    }
                    _itemsControl.Children.RemoveAll(oldItems);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    for (var i = 0; i < e.OldItems!.Count; ++i)
                    {
                        var index = i + e.OldStartingIndex;
                        var child = (Control)e.NewItems![i]!;
                        _itemsControl.Children[index] = child;
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    throw new NotSupportedException();
            }
        }
    }

    protected void NotifyAddItem(FloatButton floatButton)
    {
        var disposables = new CompositeDisposable(4);
        
        disposables.Add(BindUtils.RelayBind(this, ShapeProperty, floatButton, FloatButton.ShapeProperty));
        disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, floatButton, FloatButton.IsMotionEnabledProperty));
        floatButton.SetCurrentValue(FloatButton.IsEmbedModeProperty, true);
        if (_itemsBindingDisposables.TryGetValue(floatButton, out var oldDisposables))
        {
            oldDisposables.Dispose();
            _itemsBindingDisposables.Remove(floatButton);
        }
        _itemsBindingDisposables.Add(floatButton, disposables);
    }

    private void HandlePointerEntered(object? sender, PointerEventArgs? e)
    {
        if (Trigger == FloatButtonGroupTrigger.Hover)
        {
            SetValue(IsOpenProperty, true, BindingPriority.Style);
            if (_overlayLayer != null)
            {
                OpenRequest?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    
    private void HandlePointerExited(object? sender, PointerEventArgs e)
    {
        base.OnPointerExited(e);
        if (Trigger == FloatButtonGroupTrigger.Hover)
        {
            SetValue(IsOpenProperty, false, BindingPriority.Style);
            if (_overlayLayer != null)
            {
                CloseRequest?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _clickTriggerDisposable?.Dispose();
        _clickTriggerDisposable = null;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        ConfigureTriggerType();
    }

    private void ConfigureTriggerType()
    {
        _clickTriggerDisposable?.Dispose();
        if (Trigger == FloatButtonGroupTrigger.Click)
        {
            var inputManager = AvaloniaLocator.Current.GetService<IInputManager>()!;
            _clickTriggerDisposable = inputManager.Process.Subscribe(HandleMouseClick);
        }

        if (_itemsControl != null)
        {
            _itemsControl.IsTriggerMode = Trigger != FloatButtonGroupTrigger.Default;
        }
    }

    private void HandleMouseClick(RawInputEventArgs args)
    {
        if (args is RawPointerEventArgs pointerEventArgs)
        {
            if (pointerEventArgs.Type == RawPointerEventType.LeftButtonDown)
            {
                if (pointerEventArgs.IsPointLogicalIn(this))
                {
                    _initPressed = true;
                }
            }
            else if (pointerEventArgs.Type == RawPointerEventType.LeftButtonUp)
            {
                if (pointerEventArgs.IsPointLogicalIn(this) || pointerEventArgs.IsPointLogicalIn(_itemsControl))
                {
                    if (_initPressed && pointerEventArgs.IsPointLogicalIn(_triggerButton))
                    {
                        SetValue(IsOpenProperty, !IsOpen, BindingPriority.Style);
                        if (_overlayLayer != null)
                        {
                            if (!IsOpen)
                            {
                                OpenRequest?.Invoke(this, EventArgs.Empty);
                            }
                            else
                            {
                                CloseRequest?.Invoke(this, EventArgs.Empty);
                            }
                        }
                    }
                }
                else
                {
                    SetValue(IsOpenProperty, false, BindingPriority.Style);
                    if (_overlayLayer != null)
                    {
                        CloseRequest?.Invoke(this, EventArgs.Empty);
                    }
                }
                _initPressed = false;
            }
        }
    }
    
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        if (Shape == FloatButtonShape.Circle)
        {
            SetCurrentValue(CornerRadiusProperty, new CornerRadius(e.NewSize.Height / 2));
        }

        if (_overlayLayer != null)
        {
            FloatButton.CalculatePosition(this, _overlayLayer.Bounds.Size, Placement, FloatOffsetX, FloatOffsetY);
        }
    }
    
     private void SetupParentLayer(StyledElement? parent)
    {
        if (_overlayLayer != null)
        {
            _overlayLayer.SizeChanged -= HandleLayerSizeChanged;
        }
        if (parent is ScopeAwareOverlayLayer scopeAwareOverlayLayer)
        {
            _overlayLayer                      =  scopeAwareOverlayLayer;
            scopeAwareOverlayLayer.SizeChanged += HandleLayerSizeChanged;
        }
        else
        {
            _overlayLayer = null;
        }
    }

    private void HandleLayerSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        FloatButton.CalculatePosition(this, e.NewSize, Placement, FloatOffsetX, FloatOffsetY);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Trigger == FloatButtonGroupTrigger.Default)
        {
            return base.MeasureOverride(availableSize);
        }
        var size = base.MeasureOverride(availableSize);
        return _triggerButton?.DesiredSize ?? size; 
    }

    private void CalculateItemsControlPosition()
    {
        if (Trigger == FloatButtonGroupTrigger.Default)
        {
            return;
        }
        if (_itemsControl == null || _triggerButton == null)
        {
            return;
        }

        var offsetX = 0.0d;
        var offsetY = 0.0d;
        var width   = DesiredSize.Width;
        var height = DesiredSize.Height;
        
        LayoutHelper.MeasureChild(_itemsControl, new Size(double.PositiveInfinity, double.PositiveInfinity), new Thickness(0));
        
        if (IsOpen)
        {
            if (MenuPlacement == FloatButtonGroupMenuPlacement.Top)
            {
                offsetY = -_itemsControl.DesiredSize.Height;
            }
            else if (MenuPlacement == FloatButtonGroupMenuPlacement.Bottom)
            {
                offsetY = height;
            }
            else if (MenuPlacement == FloatButtonGroupMenuPlacement.Left)
            {
                offsetX = -_itemsControl.DesiredSize.Width;
            }
            else if (MenuPlacement == FloatButtonGroupMenuPlacement.Right)
            {
                offsetX = width;
            }
        }
        Canvas.SetLeft(_itemsControl, offsetX);
        Canvas.SetTop(_itemsControl, offsetY);
    }
}