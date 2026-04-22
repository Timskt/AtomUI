using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Styling;

namespace AtomUI.Data;

public static class LanguageResourceBinder
{
    public static IDisposable CreateBinding<TResourceKind>(AvaloniaObject target,
                                                           AvaloniaProperty targetProperty,
                                                           TResourceKind resourceKey,
                                                           BindingPriority priority = BindingPriority.Template,
                                                           Func<object?, object?>? converter = null)
        where TResourceKind : Enum
    {
        var application = Application.Current;
        if (application is null)
        {
            throw new ApplicationException("The application instance does not exist");
        }

        var themeVariant = (application as IThemeVariantHost).ActualThemeVariant;
        var observable   = application.Styles.GetResourceObservable(resourceKey, themeVariant, converter);

        return target.Bind(targetProperty, observable, priority);
    }
    
    public static string? GetLangResource<TResourceKind>(TResourceKind resourceKey, ThemeVariant? themeVariant = null)
        where TResourceKind : Enum
    {
        var application = Application.Current;
        if (application is null)
        {
            throw new ApplicationException("The application instance does not exist");
        }

        themeVariant ??= (application as IThemeVariantHost).ActualThemeVariant;
        if (application.Styles.TryGetResource(resourceKey, themeVariant, out var value))
        {
            return value as string;
        }

        return null;
    }
}