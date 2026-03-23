using AtomUI.Icons.AntDesign;
using Avalonia;

namespace AtomUI.Desktop.Controls;

public class BackTopFloatButton : FloatButton
{
    #region 公共属性定义

    public static readonly StyledProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.Register<BackTopFloatButton, TimeSpan>(nameof(Duration), TimeSpan.FromMilliseconds(450));
    
    public static readonly StyledProperty<ScrollViewer?> TargetProperty =
        AvaloniaProperty.Register<BackTopFloatButton, ScrollViewer?>(nameof(Target));
    
    public static readonly StyledProperty<double> VisibilityHeightProperty =
        AvaloniaProperty.Register<BackTopFloatButton, double>(nameof(VisibilityHeight), 400d);

    public TimeSpan Duration
    {
        get => GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }
    
    public ScrollViewer? Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }
    
    public double VisibilityHeight
    {
        get => GetValue(VisibilityHeightProperty);
        set => SetValue(VisibilityHeightProperty, value);
    }
    #endregion

    protected override void ConfigureDefaultIcon()
    {
        if (Icon == null)
        {
            SetCurrentValue(IconProperty, new VerticalAlignTopOutlined());
        }
    }
}