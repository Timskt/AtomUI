using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls.Primitives;

internal class IndicatorScrollViewer : ScrollViewer
{
    public IndicatorScrollViewer()
    {
        this.RegisterTokenResourceScope(IndicatorScrollViewerToken.ScopeProvider);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(true);
            }
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
    
    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions =
                [
                    TransitionUtils.CreateTransition<DoubleTransition>(ScrollBarOpacityProperty)
                ];
            }
        }
        else
        {
            Transitions = null;
        }
    }
}