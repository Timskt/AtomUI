using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Primitives;

public class ScopeAwareOverlayLayer : Canvas
{
    public static readonly StyledProperty<Visual?> LayerHostProperty = AvaloniaProperty
        .Register<ScopeAwareOverlayLayer, Visual?>(nameof(LayerHost));

    public Visual? LayerHost
    {
        get => GetValue(LayerHostProperty);
        set => SetValue(LayerHostProperty, value);
    }
    
    protected override bool BypassFlowDirectionPolicies => true;
    public Size AvailableSize { get; private set; }
    
    public static ScopeAwareOverlayLayer? GetLayer(Visual visual)
    {
        Layoutable? layerHost = visual.FindAncestorOfType<ScrollContentPresenter>(true);
        if (layerHost != null)
        {
            while (layerHost != null)
            {
                layerHost = layerHost.FindAncestorOfType<ScrollContentPresenter>();
            }
        }

        layerHost ??= visual.FindAncestorOfType<VisualLayerManager>();
        layerHost ??= TopLevel.GetTopLevel(visual);

        if (layerHost == null)
        {
            return null;
        }

        var layer =
            layerHost.GetVisualChildren().FirstOrDefault(c => c is ScopeAwareOverlayLayer) as ScopeAwareOverlayLayer;
        layer ??= InjectLayer(layerHost);

        return layer;
    }
    
     private static ScopeAwareOverlayLayer InjectLayer(Layoutable layerHost)
    {
        var layer = FindAdornerLayer(layerHost);
        if (layer != null)
        {
            return layer;
        }

        layer = new ScopeAwareOverlayLayer
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment   = VerticalAlignment.Stretch,
            ZIndex              = VisualLayerManager.ScopeAwareOverlayZIndex
        };

        layer.LayerHost = layerHost;
        if (layerHost is VisualLayerManager visualLayerManager)
        {
            visualLayerManager.AddLayer(layer, visualLayerManager.ZIndex);
        }
        else if (layerHost is ScrollContentPresenter scrollContentPresenter)
        {
            if (scrollContentPresenter.Content is Control controlContent)
            {
                var oldOffset = scrollContentPresenter.Offset;
                // 直接内容控件
                scrollContentPresenter.Content = null;
                scrollContentPresenter.UpdateChild();
                var panel = new Panel
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment   = VerticalAlignment.Stretch,
                };
                panel.Children.Add(controlContent);
                panel.Children.Add(layer);
                scrollContentPresenter.Content = panel;
                var scrollViewer = scrollContentPresenter.FindAncestorOfType<ScrollViewer>();
                if (scrollViewer != null)
                {
                    scrollViewer.Offset = oldOffset;
                }
            }
            else if (scrollContentPresenter.Content != null && scrollContentPresenter.ContentTemplate != null)
            {
                // 模版处理
                var injectTemplate = new FuncDataTemplate<object?>((o, scope) =>
                {
                    var originControl = scrollContentPresenter.ContentTemplate.Build(o);
                    var panel = new Panel
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment   = VerticalAlignment.Stretch,
                    };
                    Debug.Assert(originControl != null);
                    panel.Children.Add(originControl);
                    panel.Children.Add(layer);
                    return panel;
                });
                scrollContentPresenter.ContentTemplate = injectTemplate;
            }
        }

        return layer;
    }
     
    private static ScopeAwareOverlayLayer? FindAdornerLayer(Layoutable layerHost)
    {
        if (layerHost is ScrollContentPresenter scrollContentPresenter)
        {
            // 在 Panel 下面
            var panel = scrollContentPresenter.FindChildOfType<Panel>();
            if (panel is not null)
            {
                return panel.FindChildOfType<ScopeAwareOverlayLayer>();
            }
        }
        else if (layerHost is VisualLayerManager visualLayerManager)
        {
            // 直接就在下面
            visualLayerManager.FindChildOfType<ScopeAwareOverlayLayer>();
        }

        return null;
    }
    
    protected override Size MeasureOverride(Size availableSize)
    {
        foreach (Control child in Children)
        {
            child.Measure(availableSize);
        }
        return availableSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        // We are saving it here since child controls might need to know the entire size of the overlay
        // and Bounds won't be updated in time
        AvailableSize = finalSize;
        return base.ArrangeOverride(finalSize);
    }
}