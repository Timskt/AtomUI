using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class TourViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Tour";

    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = ID.ToString();
    
    private bool _basicCaseTourOpened;

    public bool BasicCaseTourOpened
    {
        get => _basicCaseTourOpened;
        set => this.RaiseAndSetIfChanged(ref _basicCaseTourOpened, value);
    }
    
    private bool _nonMaskTourOpened;

    public bool NonMaskTourOpened
    {
        get => _nonMaskTourOpened;
        set => this.RaiseAndSetIfChanged(ref _nonMaskTourOpened, value);
    }
    
    private bool _placementTourOpened;

    public bool PlacementTourOpened
    {
        get => _placementTourOpened;
        set => this.RaiseAndSetIfChanged(ref _placementTourOpened, value);
    }
    
    private IList<ITourStepOption>? _basicCaseSteps;

    public IList<ITourStepOption>? BasicCaseSteps
    {
        get => _basicCaseSteps;
        set => this.RaiseAndSetIfChanged(ref _basicCaseSteps, value);
    }

    public TourViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}