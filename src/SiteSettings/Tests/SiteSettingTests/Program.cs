
using DotNetBrightener.SiteSettings.Entities;
using DotNetBrightener.SiteSettings.Extensions;
using DotNetBrightener.SiteSettings.Models;
using Shouldly;
using Xunit;

internal class TestSetting : SiteSettingWrapper<TestSetting>
{
    public string TestString { get; set; } = "hello world";

    public          int    TestInt     { get; set; } = 2;
    public override string SettingName => "";
    public override string SettingDescription => "";
}

public class TestSiteSettingObject
{
    [Fact]
    public void SerializeObject_ShouldOnlySerializeNeededFields()
    {
        var siteSettingRecord = new SiteSettingRecord();
        var testSetting       = new TestSetting();

        testSetting.UpdateSetting(siteSettingRecord);

        siteSettingRecord.SettingContent.ShouldBe("{\"testString\":\"hello world\",\"testInt\":2}");
    }
}
