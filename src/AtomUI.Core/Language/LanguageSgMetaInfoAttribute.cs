namespace AtomUI.Theme.Language;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public class LanguageSgMetaInfoAttribute : Attribute
{
    public string? TargetNamespace { get; set; }
    
    public LanguageSgMetaInfoAttribute(string? targetNamespace)
    {
        TargetNamespace = targetNamespace;
    }
}