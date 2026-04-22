using System.Diagnostics;
using AtomUI.Data;
using Avalonia;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace AtomUI.Theme.Language;

public abstract class LanguageResourceExtension<TResourceKind> : DynamicResourceExtension
    where TResourceKind : Enum
{
    public TResourceKind? Kind
    {
        get => (TResourceKind?)ResourceKey; 
        set => ResourceKey = value; 
    }
    
    public LanguageResourceExtension()
    {
    }

    public LanguageResourceExtension(TResourceKind kind)
    {
        Kind = kind;
    }
    
    public new IBinding ProvideValue(IServiceProvider serviceProvider)
    {
        base.ProvideValue(serviceProvider);
        Debug.Assert(ResourceKey != null);
        var provideTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
        if (provideTarget?.TargetObject is not StyledElement)
        {
            var application = Application.Current;
            if (application is null)
            {
                throw new ApplicationException("The application instance does not exist");
            }
            this.SetAnchor(application);
        }
        return this;
    }
}