using AtomUIGallery.ShowCases.ViewModels;
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
    
}