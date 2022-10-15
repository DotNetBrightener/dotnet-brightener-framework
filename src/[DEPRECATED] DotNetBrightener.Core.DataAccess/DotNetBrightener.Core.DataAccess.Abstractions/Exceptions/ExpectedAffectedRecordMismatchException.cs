using System;

namespace DotNetBrightener.Core.DataAccess.Abstractions.Exceptions
{
    /// <summary>
    ///     Describes the exception that will be thrown when performing an non query operation, 
    ///     the number of affected records does not match the expectation
    /// </summary>
    public class ExpectedAffectedRecordMismatchException : InvalidOperationException
    {
        public int? ExpectedAffectedRecords { get; }

        public int? ActualAffectedRecords { get; }

        public ExpectedAffectedRecordMismatchException(int expected, int actual, Exception innerException = null)
            : this($"Expected {expected} records affected by the given operation, but {actual} records were.",
                   expected,
                   actual,
                   innerException)
        {
        }

        public ExpectedAffectedRecordMismatchException(string    message,
                                                       int?      expected       = null,
                                                       int?      actual         = null,
                                                       Exception innerException = null)
            : this(message, innerException)
        {
            ExpectedAffectedRecords = expected;
            ActualAffectedRecords   = actual;
        }

        public ExpectedAffectedRecordMismatchException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}