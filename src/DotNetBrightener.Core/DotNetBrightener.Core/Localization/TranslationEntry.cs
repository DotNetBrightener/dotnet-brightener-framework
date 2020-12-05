namespace DotNetBrightener.Core.Localization
{
    /// <summary>
    ///     Represents an entry of the translation
    /// </summary>
    public class TranslationEntry
    {
        /// <summary>
        ///     The Key of the translation entry
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        ///     The translated value of the entry
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        ///     Indicates whether the entry is to be deleted
        /// </summary>
        public bool MarkedForDelete { get; set; }
    }
}