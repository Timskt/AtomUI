using System.Diagnostics;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace AtomUI.Theme;

public static class ControlTokenResourcesScopeHostExtensions
{
    public static void RegisterTokenResourceScope(this Control host, IControlTokenResourceScopeProvider scopeProvider)
    {
        host.AttachedToLogicalTree   += HandleAttachedToLogicalTree;
        host.DetachedFromLogicalTree += HandleDetachedToLogicalTree;
        ControlTokenResourceScopeHost.SetTokenResourceScopeProvider(host, scopeProvider);
    }
    
    private static void HandleAttachedToLogicalTree(object? sender, LogicalTreeAttachmentEventArgs args)
    {
        if (sender is Control control)
        {
            var scopeProvider = ControlTokenResourceScopeHost.GetTokenResourceScopeProvider(control);
            if (scopeProvider != null)
            {
                var controlToken = TokenFinderUtils.FindControlToken(control,
                    scopeProvider.Id, scopeProvider.ResourceCatalog);
                Debug.Assert(controlToken != null);
                var delta = controlToken.GetSharedResourceDeltaDictionary();
                if (delta.Count > 0)
                {
                    var resourceDictionary = new ResourceDictionary();
                    foreach (var entry in controlToken.GetSharedResourceDeltaDictionary())
                    {
                        if (entry.Value is Color color)
                        {
                            resourceDictionary[entry.Key] = new SolidColorBrush(color);
                        }
                        else
                        {
                            resourceDictionary[entry.Key] = entry.Value;
                        }
                    
                    }
                    control.Resources.MergedDictionaries.Add(resourceDictionary);
                    ControlTokenResourceScopeHost.SetScopedTokenResources(control, resourceDictionary);
                }
            }
        }
    }

    private static void HandleDetachedToLogicalTree(object? sender, LogicalTreeAttachmentEventArgs args)
    {
        if (sender is Control control)
        {
            var scopedTokenResources = ControlTokenResourceScopeHost.GetScopedTokenResources(control);
            if (scopedTokenResources != null)
            {
                control.Resources.MergedDictionaries.Remove(scopedTokenResources);
            }
        }
    }
}

public class ControlTokenResourceScopeHost
{
    public static readonly AttachedProperty<IControlTokenResourceScopeProvider?> TokenResourceScopeProviderProperty =
        AvaloniaProperty.RegisterAttached<ControlTokenResourceScopeHost, Control, IControlTokenResourceScopeProvider?>("TokenResourceScopeProvider");
    
    internal static readonly AttachedProperty<ResourceDictionary?> ScopedTokenResourcesProperty =
        AvaloniaProperty.RegisterAttached<ControlTokenResourceScopeHost, Control, ResourceDictionary?>("ScopedTokenResources");

    public static IControlTokenResourceScopeProvider? GetTokenResourceScopeProvider(Control host)
    {
        return host.GetValue(TokenResourceScopeProviderProperty);
    }

    public static void SetTokenResourceScopeProvider(Control host, IControlTokenResourceScopeProvider? value)
    {
        host.SetValue(TokenResourceScopeProviderProperty, value);
    }

    internal static ResourceDictionary? GetScopedTokenResources(Control host)
    {
        return host.GetValue(ScopedTokenResourcesProperty);
    }

    internal static void SetScopedTokenResources(Control host, ResourceDictionary? value)
    {
        host.SetValue(ScopedTokenResourcesProperty, value);
    }
}
