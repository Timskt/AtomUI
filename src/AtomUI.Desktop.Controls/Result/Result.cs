using AtomUI.Controls;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Styling;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls;

using IconControl = AtomUI.Controls.Icon;

public class Result : AbstractResult
{
    public Result()
    {
        this.RegisterTokenResourceScope(ResultToken.ScopeProvider);
        ConfigureInstanceStyle();
    }
    
    private void ConfigureInstanceStyle()
    {
        {
            var iconStyle = new Style(x => x.Is<AbstractResult>().Descendant().Name("PART_StatusIconPresenter").Child());
            iconStyle.Add(WidthProperty, ResultTokenKind.IconSize);
            iconStyle.Add(HeightProperty, ResultTokenKind.IconSize);
            Styles.Add(iconStyle);
        }
        
        var infoStyle = new Style(x => x.PropertyEquals(StatusProperty, ResultStatus.Info));
        
        {
            var iconStyle = new Style(x => x.Nesting().Descendant().Name("PART_StatusIconPresenter").Child());
            iconStyle.Add(ForegroundProperty, ResultTokenKind.ResultInfoIconColor);
            iconStyle.Add(IconControl.FillBrushProperty, ResultTokenKind.ResultInfoIconColor);
            iconStyle.Add(IconControl.StrokeBrushProperty, ResultTokenKind.ResultInfoIconColor);
            infoStyle.Add(iconStyle);
        }
        
        Styles.Add(infoStyle);
        
        var successStyle = new Style(x => x.PropertyEquals(StatusProperty, ResultStatus.Success));
        
        {
            var iconStyle = new Style(x => x.Nesting().Descendant().Name("PART_StatusIconPresenter").Child());
            iconStyle.Add(ForegroundProperty, ResultTokenKind.ResultSuccessIconColor);
            iconStyle.Add(IconControl.FillBrushProperty, ResultTokenKind.ResultSuccessIconColor);
            iconStyle.Add(IconControl.StrokeBrushProperty, ResultTokenKind.ResultSuccessIconColor);
            successStyle.Add(iconStyle);
        }
        
        Styles.Add(successStyle);
        
        var warningStyle = new Style(x => x.PropertyEquals(StatusProperty, ResultStatus.Warning));
        
        {
            var iconStyle = new Style(x => x.Nesting().Descendant().Name("PART_StatusIconPresenter").Child());
            iconStyle.Add(ForegroundProperty, ResultTokenKind.ResultWarningIconColor);
            iconStyle.Add(IconControl.FillBrushProperty, ResultTokenKind.ResultWarningIconColor);
            iconStyle.Add(IconControl.StrokeBrushProperty, ResultTokenKind.ResultWarningIconColor);
            warningStyle.Add(iconStyle);
        }
        
        Styles.Add(warningStyle);
        
        var errorStyle = new Style(x => x.PropertyEquals(StatusProperty, ResultStatus.Error));
        
        {
            var iconStyle = new Style(x => x.Nesting().Descendant().Name("PART_StatusIconPresenter").Child());
            iconStyle.Add(ForegroundProperty, ResultTokenKind.ResultErrorIconColor);
            iconStyle.Add(IconControl.FillBrushProperty, ResultTokenKind.ResultErrorIconColor);
            iconStyle.Add(IconControl.StrokeBrushProperty, ResultTokenKind.ResultErrorIconColor);
            errorStyle.Add(iconStyle);
        }
        
        Styles.Add(errorStyle);
    }
}