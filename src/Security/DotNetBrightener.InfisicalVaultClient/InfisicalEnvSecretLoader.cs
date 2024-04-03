namespace DotNetBrightener.InfisicalVaultClient;

public static class InfisicalEnvSecretLoader
{
    private static InfisicalSecretClient _singleInstance;

    public static InfisicalSecretClient VaultSecretClient => _singleInstance;


    public static void InitializeEnvSecret()
    {
        var enableSecretVault = Environment.GetEnvironmentVariable("SecretsManagement__Enabled");

        if (enableSecretVault?.Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase) == false)
        {
            return;
        }

        var vaultUrl         = Environment.GetEnvironmentVariable("SecretsManagement__VaultUrl");
        var vaultAccessToken = Environment.GetEnvironmentVariable("SecretsManagement__VaultAccessKey");
        var vaultProjectId   = Environment.GetEnvironmentVariable("SecretsManagement__ProjectID");
        var vaultEnvironment = Environment.GetEnvironmentVariable("SecretsManagement__Environment") ?? "dev";

        var secretClient = new InfisicalSecretClient(vaultUrl, vaultAccessToken);
        secretClient.ChangeEnvironment(vaultEnvironment);

        var secrets = secretClient.RetrieveSecrets(vaultProjectId)
                                  .Result;

        foreach (var secretInformation in secrets)
        {
            Environment.SetEnvironmentVariable(secretInformation.SecretKey, secretInformation.SecretValue);
        }

        _singleInstance = secretClient;
    }
}