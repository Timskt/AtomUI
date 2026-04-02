namespace AtomUI.Controls.Primitives;

using AvaloniaVirtualizingStackPanel = Avalonia.Controls.VirtualizingStackPanel;

internal class VirtualizingStackPanel : AvaloniaVirtualizingStackPanel
{
    public void ScrollItemIntoView(int index)
    {
        ScrollIntoView(index);
    }
}
