namespace AtomUI.Controls;

public interface IForm
{
    void Validate();
    void Submit();
    void Reset();
    void SetFormValues(IFormValues formValues);
    void DeleteFormItem(IFormItem formItem);
}