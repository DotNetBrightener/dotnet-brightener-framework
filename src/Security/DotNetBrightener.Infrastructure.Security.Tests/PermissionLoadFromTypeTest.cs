using System.Linq;
using DotNetBrightener.Infrastructure.Security.Services;
using NUnit.Framework;

namespace DotNetBrightener.Infrastructure.Security.Tests;

[TestFixture]
public class PermissionLoadFromTypeTest
{
    [SetUp]
    public void Setup()
    {

    }

    [Test]
    public void TestLoadPermissionFromType()
    {
        var loadedPermissions = new SomePermissionsList().GetPermissions()
                                                         .ToArray();
            
        Assert.AreEqual(SomePermissionsList.Permission1, loadedPermissions[0].PermissionKey);
        Assert.AreEqual("Description for Permission 1", loadedPermissions[0].Description);

        Assert.AreEqual(SomePermissionsList.Permission2, loadedPermissions[1].PermissionKey);
        Assert.AreEqual("Description for Permission 2", loadedPermissions[1].Description);
    }

    private class SomePermissionsList: AutomaticPermissionProvider
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