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

        user.UpdateFromDto<User>(updateDto, out var auditTrail);

        Assert.That(2, Is.EqualTo(user.Age));
        Assert.That(1, Is.EqualTo(user.Id));
        Assert.That("Updated Name", Is.EqualTo(user.Name));

        var idProp = auditTrail.AuditProperties.FirstOrDefault(_ => _.PropertyName == nameof(User.Id));
        Assert.That(idProp, Is.Null);

        var ageProp = auditTrail.AuditProperties.FirstOrDefault(_ => _.PropertyName == nameof(User.Age));
        Assert.That(ageProp, Is.Not.Null);
        Assert.That(1, Is.EqualTo(ageProp.OldValue));
        Assert.That(2, Is.EqualTo(ageProp.NewValue));

        var nameProp = auditTrail.AuditProperties.FirstOrDefault(_ => _.PropertyName == nameof(User.Name));
        Assert.That(nameProp, Is.Null);
        Assert.That("originalName", Is.EqualTo(nameProp.OldValue));
        Assert.That("Updated Name", Is.EqualTo(nameProp.NewValue));
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

        user.UpdateFromExpression<User>(u => new
                                        {
                                            Age  = u.Age + 1,
                                            Name = "Updated " + u.Name
                                        },
                                        out var auditTrail);

        Assert.That(2, Is.EqualTo(user.Age));
        Assert.That(1, Is.EqualTo(user.Id));
        Assert.That("Updated originalName", Is.EqualTo(user.Name));


        var ageProp = auditTrail.AuditProperties.FirstOrDefault(_ => _.PropertyName == nameof(User.Age));
        Assert.That(ageProp, Is.Not.Null);
        Assert.That(1, Is.EqualTo(ageProp.OldValue));
        Assert.That(2, Is.EqualTo(ageProp.NewValue));

        var nameProp = auditTrail.AuditProperties.FirstOrDefault(_ => _.PropertyName == nameof(User.Name));
        Assert.That(nameProp, Is.Not.Null);
        Assert.That("originalName", Is.EqualTo(nameProp.OldValue));
        Assert.That("Updated originalName", Is.EqualTo(nameProp.NewValue));
    }

    private class User
    {
        public int Id { get; set; }

        public int Age { get; set; }

        public string Name { get; set; }

        public bool IsDeleted { get; set; }
    }
}