using Avalonia.Metadata;

namespace AtomUI.Theme;

public class ControlTokenInfoSetter
{
    public bool EnableAlgorithm { get; set; } = false;
    
    [Content]
    public List<TokenSetter> Setters { get; set; } = new ();
    
    public string TokenId { get; set; } = string.Empty;

    public ControlTokenInfoSetter()
    {
    }

    public ControlTokenInfoSetter(string tokenId)
    {
        TokenId = tokenId;
    }
}