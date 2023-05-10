using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DotNetBrightener.InfisicalVaultClient;

public class InfisicalSecretsListResponse
{
    public List<InfisicalSecret> Secrets { get; set; }
}

public class InfisicalSecretResponse
{
    public InfisicalSecret Secret { get; set; }
}

public class SecretInformation
{
    public string SecretKey   { get; set; }

    public string SecretValue { get; set; }
}

public class InfisicalServiceTokenResponse
{
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; set; }

    [JsonProperty("workspace", NullValueHandling = NullValueHandling.Ignore)]
    public string Workspace { get; set; }

    [JsonProperty("environment", NullValueHandling = NullValueHandling.Ignore)]
    public string Environment { get; set; }

    [JsonProperty("lastUsed", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime LastUsed { get; set; }

    [JsonProperty("encryptedKey", NullValueHandling = NullValueHandling.Ignore)]
    public string EncryptedKey { get; set; }

    [JsonProperty("iv", NullValueHandling = NullValueHandling.Ignore)]
    public string Iv { get; set; }

    [JsonProperty("tag", NullValueHandling = NullValueHandling.Ignore)]
    public string Tag { get; set; }

    [JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
    public List<string> Permissions { get; set; }

    [JsonProperty("createdAt", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("updatedAt", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime UpdatedAt { get; set; }

}

public class InfisicalSecret
{
    [JsonProperty("_id", NullValueHandling = NullValueHandling.Ignore)]
    public string Id { get; set; }

    [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
    public int Version { get; set; }

    [JsonProperty("workspace", NullValueHandling = NullValueHandling.Ignore)]
    public string Workspace { get; set; }

    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    public string Type { get; set; }

    [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
    public List<object> Tags { get; set; }

    [JsonProperty("environment", NullValueHandling = NullValueHandling.Ignore)]
    public string Environment { get; set; }

    [JsonProperty("secretKeyCiphertext", NullValueHandling = NullValueHandling.Ignore)]
    public string SecretKeyCiphertext { get; set; }

    [JsonProperty("secretKeyIV", NullValueHandling = NullValueHandling.Ignore)]
    public string SecretKeyIV { get; set; }

    [JsonProperty("secretKeyTag", NullValueHandling = NullValueHandling.Ignore)]
    public string SecretKeyTag { get; set; }

    [JsonProperty("secretValueCiphertext", NullValueHandling = NullValueHandling.Ignore)]
    public string SecretValueCiphertext { get; set; }

    [JsonProperty("secretValueIV", NullValueHandling = NullValueHandling.Ignore)]
    public string SecretValueIV { get; set; }

    [JsonProperty("secretValueTag", NullValueHandling = NullValueHandling.Ignore)]
    public string SecretValueTag { get; set; }

    [JsonProperty("secretCommentCiphertext", NullValueHandling = NullValueHandling.Ignore)]
    public string SecretCommentCiphertext { get; set; }

    [JsonProperty("secretCommentIV", NullValueHandling = NullValueHandling.Ignore)]
    public string SecretCommentIV { get; set; }

    [JsonProperty("secretCommentTag", NullValueHandling = NullValueHandling.Ignore)]
    public string SecretCommentTag { get; set; }

    [JsonProperty("path", NullValueHandling = NullValueHandling.Ignore)]
    public string Path { get; set; }

    [JsonProperty("__v", NullValueHandling = NullValueHandling.Ignore)]
    public int V { get; set; }

    [JsonProperty("createdAt", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("updatedAt", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime UpdatedAt { get; set; }
}