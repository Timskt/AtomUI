using System.Collections.Specialized;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

using ControlList = Avalonia.Controls.Controls;

public enum FloatButtonGroupPlacement
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
    public static readonly StyledProperty<FloatButtonType> ButtonTypeProperty =
        FloatButton.ButtonTypeProperty.AddOwner<FloatButtonGroup>();

    public static readonly StyledProperty<FloatButtonShape> ShapeProperty =
        FloatButton.ShapeProperty.AddOwner<FloatButtonGroup>();
    
    public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
        Border.BoxShadowProperty.AddOwner<FloatButtonGroup>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<FloatButtonGroup>();
    
    public static readonly StyledProperty<FloatButtonGroupPlacement> PlacementProperty =
        AvaloniaProperty.Register<FloatButtonGroup, FloatButtonGroupPlacement>(nameof(Placement), FloatButtonGroupPlacement.Top);
    
    public static readonly StyledProperty<FloatButtonGroupTrigger> TriggerProperty =
        AvaloniaProperty.Register<FloatButtonGroup, FloatButtonGroupTrigger>(nameof(Trigger), FloatButtonGroupTrigger.Default);
    
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<FloatButtonGroup, bool>(nameof(IsOpen));
    
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
    
    public FloatButtonGroupPlacement Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
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
    
    internal static readonly StyledProperty<Orientation> OrientationProperty =
        FloatButtonItemsControl.OrientationProperty.AddOwner<FloatButtonItemsControl>();
    
    internal Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => FloatButtonToken.ID;

    #endregion
    
    private FloatButtonItemsControl? _itemsControl;
    private readonly Dictionary<object, CompositeDisposable> _itemsBindingDisposables = new();
    
    public FloatButtonGroup()
    {
        this.RegisterResources();
        Children.CollectionChanged += NotifyChildrenChanged;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == PlacementProperty)
        {
            ConfigureOrientation();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _itemsControl = e.NameScope.Find<FloatButtonItemsControl>("ItemsControl");
        _itemsControl?.Children.AddRange(Children);
        ConfigureOrientation();
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

    private void ConfigureOrientation()
    {
        if (Placement == FloatButtonGroupPlacement.Top || Placement == FloatButtonGroupPlacement.Bottom)
        {
            SetCurrentValue(OrientationProperty, Orientation.Vertical);
        }
        else
        {
            SetCurrentValue(OrientationProperty, Orientation.Horizontal);
        }
    }
}