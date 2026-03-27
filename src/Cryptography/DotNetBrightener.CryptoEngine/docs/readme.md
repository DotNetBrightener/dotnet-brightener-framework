# DotNetBrightener.CryptoEngine

A library providing APIs for asymmetric and symmetric encryption methods in .NET applications.

&copy; DotNet Brightener

## Installation

```bash
dotnet add package DotNetBrightener.CryptoEngine
```

## Features

- **RSA Asymmetric Encryption** - Encrypt/decrypt strings and files with public/private key pairs
- **AES Symmetric Encryption** - Modern symmetric encryption (recommended)
- **TripleDES Symmetric Encryption** - Legacy symmetric encryption (deprecated)
- **Password Utilities** - Password salt generation and random string generation
- **Time-based Tokens** - Generate and validate expiring tokens
- **Password Validation** - Secure password hashing and validation with `IPasswordValidationProvider`

---

## Quick Start

### 1. Register Services (Dependency Injection)

```csharp
using DotNetBrightener.CryptoEngine;
using DotNetBrightener.CryptoEngine.Loaders;
using DotNetBrightener.CryptoEngine.Options;

// Configure crypto engine
builder.Services.Configure<CryptoEngineConfiguration>(options =>
{
    options.RsaKeyLoader = "FileLoader"; // or "EnvVarLoader"
    options.RsaEnvironmentVariableName = "RSAPrivateKey"; // for EnvVarLoader
});

// Register loaders
builder.Services.AddSingleton<FileRSAKeysLoader>();
builder.Services.AddSingleton<EnvironmentVarISAKeysLoader>();

// Register crypto services
builder.Services.AddSingleton<ICryptoEngine, DefaultCryptoEngine>();
builder.Services.AddSingleton<IPasswordValidationProvider, DefaultPasswordValidationProvider>();
```

### 2. Using ICryptoEngine

```csharp
public class MyService
{
    private readonly ICryptoEngine _cryptoEngine;

    public MyService(ICryptoEngine cryptoEngine)
    {
        _cryptoEngine = cryptoEngine;
        _cryptoEngine.Initialize();
    }

    public void Example()
    {
        string plainText = "Sensitive data";

        // Encrypt with system key
        string encrypted = _cryptoEngine.EncryptText(plainText);

        // Decrypt with system key
        string decrypted = _cryptoEngine.DecryptText(encrypted);

        // Sign data
        string signature = _cryptoEngine.SignData(plainText);

        // Verify signature
        bool isValid = _cryptoEngine.VerifyData(plainText, signature);

        // Get public key for sharing
        string publicKey = _cryptoEngine.GetPublicKey();
    }
}
```

---

## RSA Encryption (RsaCryptoEngine)

Static class providing RSA encryption operations.

### Generate Key Pair

```csharp
using DotNetBrightener.CryptoEngine;

// Generate new RSA key pair (2048-bit)
var (publicKey, privateKey) = RsaCryptoEngine.GenerateKeyPair();

// Generate without PEM headers (inline string)
var (publicKeyInline, privateKeyInline) = RsaCryptoEngine.GenerateKeyPair(inlineString: true);
```

### Encrypt/Decrypt Strings

```csharp
string plainText = "Hello, World!";
string publicKey = "..."; // PEM or XML format

// Encrypt with public key
string encrypted = RsaCryptoEngine.EncryptString(plainText, publicKey);

// Decrypt with private key
string privateKey = "...";
string decrypted = RsaCryptoEngine.DecryptString(encrypted, privateKey);
```

### Encrypt/Decrypt Files

```csharp
// Encrypt file
RsaCryptoEngine.EncryptFile(@"C:\plain.txt", publicKey, @"C:\encrypted.txt");

// Decrypt file
RsaCryptoEngine.DecryptFile(@"C:\encrypted.txt", privateKey, @"C:\decrypted.txt");
```

### Sign and Verify Data

```csharp
string message = "Data to sign";

// Sign with private key
string signature = RsaCryptoEngine.SignData(message, privateKey);

// Verify with public key
bool isValid = RsaCryptoEngine.VerifyData(message, signature, publicKey);
```

### Validate Key Pair

```csharp
bool isValid = RsaCryptoEngine.ValidateKeyPair(publicKey, privateKey);
```

### Key Import/Export

