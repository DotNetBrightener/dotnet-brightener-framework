namespace DotNetBrightener.Core.DataAccess.NameWriters
{
    public class NoneRewriter : INameRewriter
    {
        public string RewriteName(string name)
        {
            return name;
        }
    }
}
