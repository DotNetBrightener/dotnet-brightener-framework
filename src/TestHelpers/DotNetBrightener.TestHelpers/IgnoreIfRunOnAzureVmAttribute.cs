using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace DotNetBrightener.TestHelpers;

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method,
                AllowMultiple = false,
                Inherited = false)]
public class IgnoreIfRunOnAzureVmAttribute : NUnitAttribute, IApplyToTest
{
    private static bool IsFromPipeLineBuild()
        => Environment.GetEnvironmentVariable("TF_BUILD") != null;

    public void ApplyToTest(Test test)
    {
        if (!IsFromPipeLineBuild())
            return;

        test.RunState = RunState.Skipped;
        test.MakeTestResult().SetResult(ResultState.Skipped, "This test is ignored because it is running on Azure VM");
    }
}