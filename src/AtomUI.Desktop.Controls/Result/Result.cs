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
            iconStyle.Add(WidthProperty, ResultTokenKey.IconSize);
            iconStyle.Add(HeightProperty, ResultTokenKey.IconSize);
            Styles.Add(iconStyle);
        }
        
        var infoStyle = new Style(x => x.PropertyEquals(StatusProperty, ResultStatus.Info));
        
        {
            var iconStyle = new Style(x => x.Nesting().Descendant().Name("PART_StatusIconPresenter").Child());
            iconStyle.Add(ForegroundProperty, ResultTokenKey.ResultInfoIconColor);
            iconStyle.Add(IconControl.FillBrushProperty, ResultTokenKey.ResultInfoIconColor);
            iconStyle.Add(IconControl.StrokeBrushProperty, ResultTokenKey.ResultInfoIconColor);
            infoStyle.Add(iconStyle);
        }
        
        Styles.Add(infoStyle);
        
        var successStyle = new Style(x => x.PropertyEquals(StatusProperty, ResultStatus.Success));
        
        {
            var iconStyle = new Style(x => x.Nesting().Descendant().Name("PART_StatusIconPresenter").Child());
            iconStyle.Add(ForegroundProperty, ResultTokenKey.ResultSuccessIconColor);
            iconStyle.Add(IconControl.FillBrushProperty, ResultTokenKey.ResultSuccessIconColor);
            iconStyle.Add(IconControl.StrokeBrushProperty, ResultTokenKey.ResultSuccessIconColor);
            successStyle.Add(iconStyle);
        }
        
        Styles.Add(successStyle);
        
        var warningStyle = new Style(x => x.PropertyEquals(StatusProperty, ResultStatus.Warning));
        
        {
            var iconStyle = new Style(x => x.Nesting().Descendant().Name("PART_StatusIconPresenter").Child());
            iconStyle.Add(ForegroundProperty, ResultTokenKey.ResultWarningIconColor);
            iconStyle.Add(IconControl.FillBrushProperty, ResultTokenKey.ResultWarningIconColor);
            iconStyle.Add(IconControl.StrokeBrushProperty, ResultTokenKey.ResultWarningIconColor);
            warningStyle.Add(iconStyle);
        }
        
        Styles.Add(warningStyle);
        
        var errorStyle = new Style(x => x.PropertyEquals(StatusProperty, ResultStatus.Error));
        
        {
            var iconStyle = new Style(x => x.Nesting().Descendant().Name("PART_StatusIconPresenter").Child());
            iconStyle.Add(ForegroundProperty, ResultTokenKey.ResultErrorIconColor);
            iconStyle.Add(IconControl.FillBrushProperty, ResultTokenKey.ResultErrorIconColor);
            iconStyle.Add(IconControl.StrokeBrushProperty, ResultTokenKey.ResultErrorIconColor);
            errorStyle.Add(iconStyle);
        }
        
        Styles.Add(errorStyle);
    }
}