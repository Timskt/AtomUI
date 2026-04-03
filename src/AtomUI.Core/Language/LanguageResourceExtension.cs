using System.Diagnostics;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace AtomUI.Theme.Language;

public abstract class LanguageResourceExtension<TResourceKind> : MarkupExtension
    where TResourceKind : Enum
{
    public TResourceKind? Kind { get; set; }
    
    public LanguageResourceExtension()
    {
    }

    public LanguageResourceExtension(TResourceKind kind)
    {
        Kind = kind;
    }
    
    public override IBinding ProvideValue(IServiceProvider serviceProvider)
    {
        Debug.Assert(Kind != null);
        return new DynamicResourceExtension(Kind);
    }
}