using DotNetBrightener.Core.DataAccess.NameWriters;

namespace DotNetBrightener.Core.DataAccess
{
    public class DataAccessConfiguration
    {
        public INameRewriter UsingNameRewriter { get; internal set; } = new NoneRewriter();

        /// <summary>
        ///     Indicates the naming convention to convert the table / column name to snake case.
        ///     Eg. entity with name `TableName` will become `table_name`
        /// </summary>
        public void UseSnakeCaseNameConvention()
        {
            UsingNameRewriter = new SnakeCaseNameRewriter();
        }

        /// <summary>
        ///     Indicates the naming convention to convert the table / column name to lower case.
        ///     Eg. entity with name `TableName` will become `tablename`
        /// </summary>
        public void UseLowerCaseNameConvention()
        {
            UsingNameRewriter = new LowerCaseNameRewriter();
        }

        /// <summary>
        ///     Indicates the naming convention to convert the table / column name to upper case.
        ///     Eg. entity with name `TableName` will become `TABLENAME`
        /// </summary>
        public void UseUpperCaseNameConvention()
        {
            UsingNameRewriter = new UpperCaseNameRewriter();
        }

        /// <summary>
        ///     Indicates the naming convention to convert the table / column name to camel case.
        ///     Eg. entity with name `TableName` will become `tableName`
        /// </summary>
        public void UseCamelCaseNameConvention()
        {
            UsingNameRewriter = new CamelCaseNameRewriter();
        }

        /// <summary>
        ///     Indicates the naming convention to convert the table / column name to snake case and make it upper case.
        ///     Eg. entity with name `TableName` will become `TABLE_NAME`
        /// </summary>
        public void UseUpperSnakeCaseNameConvention()
        {
            UsingNameRewriter = new UpperSnakeCaseNameRewriter();
        }
    }
}