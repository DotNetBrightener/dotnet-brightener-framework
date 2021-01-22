using System;

namespace DotNetBrightener.Core.DataAccess.Abstractions.Exceptions
{
    /// <summary>
    ///     Describes the exception that will be thrown when performing an non query operation, 
    ///     the number of affected records does not match the expectation
    /// </summary>
    public class ExpectedAffectedRecordMismatch : InvalidOperationException
    {
        public int? ExpectedAffectedRecords { get; private set; }

        public int? ActualAffectedRecords { get; private set; }

        public ExpectedAffectedRecordMismatch() : this("Expected the number of affected records mismatches")
        {

        }

        public ExpectedAffectedRecordMismatch(string message, int? expected = null, int? actual = null) : this(message, null)
        {
            ExpectedAffectedRecords = expected;
            ActualAffectedRecords = actual;
        }

        public ExpectedAffectedRecordMismatch(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}