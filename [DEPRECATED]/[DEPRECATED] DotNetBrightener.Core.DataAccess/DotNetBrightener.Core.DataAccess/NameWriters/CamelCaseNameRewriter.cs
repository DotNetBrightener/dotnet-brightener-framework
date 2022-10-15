using System.Globalization;

namespace DotNetBrightener.Core.DataAccess.NameWriters
{
    public class CamelCaseNameRewriter : INameRewriter
    {
        private readonly CultureInfo _culture;

        public CamelCaseNameRewriter(CultureInfo culture = null) => _culture = culture ?? CultureInfo.InvariantCulture;

        public string RewriteName(string name) =>
            string.IsNullOrEmpty(name) ? name : char.ToLower(name[0], _culture) + name.Substring(1);
    }
}
