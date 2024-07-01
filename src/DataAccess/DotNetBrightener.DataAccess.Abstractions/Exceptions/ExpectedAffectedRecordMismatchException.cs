namespace DotNetBrightener.DataAccess.Exceptions;

/// <summary>
///     Describes the exception that will be thrown when performing a non query operation, 
///     the number of affected records does not match the expectation
/// </summary>
public class ExpectedAffectedRecordMismatchException(string message, Exception innerException)
    : InvalidOperationException(message, innerException)
{
    public int? ExpectedAffectedRecords { get; private set; }

    public int? ActualAffectedRecords { get; private set; }

    public ExpectedAffectedRecordMismatchException()
        : this("Expected the number of affected records mismatches")
    {

    }

    public ExpectedAffectedRecordMismatchException(int? expected = null, int? actual = null)
        : this("Expected the number of affected records mismatches", expected, actual)
    {
        ExpectedAffectedRecords = expected;
        ActualAffectedRecords   = actual;
    }

    public ExpectedAffectedRecordMismatchException(string message, int? expected = null, int? actual = null)
        : this(message, null)
    {
        ExpectedAffectedRecords = expected;
        ActualAffectedRecords   = actual;
    }
}