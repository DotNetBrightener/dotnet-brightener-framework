using System.Globalization;
using System;

namespace DotNetBrightener.Core.DataAccess.NameWriters
{
    public class UpperSnakeCaseNameRewriter : SnakeCaseNameRewriter
    {
        private readonly CultureInfo _culture;

        public UpperSnakeCaseNameRewriter(CultureInfo culture = null): base(culture) => _culture = culture ?? CultureInfo.InvariantCulture;

        public override string RewriteName(string name) => base.RewriteName(name).ToUpper(_culture);
    }
}
