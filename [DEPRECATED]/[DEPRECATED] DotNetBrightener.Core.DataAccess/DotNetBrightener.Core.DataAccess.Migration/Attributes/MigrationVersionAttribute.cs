using System;
using System.Globalization;
using FluentMigrator;

namespace DotNetBrightener.Core.DataAccess.Migration.Attributes
{
    public class MigrationVersionAttribute : MigrationAttribute
    {
        private static string[] SupportedDateFormat { get; } = {
            "yyyy-MM-dd HH:mm:ss",
            "yyyy.MM.dd HH:mm:ss",
            "yyyy/MM/dd HH:mm:ss"
        };

        public MigrationVersionAttribute(string dateTime) :
            base(GetVersion(dateTime), null)
        {
        }
                
        public MigrationVersionAttribute(string dateTime, string description) :
            base(GetVersion(dateTime), description)
        {
        }

        private static long GetVersion(string dateTime)
        {
            return DateTime.ParseExact(dateTime, SupportedDateFormat, CultureInfo.InvariantCulture).Ticks;
        }
    }
}
