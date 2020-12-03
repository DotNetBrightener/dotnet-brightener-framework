using System;

namespace DotNetBrightener.Integration.DataMigration.Exceptions
{
    public class DataMigrationException : Exception
    {
        public DataMigrationException(string message)
            : base(message, null)
        {
        }

        public DataMigrationException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }
}