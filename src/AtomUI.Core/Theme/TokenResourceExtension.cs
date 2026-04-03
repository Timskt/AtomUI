using Avalonia.Markup.Xaml.MarkupExtensions;

namespace AtomUI.Theme;

public abstract class TokenResourceExtension<TTokenKind> : DynamicResourceExtension
    where TTokenKind : Enum
{
    public TTokenKind? Kind
    {
        get => (TTokenKind?)ResourceKey; 
        set => ResourceKey = value; 
    }
    
    public TokenResourceExtension()
    {
    }

    public TokenResourceExtension(TTokenKind kind)
    {
        Kind = kind;
    }
}

