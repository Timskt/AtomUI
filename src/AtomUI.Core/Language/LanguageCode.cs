namespace AtomUI.Theme.Language;

public enum LanguageCode
{
    zh_CN,
    en_US,
}

public static class LanguageCodeExtensions
{
    public static string ToHyphenString(this LanguageCode code)
    {
        var name = code.ToString();
        return name.Replace('_', '-');
    }
}