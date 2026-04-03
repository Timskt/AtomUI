using AtomUI.Theme.Styling;
using Avalonia;
using Avalonia.Styling;

namespace AtomUI.Controls;

public static class WaveSpiritAwareControlExtensions
{
    public static void ConfigureWaveSpiritBindingStyle(this IWaveSpiritAwareControl waveSpiritAwareControl)
    {
        if (waveSpiritAwareControl is StyledElement styledElement)
        {
            var bindingStyle = new Style();
            bindingStyle.Add(MotionAwareControlProperty.IsMotionEnabledProperty, SharedTokenKind.EnableMotion);
            bindingStyle.Add(WaveSpiritAwareControlProperty.IsWaveSpiritEnabledProperty, SharedTokenKind.EnableWaveSpirit);
            styledElement.Styles.Add(bindingStyle);
        }
    }
}