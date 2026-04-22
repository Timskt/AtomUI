using AtomUI;
using AtomUI.Desktop.Controls;
using AtomUIGallery.ShowCases.ViewModels;
using Avalonia;
using Avalonia.Interactivity;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.ShowCases.Views;

public partial class TourShowCase : ReactiveUserControl<TourViewModel>
{
    public TourShowCase()
    {
        this.WhenActivated(disposables =>
        {
            if (DataContext is TourViewModel vm)
            {
                vm.CustomGapRadius = 2;
            }
        });
        InitializeComponent();
    }

    private void HandleBasicBeginTour(object? sender, RoutedEventArgs args)
    {
        if (DataContext is TourViewModel vm)
        {
            vm.BasicCaseTourOpened = !vm.BasicCaseTourOpened;
        }
    }
    
    private void HandleNonMaskBeginTour(object? sender, RoutedEventArgs args)
    {
        if (DataContext is TourViewModel vm)
        {
            vm.NonMaskTourOpened = !vm.NonMaskTourOpened;
        }
    }
    
    private void HandlePlacementBeginTour(object? sender, RoutedEventArgs args)
    {
        if (DataContext is TourViewModel vm)
        {
            vm.PlacementTourOpened = !vm.PlacementTourOpened;
        }
    }

    private void HandleCustomIndicatorBeginTour(object? sender, RoutedEventArgs args)
    {
        if (DataContext is TourViewModel vm)
        {
            vm.CustomIndicatorTourOpened = !vm.CustomIndicatorTourOpened;
        }
    }
    
    private void HandleCustomMaskBeginTour(object? sender, RoutedEventArgs args)
    {
        if (DataContext is TourViewModel vm)
        {
            vm.CustomMaskTourOpened = !vm.CustomMaskTourOpened;
        }
    }
    
    private void HandleCustomGapBeginTour(object? sender, RoutedEventArgs args)
    {
        if (DataContext is TourViewModel vm)
        {
            vm.CustomGapTourOpened = !vm.CustomGapTourOpened;
        }
    }
    
    private void HandleCustomActionBeginTour(object? sender, RoutedEventArgs args)
    {
        if (DataContext is TourViewModel vm)
        {
            vm.CustomActionTourOpened = !vm.CustomActionTourOpened;
        }
    }
}

public class SkipTourActionButton : Button, ITourAction
{
    public static readonly StyledProperty<int> StepCountProperty = 
        AvaloniaProperty.Register<SkipTourActionButton, int>(nameof(StepCount));
    
    public static readonly StyledProperty<int> ActiveIndexProperty = 
        AvaloniaProperty.Register<SkipTourActionButton, int>(nameof(ActiveIndex));

    public static readonly StyledProperty<TourStyleType> StyleTypeProperty =
        Tour.StyleTypeProperty.AddOwner<SkipTourActionButton>();
    
    public int StepCount
    {
        get => GetValue(StepCountProperty);
        set => SetValue(StepCountProperty, value);
    }
    
    public int ActiveIndex
    {
        get => GetValue(ActiveIndexProperty);
        set => SetValue(ActiveIndexProperty, value);
    }
    
    public TourStyleType StyleType
    {
        get => GetValue(StyleTypeProperty);
        set => SetValue(StyleTypeProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(Button);

    private Tour? _tour;

    static SkipTourActionButton()
    {
        SizeTypeProperty.OverrideDefaultValue<SkipTourActionButton>(SizeType.Small);
        ButtonTypeProperty.OverrideDefaultValue<SkipTourActionButton>(ButtonType.Default);
    }

    void ITourAction.NotifyAttached(Tour tour)
    {
        _tour = tour;
    }

    protected override void OnClick()
    {
        base.OnClick();
        if (_tour != null)
        {
            _tour.SetCurrentValue(Tour.IsOpenProperty, false);
        }
    }
}