using System.Linq;
using NUnit.Framework;

namespace DotNetBrightener.DataTransferObjectUtility.Tests;

[TestFixture]
public class DtoUpdateEntityTest
{
    [SetUp]
    public void Setup()
    {

    }

    [Test]
    public void TestUpdateEntityByUsingDto()
    {
        var updateDto = new
        {
            Age  = 2,
            Name = "Updated Name"
        };


        var user = new User
        {
            Id   = 1,
            Age  = 1,
            Name = "originalName"
        };

        DataTransferObjectUtils.UpdateEntityFromDto<User>(user, updateDto, out var auditTrail);

        Assert.AreEqual(2, user.Age);
        Assert.AreEqual(1, user.Id);
        Assert.AreEqual("Updated Name", user.Name);

        var idProp = auditTrail.AuditProperties.FirstOrDefault(_ => _.PropertyName == nameof(User.Id));
        Assert.IsNull(idProp);

        var ageProp = auditTrail.AuditProperties.FirstOrDefault(_ => _.PropertyName == nameof(User.Age));
        Assert.NotNull(ageProp);
        Assert.AreEqual(1, ageProp.OldValue);
        Assert.AreEqual(2, ageProp.NewValue);

        var nameProp = auditTrail.AuditProperties.FirstOrDefault(_ => _.PropertyName == nameof(User.Name));
        Assert.NotNull(nameProp);
        Assert.AreEqual("originalName", nameProp.OldValue);
        Assert.AreEqual("Updated Name", nameProp.NewValue);
    }

    [Test]
    public void TestUpdateEntityByUsingExpressionDto()
    {
        var user = new User
        {
            Id   = 1,
            Age  = 1,
            Name = "originalName"
        };

        DataTransferObjectUtils.UpdateEntityFromDtoExpression<User>(user,
                                                                    u => new
                                                                    {
                                                                        Age  = u.Age + 1,
                                                                        Name = "Updated " + u.Name
                                                                    },
                                                                    out var auditTrail);
            
        Assert.AreEqual(2, user.Age);
        Assert.AreEqual(1, user.Id);
        Assert.AreEqual("Updated originalName", user.Name);


        var ageProp = auditTrail.AuditProperties.FirstOrDefault(_ => _.PropertyName == nameof(User.Age));
        Assert.NotNull(ageProp);
        Assert.AreEqual(1, ageProp.OldValue);
        Assert.AreEqual(2, ageProp.NewValue);

        var nameProp = auditTrail.AuditProperties.FirstOrDefault(_ => _.PropertyName == nameof(User.Name));
        Assert.NotNull(nameProp);
        Assert.AreEqual("originalName", nameProp.OldValue);
        Assert.AreEqual("Updated originalName", nameProp.NewValue);
    }

    private class User
    {
        public int Id { get; set; }

        public int Age { get; set; }

        public string Name { get; set; }

        public bool IsDeleted { get; set; }
    }
}