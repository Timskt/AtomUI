using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class TourViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Tour";

    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = ID.ToString();

    public TourViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}