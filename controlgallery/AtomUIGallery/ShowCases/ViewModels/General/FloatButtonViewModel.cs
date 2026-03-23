using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class FloatButtonViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "FloatButton";

    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = ID.ToString();

    public FloatButtonViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}