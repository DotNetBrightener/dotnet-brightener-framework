using System;

namespace DotNetBrightener.Core.DataAccess.Exceptions
{
    public class ExpectedAffectedRecordMismatch : InvalidOperationException
    {
        public ExpectedAffectedRecordMismatch(): this("Expected the number of affected records mismatches")
        {
            
        }

        public ExpectedAffectedRecordMismatch(string message) : base(message)
        {
            
        }
    }
}