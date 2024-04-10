
using DotNetBrightener.SiteSettings.Entities;
using DotNetBrightener.SiteSettings.Extensions;
using DotNetBrightener.SiteSettings.Models;
using NUnit.Framework;

internal class TestSetting : SiteSettingWrapper<TestSetting>
{
    public string TestString { get; set; } = "hello world";

    public          int    TestInt                    { get; set; } = 2;
}

public class TestSiteSettingObject
{
    [Test]
    public void SerializeObject_ShouldOnlySerializeNeededFields()
    {
        var siteSettingRecord = new SiteSettingRecord();
        var testSetting       = new TestSetting();

        testSetting.UpdateSetting(siteSettingRecord);

        Assert.That(siteSettingRecord.SettingContent, Is.EqualTo("{\"testString\":\"hello world\",\"testInt\":2}"));
    }
}