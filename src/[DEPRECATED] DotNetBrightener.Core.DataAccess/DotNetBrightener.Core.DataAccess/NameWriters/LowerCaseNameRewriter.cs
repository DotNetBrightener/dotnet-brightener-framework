using System.Globalization;

namespace DotNetBrightener.Core.DataAccess.NameWriters
{
    public class LowerCaseNameRewriter : INameRewriter
    {
        private readonly CultureInfo _culture;

        public LowerCaseNameRewriter(CultureInfo culture = null) => _culture = culture ?? CultureInfo.InvariantCulture;

        public string RewriteName(string name) => name.ToLower(_culture);
    }
}
