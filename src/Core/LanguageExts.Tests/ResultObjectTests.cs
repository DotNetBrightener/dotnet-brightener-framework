using LanguageExts.Results;
using NUnit.Framework;

namespace LanguageExts.Tests;

public class ResultObjectTests
{
    [Test]
    public void ResultObject_ImplicitCast_ShouldSuccessfullyCastValue()
    {
        Result<string> result = "Hello, World!";

        string value = result;

        Assert.That(value, Is.EqualTo("Hello, World!"));
    }
}