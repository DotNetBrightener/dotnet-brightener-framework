using System.Globalization;

namespace DotNetBrightener.Core.DataAccess.NameWriters
{
    public class UpperCaseNameRewriter : INameRewriter
    {
        private readonly CultureInfo _culture;

        public UpperCaseNameRewriter(CultureInfo culture = null) => _culture = culture ?? CultureInfo.InvariantCulture;

        public string RewriteName(string name) => name.ToUpper(_culture);
    }
}
