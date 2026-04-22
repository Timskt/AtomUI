using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Styling;

namespace AtomUI.Data;

public static class TokenResourceBinder
{
    public static IDisposable CreateTokenBinding<TTokenKind>(AvaloniaObject target,
                                                             AvaloniaProperty targetProperty,
                                                             TTokenKind resourceKey)
        where TTokenKind : Enum
    {
        return target.Bind(targetProperty, new DynamicResourceExtension(resourceKey));
    }

    public static IDisposable CreateTokenBinding(AvaloniaObject target,
                                                 AvaloniaProperty targetProperty,
                                                 string resourceKey)
    {
        return target.Bind(targetProperty, new DynamicResourceExtension(resourceKey));
    }
    
    public static IDisposable CreateTokenBinding<TTokenKind>(AvaloniaObject target,
                                                             AvaloniaProperty targetProperty,
                                                             Control context,
                                                             TTokenKind resourceKey,
                                                             BindingPriority priority = BindingPriority.Template,
                                                             Func<object?, object?>? converter = null)
        where TTokenKind : Enum
    {
        return target.Bind(targetProperty, context.GetResourceObservable(resourceKey, converter), priority);
    }

    public static IDisposable CreateTokenBinding<TTokenKind>(Control target,
                                                             AvaloniaProperty targetProperty,
                                                             TTokenKind resourceKey,
                                                             BindingPriority priority = BindingPriority.Template,
                                                             Func<object?, object?>? converter = null)
        where TTokenKind : Enum
    {
        return target.Bind(targetProperty, target.GetResourceObservable(resourceKey, converter), priority);
    }

    public static IDisposable CreateTokenBinding(Control target,
                                                 AvaloniaProperty targetProperty,
                                                 object resourceKey,
                                                 BindingPriority priority = BindingPriority.Template,
                                                 Func<object?, object?>? converter = null)
    {
        return target.Bind(targetProperty, target.GetResourceObservable(resourceKey, converter), priority);
    }

    public static IDisposable CreateGlobalTokenBinding<TTokenKind>(AvaloniaObject target,
                                                                   AvaloniaProperty targetProperty,
                                                                   TTokenKind resourceKey,
                                                                   BindingPriority priority = BindingPriority.Template,
                                                                   Func<object?, object?>? converter = null)
        where TTokenKind : Enum
    {
        return target.Bind(targetProperty, GetGlobalTokenResourceObservable(resourceKey, null, converter), priority);
    }

    public static IDisposable CreateGlobalResourceBinding(AvaloniaObject target,
                                                          AvaloniaProperty targetProperty,
                                                          object resourceKey,
                                                          BindingPriority priority = BindingPriority.Template,
                                                          Func<object?, object?>? converter = null)
    {
        return target.Bind(targetProperty, GetGlobalResourceObservable(resourceKey, null, converter), priority);
    }

    /// <summary>
    /// 直接在 resource dictionary 中查找，忽略本地覆盖的值
    /// </summary>
    public static IObservable<object?> GetGlobalTokenResourceObservable<TTokenKind>(TTokenKind resourceKey,
                                                                        ThemeVariant? themeVariant = null,
                                                                        Func<object?, object?>? converter = null)
        where TTokenKind : Enum
    {
        return GetGlobalResourceObservable(resourceKey, themeVariant, converter);
    }

    public static IObservable<object?> GetGlobalResourceObservable(object resourceKey,
                                                                   ThemeVariant? themeVariant = null,
                                                                   Func<object?, object?>? converter = null)
    {
        var application = Application.Current;
        if (application is null)
        {
            throw new ApplicationException("The application instance does not exist");
        }

        themeVariant ??= (application as IThemeVariantHost).ActualThemeVariant;
        return application.Styles.GetResourceObservable(resourceKey, themeVariant, converter);
    }
}