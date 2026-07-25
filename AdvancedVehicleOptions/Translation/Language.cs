using System.Collections.Generic;


namespace AdvancedVehicleOptionsUID
{
    /// <summary>
    /// Translation language class.
    /// </summary>
    public class Language
    {
        // Private fields.
        private Dictionary<string, string> _translationDictionary = new Dictionary<string, string>();

        /// <summary>
        /// Gets the translation key that identifies the file's readable language name.
        /// </summary>
        public static string NameKey => "NAME";

        /// <summary>
        /// Gets or sets the language's language code.
        /// </summary>
        public string Code { get; set; } = null;

        /// <summary>
        /// Gets or sets the language's human-readable name (in native language).
        /// </summary>
        public string Name { get; set; } = null;

        /// <summary>
        /// Gets the dictionary of translations for this language.
        /// </summary>
        public Dictionary<string, string> TranslationDictionary => _translationDictionary;
    }
}
