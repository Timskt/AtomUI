namespace AtomUI.Theme.Language;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class LanguageProviderAttribute : Attribute
{
    public const string DefaultLanguageId = "Default";
    public LanguageCode LanguageCode { get; }
    public string LanguageId { get; }

    public LanguageProviderAttribute(LanguageCode languageCode, 
                                     string languageId = DefaultLanguageId)
    {
        LanguageCode    = languageCode;
        LanguageId      = languageId;
    }
}