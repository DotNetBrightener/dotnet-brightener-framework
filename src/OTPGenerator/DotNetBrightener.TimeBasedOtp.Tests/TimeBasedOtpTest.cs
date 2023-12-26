using System;
using System.Text.RegularExpressions;
using NSubstitute;
using NUnit.Framework;

namespace DotNetBrightener.TimeBasedOtp.Tests;

[TestFixture]
public class TimeBasedOtpTest
{
    private          string            _secret;
    private          string            _secretWithSpace;
    private          IOTPProvider      _sut;
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();

    [SetUp]
    public void Setup()
    {
        _secret          = "8STItbyoq7XXE8IMtdaR54bh";
        _secretWithSpace = Regex.Replace(_secret, ".{4}", "$0 ").Trim();
        _sut             = new TimeBasedOTPProvider(_dateTimeProvider, true);
    }

    [Test]
    public void GeneratePassword()
    {
        _dateTimeProvider.UtcNow.Returns(new DateTime(2022, 10, 15, 19, 10, 0));

        var otp = _sut.GetPassword(_secret, 8);
        Console.WriteLine("Testing with {0}, value: {1}", _secret, otp);

        var otp2 = _sut.GetPassword(_secretWithSpace, 8);
        Console.WriteLine("Testing with {0}, value: {1}", _secretWithSpace, otp2);

        var otp3 = _sut.GetPassword(_secretWithSpace, 8, true);
        Console.WriteLine("Testing with {0}, value: {1}", _secretWithSpace, otp3);

        Assert.That(otp3, Is.EqualTo(otp), "If spaces don't matter, OTP should be same");

        Assert.That(otp2, Is.Not.EqualTo(otp), "If spaces do matter, OTP should be different");
    }

    [Test]
    public void ValidatePasswordTest()
    {
        // set the time for generating the OTP
        var generatingOtpTime = new DateTime(2022, 10, 15, 19, 10, 0);
        _dateTimeProvider.UtcNow.Returns(generatingOtpTime);

        var otp = _sut.GetPassword(_secret, 8);
        Console.WriteLine("Generated password: {0}", otp);

        // set the time for verify the OTP 20 sec after the generated time
        _dateTimeProvider.UtcNow.Returns(generatingOtpTime.AddSeconds(20));

        var validationResult = _sut.ValidateOTP(otp, _secret);
        Assert.That(validationResult, Is.True, "Within a minute should be valid");

        // set the time for verify the OTP 50 sec after the generated time
        _dateTimeProvider.UtcNow.Returns(generatingOtpTime.AddSeconds(50));

        var validationResult2 = _sut.ValidateOTP(otp, _secret);
        Assert.That(validationResult2, Is.True, "Within a minute should be valid");

        // set the time for verify the OTP 80 sec after the generated time
        _dateTimeProvider.UtcNow.Returns(generatingOtpTime.AddSeconds(80));

        var validationResult3 = _sut.ValidateOTP(otp, _secret);
        Assert.That(validationResult3, Is.False, "Outside a minute should be invalid");

        validationResult3 = _sut.ValidateOTP(otp, _secret, checkAdjacentIntervals: 2);
        Assert.That(validationResult3, Is.True, "Outside a minute but with bigger step should be valid");

        // set the time for verify the OTP 80 sec after the generated time
        _dateTimeProvider.UtcNow.Returns(generatingOtpTime.AddSeconds(120));
        validationResult3 = _sut.ValidateOTP(otp, _secret, checkAdjacentIntervals: 2);
        Assert.That(validationResult3,
                    Is.False,
                    "Outside a minute with bigger step, but not big enough should still be invalid");
    }
}