```csharp
// Import from PEM format
var csp = RsaCryptoEngine.ImportPemPublicKey(publicKeyPem);
var csp = RsaCryptoEngine.ImportPemPrivateKey(privateKeyPem);

// Import from XML format
var csp = RsaCryptoEngine.ImportFromXml(xmlContent);

// Export to PEM format
string publicKeyPem = csp.ExportPublicKeyToPem();
string privateKeyPem = csp.ExportPrivateKeyToPem();

// Export as inline string (no PEM headers)
string publicKeyInline = csp.ExportPublicKeyToPem(inlineString: true);
```

---

## AES Encryption (AesCryptoEngine)

Recommended symmetric encryption using AES algorithm. Supports key sizes of 128, 192, or 256 bits.

### Basic Usage

```csharp
using DotNetBrightener.CryptoEngine;

string plainText = "Secret message";
string key = "MyEncryptionKey123"; // Max 32 characters

// Encrypt
string encrypted = AesCryptoEngine.Encrypt(plainText, key);

// Decrypt
string decrypted = AesCryptoEngine.Decrypt(encrypted, key);

// Safe decrypt (returns false on failure)
if (AesCryptoEngine.TryDecrypt(encrypted, out string result, key))
{
    Console.WriteLine($"Decrypted: {result}");
}
```

### Key Size Behavior

The encryption key length determines the AES key size:

| Key Length | AES Key Size |
|------------|--------------|
| <= 16 chars | 128-bit |
| 17-24 chars | 192-bit |
| 25-32 chars | 256-bit |

> **Note:** Maximum key length is 32 characters.

---

## TripleDES Encryption (TripleDesCryptoEngine)

> **Warning:** TripleDES is deprecated. Use `AesCryptoEngine` for new implementations.

```csharp
using DotNetBrightener.CryptoEngine;

string plainText = "Secret message";
string key = "EncryptionKey";

// Encrypt
string encrypted = TripleDesCryptoEngine.Encrypt(plainText, key);

// Decrypt
string decrypted = TripleDesCryptoEngine.Decrypt(encrypted, key);

// Safe decrypt
bool success = TripleDesCryptoEngine.TryDecrypt(encrypted, out string result, key);
```

---

## Password Utilities

### Generate Password Salt

```csharp
using DotNetBrightener.CryptoEngine;

// Generate with random size
string salt = PasswordUtilities.CreatePasswordSalt();

// Generate with specific size
string salt = PasswordUtilities.CreatePasswordSalt(saltSize: 32);
```

### Generate Random Strings

```csharp
// Alphanumeric string
string random = PasswordUtilities.GenerateRandomString(length: 16);

// Include symbols
string withSymbols = PasswordUtilities.GenerateRandomString(length: 16, includeSymbols: true);

// Numeric only
string numeric = PasswordUtilities.GenerateRandomNumericString(length: 6); // OTP codes
```

### JavaScript Base64 Compatibility

```csharp
// Equivalent to JavaScript btoa()
string encoded = PasswordUtilities.JsBtoAString("input string");

// Equivalent to JavaScript atob()
string decoded = PasswordUtilities.JsAtoBString(encoded);
```

---

## Crypto Utilities

### Random Token Generation

```csharp
using DotNetBrightener.CryptoEngine;

// Generate random token
string token = CryptoUtilities.CreateRandomToken(64);

// Let system choose random size
string token = CryptoUtilities.CreateRandomToken(0);
```

### Time-based Tokens

```csharp
string originalToken = null;

// Generate token that expires in 5 minutes (default)
string timeToken = CryptoUtilities.GenerateTimeBasedToken(ref originalToken);

// Generate token with custom expiration
string timeToken = CryptoUtilities.GenerateTimeBasedToken(ref originalToken, TimeSpan.FromHours(1));

// Validate token
if (CryptoUtilities.ValidateTimeBasedToken(timeToken, out string tokenData))
{
    Console.WriteLine($"Valid! Token data: {tokenData}");
}
else
{
    Console.WriteLine("Token expired or invalid");
}
```

---

## Password Validation Provider

### Interface

```csharp
public interface IPasswordValidationProvider
{
    /// <summary>
    /// Generates an encrypted password and its associated key
    /// </summary>
    Tuple<string, string> GenerateEncryptedPassword(string plainTextPassword);

    /// <summary>
    /// Validates the plain text password
    /// </summary>
    bool ValidatePassword(string plainTextPassword, string passwordEncryptionKey, string hashedPassword);
}
```

