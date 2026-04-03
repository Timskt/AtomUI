using System.Diagnostics;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Logging;

namespace AtomUI.Theme.Language;

public abstract class LanguageProvider : ILanguageProvider
{
    public LanguageCode LangCode { get; }

    public string LangId { get; }

    public string ResourceCatalog { get; }

    public LanguageProvider()
    {
        var type                      = GetType();
        var languageProviderAttribute = type.GetCustomAttribute<LanguageProviderAttribute>();
        if (languageProviderAttribute is null)
        {
            throw new LanguageMetaInfoParseException("No annotations found LanguageProviderAttribute");
        }

        LangCode        = languageProviderAttribute.LanguageCode;
        LangId          = languageProviderAttribute.LanguageId;
        ResourceCatalog = languageProviderAttribute.ResourceCatalog;
    }

    public void BuildResourceDictionary(IResourceDictionary dictionary)
    {
        var type             = GetType();
        var resourceKindType = GetResourceKindType();
        try
        {
            var languageFields = type.GetFields(BindingFlags.Public |
                                                BindingFlags.Static |
                                                BindingFlags.FlattenHierarchy)
                                     .ToDictionary(x => x.Name);
            foreach (var value in Enum.GetValues(resourceKindType))
            {
                var languageKey = Enum.GetName(resourceKindType, value);
                Debug.Assert(languageKey != null);
                if (languageFields.TryGetValue(languageKey, out var field))
                {
                    var languageText = field.GetValue(this);
                    dictionary[value] = languageText; 
                }
                else
                {
                    throw new Exception($"Language item: {languageKey} does not exist in {type.FullName}");
                }
            }
        }
        catch (Exception)
        {
            var logger = Logger.TryGet(LogEventLevel.Error, AtomUILogArea.Theme);
            logger?.Log(this, $"Build Resource for Language {resourceKindType.FullName} error.");
            throw;
        }
    }

    protected abstract Type GetResourceKindType();
}