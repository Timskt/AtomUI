using AvaloniaVisualLayerManager = Avalonia.Controls.Primitives.VisualLayerManager;

namespace AtomUI.Controls.Primitives;

public class VisualLayerManager : AvaloniaVisualLayerManager
{
    internal const int ScopeAwareAdornerLayerZIndex = int.MaxValue - 1000;
    internal const int ScopeAwareOverlayZIndex = int.MaxValue - 990;
    
    protected T? FindLayer<T>() where T : class
    {
        var layers = this.GetLayers();
        foreach (var layer in layers)
        {
            if (layer is T match)
            {
                return match;
            }
        }
        return null;
    }
}