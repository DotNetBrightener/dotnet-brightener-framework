using DotNetBrightener.Infrastructure.Security.Services;
using Shouldly;
using Xunit;

namespace DotNetBrightener.Infrastructure.Security.Tests;

public class PermissionLoadFromTypeTest
{
    [Fact]
    public void TestLoadPermissionFromType()
    {
        var loadedPermissions = new SomePermissionsList().GetPermissions()
                                                         .ToArray();

        loadedPermissions[0].PermissionKey.ShouldBe(SomePermissionsList.Permission1);
        loadedPermissions[0].Description.ShouldBe("Description for Permission 1");

        loadedPermissions[1].PermissionKey.ShouldBe(SomePermissionsList.Permission2);
        loadedPermissions[1].Description.ShouldBe("Description for Permission 2");
    }

    private class SomePermissionsList : AutomaticPermissionProvider
    {
        /// <summary>
        ///     Description for Permission 1
        /// </summary>
        public const string Permission1 = "Permission1";

        /// <summary>
        ///     Description for Permission 2
        /// </summary>
        public const string Permission2 = "Permission.Permission2";
    }
}