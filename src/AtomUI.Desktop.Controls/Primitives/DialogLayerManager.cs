using AtomUI.Controls.Primitives;

namespace AtomUI.Desktop.Controls.Primitives;

internal class DialogLayerManager : VisualLayerManager
{
    private const int DialogZIndex = int.MaxValue - 1000;
    
    public DialogLayer DialogLayer
    {
        get
        {
            var dialogLayer = FindLayer<DialogLayer>();
            if (dialogLayer == null)
            {
                dialogLayer = new DialogLayer();
                this.AddLayer(dialogLayer, DialogZIndex);
            }
            return dialogLayer;
        }
    }
}