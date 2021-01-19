using System;

namespace DotNetBrightener.Core.DataAccess.Exceptions
{
    public class ExpectedAffectedRecordMismatch : InvalidOperationException
    {
        public int? ExpectedAffectedRecords { get; private set; }

        public int? ActualAffectedRecords { get; private set; }

        public ExpectedAffectedRecordMismatch() : this("Expected the number of affected records mismatches")
        {

        }

        public ExpectedAffectedRecordMismatch(string message, int? expected = null, int? actual = null) : base(message)
        {
            ExpectedAffectedRecords = expected;
            ActualAffectedRecords = actual;
        }
    }
}