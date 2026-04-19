using AtomUI.Controls;
using AtomUI.Theme.Styling;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AtomUI.Animations;

public class TransitionProvider : MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var transitions = new Transitions();
        transitions.Add(TransitionUtils.CreateTransition<SolidColorBrushTransition>(Border.BorderBrushProperty, SharedTokenKind.MotionDurationMid));
        return transitions;
    }
}