namespace DotNetBrightener.TimeBasedOtp;

public interface IOTPProvider
{
    /// <summary>
    ///     Generate One-Time-Password from the given <see cref="secret"/>,
    ///     optionally specify password length and
    ///     whether the <see cref="secret"/> spaces matter
    /// </summary>
    /// <param name="secret">
    ///     The secret to form the OTP
    /// </param>
    /// <param name="digits">
    ///     Length of the generated OTP
    /// </param>
    /// <param name="ignoreSpaces">
    ///     Indicates whether spaces in the <see cref="secret"/> matter
    /// </param>
    /// <returns>
    ///     The OTP value with the length of <see cref="digits"/>
    /// </returns>
    string GetPassword(string secret, int digits = 6, bool ignoreSpaces = false);

    /// <summary>
    ///     Validates the OTP against the given <see cref="secret"/>
    /// </summary>
    /// <param name="password">
    ///     The password
    /// </param>
    /// <param name="secret">
    ///     The secret to validate the <see cref="password"/>
    /// </param>
    /// <param name="ignoreSpaces">
    ///     Indicates whether spaces in the <see cref="secret"/> matter
    /// </param>
    /// <param name="checkAdjacentIntervals">
    ///     The interval for checking in case the time is slightly off when receiving the validation requests
    /// </param>
    /// <returns>
    ///     <c>true</c> if the given password is correct, <c>false</c> otherwise
    /// </returns>
    bool ValidateOTP(string password, string secret, bool ignoreSpaces = false, int checkAdjacentIntervals = 1);
}