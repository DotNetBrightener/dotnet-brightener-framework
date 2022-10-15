# Time Based OTP Library

&copy; 2022 DotNet Brightener.

## Installation

Run this in command line:

``` bash
dotnet add package DotNetBrightener.TimeBasedOtp
```

Or add the following to `.csproj` file

```xml
<PackageReference Include="DotNetBrightener.TimeBasedOtp" Version="2022.9.0" />
```

## Usage

### Register service

``` cs
serviceCollection.AddOtpProvider();
```

### In your service / controller:

``` cs 
// inject the service
IOTPProvider otpProvider; 

// generate a secret
var secret = "<some random string>";

// define the length of the password to generate
var passwordLength = 6;

// indicates whether the secret's spaces matter or not.
var ignoreSpaces = false; // false: Spaces matter. true: Spaces don't matter

var otp = otpProvider.GetPassword(secret, passwordLength, ignoreSpaces);

// the adjacent intervals to check for the OTP. The greater this value is, the higher risk of being hijacked.
var checkAdjacentIntervals = 2;

var isOtpValid = otpProvider.ValidateOTP(otp, secret, ignoreSpaces, checkAdjacentIntervals;
```

## Note

The secret should be stored securely within your application.