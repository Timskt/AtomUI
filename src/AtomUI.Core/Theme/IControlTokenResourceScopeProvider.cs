namespace AtomUI.Theme;

public interface IControlTokenResourceScopeProvider
{
    string Id { get; }
    string? ResourceCatalog => null;
}

public class ControlTokenResourceScopeProvider : IControlTokenResourceScopeProvider
{
    public string Id { get; }
    public string? ResourceCatalog { get; }

    public ControlTokenResourceScopeProvider(string id, string? resourceCatalog = null)
    {
        Id = id;
        ResourceCatalog = resourceCatalog;
    }
}