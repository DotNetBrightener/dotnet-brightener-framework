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
            
        DataTransferObjectUtils.UpdateEntityFromDto<User>(user, updateDto);
            
        Assert.AreEqual(2, user.Age);
        Assert.AreEqual(1, user.Id);
        Assert.AreEqual("Updated Name", user.Name);
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
                                                                    });
            
        Assert.AreEqual(2, user.Age);
        Assert.AreEqual(1, user.Id);
        Assert.AreEqual("Updated originalName", user.Name);
    }

    private class User
    {
        public int Id { get; set; }

        public int Age { get; set; }

        public string Name { get; set; }

        public bool IsDeleted { get; set; }
    }
}