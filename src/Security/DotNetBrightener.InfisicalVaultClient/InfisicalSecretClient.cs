using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace DotNetBrightener.InfisicalVaultClient;

public class InfisicalSecretClient
{
    public const     string                        InfisicalUrl = "https://app.infisical.com";
    private readonly string                        _baseUrl;
    private readonly string                        _serviceToken;
    private          string                        _projectKey;
    private          string                        _environment = "dev";
    private          InfisicalServiceTokenResponse _serviceTokenData;

    public InfisicalSecretClient(string serviceToken)
        : this(InfisicalUrl, serviceToken)
    {
    }

    public InfisicalSecretClient(string baseUrl, string serviceToken)
    {
        _baseUrl      = baseUrl;
        _serviceToken = serviceToken;
    }

    public InfisicalSecretClient ChangeEnvironment(string environment)
    {
        _environment = environment;

        return this;
    }

    public async Task<string> RetrieveSecret(string secretKey,
                                             string projectId,
                                             string environment = null)
    {
        if (_serviceTokenData is null ||
            string.IsNullOrEmpty(_projectKey))
        {
            await RetrieveServiceTokenInfo();
        }

        if (string.IsNullOrEmpty(environment))
        {
            environment = _environment;
        }

        var requestUrl =
            $"{_baseUrl}/api/v3/secrets/{secretKey}?workspaceId={projectId}&environment={environment}";

        var result =
            await RequestInfisicalApi<InfisicalSecretResponse>(requestUrl,
                                                          _serviceToken,
                                                          HttpMethod.Get);

        var secret = result.Secret;

        var secretValue = Decrypt(secret.SecretValueCiphertext,
                                  _projectKey,
                                  secret.SecretValueIV,
                                  secret.SecretValueTag);


        return secretValue;
    }

    public async Task<List<SecretInformation>> RetrieveSecrets(string projectId,
                                                               string environment = null)
    {
        if (_serviceTokenData is null ||
            string.IsNullOrEmpty(_projectKey))
        {
            await RetrieveServiceTokenInfo();
        }

        if (string.IsNullOrEmpty(environment))
        {
            environment = _environment;
        }

        var requestUrl =
            $"{_baseUrl}/api/v3/secrets?workspaceId={projectId}&environment={environment}";

        var result =
            await RequestInfisicalApi<InfisicalSecretsListResponse>(requestUrl,
                                                          _serviceToken,
                                                          HttpMethod.Get);

        if (result == null)
        {
            return new List<SecretInformation>();
        }

        var secrets = result.Secrets;

        var secretResults = secrets.Select(secret => new SecretInformation
                                    {
                                        SecretKey = Decrypt(secret.SecretKeyCiphertext,
                                                            _projectKey,
                                                            secret.SecretKeyIV,
                                                            secret.SecretKeyTag),
                                        SecretValue = Decrypt(secret.SecretValueCiphertext,
                                                              _projectKey,
                                                              secret.SecretValueIV,
                                                              secret.SecretValueTag)
                                    })
                                   .ToList();


        return secretResults;
    }

    public async Task RetrieveServiceTokenInfo()
    {
        _serviceTokenData =
            await RequestInfisicalApi<InfisicalServiceTokenResponse>($"{_baseUrl}/api/v2/service-token", _serviceToken);

        var serviceTokenSecret = _serviceToken.Substring(_serviceToken.LastIndexOf('.') + 1);

        _projectKey = Decrypt(
                              _serviceTokenData.EncryptedKey,
                              serviceTokenSecret,
                              _serviceTokenData.Iv,
                              _serviceTokenData.Tag
                             );
    }

    static async Task<TResult> RequestInfisicalApi<TResult>(string url, string serviceToken, HttpMethod method = null)
        where TResult : class, new()
    {

        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method     = method ?? HttpMethod.Get,
            RequestUri = new Uri(url),
            Headers =
            {
                {
                    "Authorization", $"Bearer {serviceToken}"
                }
            }
        };

        using var response = await client.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var responseStream = await response.Content.ReadAsStringAsync();
            var data           = JsonConvert.DeserializeObject<TResult>(responseStream);

            Console.WriteLine($"Data received: {responseStream}");

            return data;
        }

        return null;
    }

    internal static string Decrypt(string cipherText, string key, string iv, string tag)
    {
        var cipherTextBytes = Convert.FromBase64String(cipherText);
        var keyBytes        = Encoding.UTF8.GetBytes(key);
        var ivBytes         = Convert.FromBase64String(iv);
        var tagBytes        = Convert.FromBase64String(tag);

        byte[] decrypted;

        var plaintextBytes = new byte[cipherTextBytes.Length];

        var cipher     = new GcmBlockCipher(new AesEngine());
        var parameters = new AeadParameters(new KeyParameter(keyBytes), 128, ivBytes);
        cipher.Init(false, parameters);

        var bcCipherText = cipherTextBytes.Concat(tagBytes).ToArray();

        var offset = cipher.ProcessBytes(bcCipherText, 0, bcCipherText.Length, plaintextBytes, 0);
        cipher.DoFinal(plaintextBytes, offset);

        return Encoding.UTF8.GetString(plaintextBytes);
    }
}