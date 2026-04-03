using AtomUI.Theme;
using AtomUI.Theme.Language;

namespace AtomUI.Desktop.Controls.ColorPickerLang
{
    internal static class ColorPickerLangResourceKey
    {
        public static readonly LanguageResourceKey EmptyColorText = new LanguageResourceKey("ColorPicker.EmptyColorText", "AtomUI.Lang");
    }

    public enum ColorPickerLangResourceKind
    {
        EmptyColorText
    }

    public class ColorPickerLangResourceKeyExtension : LanguageResourceExtension<ColorPickerLangResourceKind>
    {
        public ColorPickerLangResourceKeyExtension()
        {
        }

        public ColorPickerLangResourceKeyExtension(ColorPickerLangResourceKind kind) : base(kind)
        {
        }
    }
}