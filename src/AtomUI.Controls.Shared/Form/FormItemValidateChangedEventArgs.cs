using Avalonia.Interactivity;

namespace AtomUI.Controls;

public class FormItemValidateChangedEventArgs : RoutedEventArgs
{
    public FormValidateStatus Status { get; }
    public FormItemValidateChangedEventArgs(FormValidateStatus status)
    {
        Status = status;
    }
}