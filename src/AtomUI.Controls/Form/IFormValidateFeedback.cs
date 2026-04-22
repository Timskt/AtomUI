using Avalonia.Controls.Templates;

namespace AtomUI.Controls;

public interface IFormValidateFeedback
{
    FormValidateStatus ValidateStatus { get; set; }
    
    public object? SuccessFeedback { get; set; }
    
    public IDataTemplate? SuccessFeedbackTemplate { get; set; }
  
    public object? WarningFeedback { get; set; }
    
    public IDataTemplate? WarningFeedbackTemplate { get; set; }
    
    public object? ErrorFeedback { get; set; }
    public IDataTemplate? ErrorFeedbackTemplate { get; set; }
    
    public object? ValidatingFeedback { get; set; }

    public IDataTemplate? ValidatingFeedbackTemplate { get; set; }
}