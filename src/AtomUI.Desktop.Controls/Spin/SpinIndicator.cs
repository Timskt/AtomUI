using AtomUI.Controls;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Styling;
using Avalonia.Controls;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls;

public class SpinIndicator : AbstractSpinIndicator
{
    public SpinIndicator()
    {
        this.RegisterTokenResourceScope(SpinToken.ScopeProvider);
        ConfigureInstanceStyles();
    }
    
     private void ConfigureInstanceStyles()
    {
        {
            var middleStyle = new Style(x =>
                x.PropertyEquals(SizeTypeProperty, SizeType.Middle));
            var iconStyle = new Style(x => Selectors.Or(
                x.Nesting().Descendant().Name("PART_CustomIndicatorPresenter").Child()
                 .OfType<PathIcon>(),
                x.Nesting().Descendant().Name("PART_CustomIndicatorPresenter").Child()
                 .OfType<Icon>()));
            iconStyle.Add(WidthProperty, SpinTokenKind.IndicatorSize);
            iconStyle.Add(HeightProperty, SpinTokenKind.IndicatorSize);
            middleStyle.Add(iconStyle);
            Styles.Add(middleStyle);
        }
        {
            var smallStyle = new Style(x =>
                x.PropertyEquals(SizeTypeProperty, SizeType.Small));
            var iconStyle = new Style(x => Selectors.Or(
                x.Nesting().Descendant().Name("PART_CustomIndicatorPresenter").Child()
                 .OfType<PathIcon>(),
                x.Nesting().Descendant().Name("PART_CustomIndicatorPresenter").Child()
                 .OfType<Icon>()));
            iconStyle.Add(WidthProperty, SpinTokenKind.IndicatorSizeSM);
            iconStyle.Add(HeightProperty, SpinTokenKind.IndicatorSizeSM);
            smallStyle.Add(iconStyle);
            Styles.Add(smallStyle);
        }
        {
            var largeStyle = new Style(x =>
                x.PropertyEquals(SizeTypeProperty, SizeType.Large));
            var iconStyle = new Style(x => Selectors.Or(
                x.Nesting().Descendant().Name("PART_CustomIndicatorPresenter").Child()
                 .OfType<PathIcon>(),
                x.Nesting().Descendant().Name("PART_CustomIndicatorPresenter").Child()
                 .OfType<Icon>()));
            iconStyle.Add(WidthProperty, SpinTokenKind.IndicatorSizeLG);
            iconStyle.Add(HeightProperty, SpinTokenKind.IndicatorSizeLG);
            largeStyle.Add(iconStyle);
            Styles.Add(largeStyle);
        }
    }
}