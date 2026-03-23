using AvaloniaVisualLayerManager = Avalonia.Controls.Primitives.VisualLayerManager;

namespace AtomUI.Controls.Primitives;

public class VisualLayerManager : AvaloniaVisualLayerManager
{
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