### Usage

```csharp
public class UserService
{
    private readonly IPasswordValidationProvider _passwordProvider;

    public UserService(IPasswordValidationProvider passwordProvider)
    {
        _passwordProvider = passwordProvider;
    }

    public void RegisterUser(string email, string password)
    {
        var (encryptedKey, hashedPassword) = _passwordProvider.GenerateEncryptedPassword(password);

        // Store encryptedKey and hashedPassword in database
        // encryptedKey = password salt (encrypted with RSA)
        // hashedPassword = AES encrypted password with the salt
    }

    public bool VerifyPassword(string email, string inputPassword, string storedKey, string storedHash)
    {
        return _passwordProvider.ValidatePassword(inputPassword, storedKey, storedHash);
    }
}
```

---

## RSA Key Loaders

### FileRSAKeysLoader

Loads or generates RSA keys from files in `enc_keys/` folder.

```csharp
builder.Services.AddSingleton<FileRSAKeysLoader>(sp =>
    new FileRSAKeysLoader("/path/to/app/root"));
```

Keys are stored as:
- `enc_keys/public.key` - Public key (PEM format)
- `enc_keys/private.key` - Private key (PEM format)

### EnvironmentVarISAKeysLoader

Loads RSA keys from environment variables.

```csharp
builder.Services.Configure<CryptoEngineConfiguration>(options =>
{
    options.RsaKeyLoader = "EnvVarLoader";
    options.RsaEnvironmentVariableName = "MY_RSA_PRIVATE_KEY";
});
```

Set the environment variable:
```bash
# Linux/macOS
export MY_RSA_PRIVATE_KEY="-----BEGIN RSA PRIVATE KEY-----..."

# Windows PowerShell
$env:MY_RSA_PRIVATE_KEY="-----BEGIN RSA PRIVATE KEY-----..."

# Azure App Service
# Add Application Setting: MY_RSA_PRIVATE_KEY
```

---

## Configuration

```csharp
public class CryptoEngineConfiguration
{
    /// <summary>
    /// Name of the RSA key loader to use: "FileLoader" or "EnvVarLoader"
    /// </summary>
    public string RsaKeyLoader { get; set; }

    /// <summary>
    /// Environment variable name for RSA private key (default: "RSAPrivateKey")
    /// </summary>
    public string RsaEnvironmentVariableName { get; set; } = "RSAPrivateKey";
}
```

### appsettings.json

```json
{
  "CryptoEngineConfiguration": {
    "RsaKeyLoader": "EnvVarLoader",
    "RsaEnvironmentVariableName": "RSA_PRIVATE_KEY"
  }
}
```

---

## Dependencies

- `Microsoft.Extensions.Configuration`
- `Microsoft.Extensions.Logging.Abstractions`
- `Microsoft.Extensions.Options`
- `Portable.BouncyCastle` - For PEM key handling
- `System.Security.Cryptography.Xml`

---

## Security Considerations

1. **Key Storage**: Store private keys securely. Use environment variables or secure key vaults in production.

2. **AES Key Length**: Use 32-character keys for AES-256 encryption.

3. **Deprecated Algorithms**: Avoid `TripleDesCryptoEngine` and `SymmetricCryptoEngine` for new implementations.

4. **Password Storage**: Always use `IPasswordValidationProvider` for password hashing - it combines RSA and AES encryption.

5. **Key Rotation**: Implement key rotation policies for long-running applications.

---

## API Reference

| Class | Description |
|-------|-------------|
| `ICryptoEngine` | Main interface for encryption operations |
| `DefaultCryptoEngine` | Default implementation using RSA encryption |
| `RsaCryptoEngine` | Static RSA encryption utilities |
| `AesCryptoEngine` | Static AES encryption utilities |
| `TripleDesCryptoEngine` | Static TripleDES encryption (deprecated) |
| `CryptoUtilities` | Token generation utilities |
| `PasswordUtilities` | Password-related utilities |
| `IPasswordValidationProvider` | Password hashing interface |
| `IRSAKeysLoader` | RSA key loading interface |
| `FileRSAKeysLoader` | File-based key loader |
| `EnvironmentVarISAKeysLoader` | Environment variable key loader |
