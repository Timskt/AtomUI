using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal class TourStepIndicator : Control
{
    #region 公共属性定义

    public static readonly StyledProperty<double> IndicatorSizeProperty = 
        AvaloniaProperty.Register<TourStepIndicator, double>(nameof(IndicatorSize));
    
    public static readonly StyledProperty<IBrush?> IndicatorColorProperty = 
        AvaloniaProperty.Register<TourStepIndicator, IBrush?>(nameof(IndicatorColor));
    
    public static readonly StyledProperty<IBrush?> IndicatorActiveColorProperty = 
        AvaloniaProperty.Register<TourStepIndicator, IBrush?>(nameof(IndicatorActiveColor));
    
    public static readonly StyledProperty<int> StepCountProperty = 
        AvaloniaProperty.Register<TourStepIndicator, int>(nameof(StepCount));
    
    public static readonly StyledProperty<int> ActiveIndexProperty = 
        AvaloniaProperty.Register<TourStepIndicator, int>(nameof(ActiveIndex));
    
    public static readonly StyledProperty<double> ItemSpacingProperty = 
        AvaloniaProperty.Register<TourStepIndicator, double>(nameof(ItemSpacing));

    public double IndicatorSize
    {
        get => GetValue(IndicatorSizeProperty);
        set => SetValue(IndicatorSizeProperty, value);
    }
        
    public IBrush? IndicatorColor
    {
        get => GetValue(IndicatorColorProperty);
        set => SetValue(IndicatorColorProperty, value);
    }
    
    public IBrush? IndicatorActiveColor
    {
        get => GetValue(IndicatorActiveColorProperty);
        set => SetValue(IndicatorActiveColorProperty, value);
    }
    
    public int StepCount
    {
        get => GetValue(StepCountProperty);
        set => SetValue(StepCountProperty, value);
    }
    
    public int ActiveIndex
    {
        get => GetValue(ActiveIndexProperty);
        set => SetValue(ActiveIndexProperty, value);
    }
    
    public double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }
    #endregion

    static TourStepIndicator()
    {
        AffectsRender<TourStepIndicator>(IndicatorColorProperty, IndicatorActiveColorProperty, ActiveIndexProperty);
        AffectsMeasure<TourStepIndicator>(ItemSpacingProperty, IndicatorSizeProperty, StepCountProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var height = IndicatorSize;
        var width = StepCount * IndicatorSize + ItemSpacing * (StepCount + 1);
        return new Size(width, height);
    }

    public override void Render(DrawingContext context)
    {
        var offsetX = ItemSpacing;
        var offsetY = (DesiredSize.Height - IndicatorSize) / 2;
        for (var i = 0; i < StepCount; i++)
        {
            IBrush? brush = null;
            if (ActiveIndex == i)
            {
                brush = IndicatorActiveColor;
            }
            else
            {
                brush = IndicatorColor;
            }
            context.DrawEllipse(brush, null, new Rect(offsetX, offsetY, IndicatorSize, IndicatorSize));
            offsetX += IndicatorSize + ItemSpacing;
        }
    }
}