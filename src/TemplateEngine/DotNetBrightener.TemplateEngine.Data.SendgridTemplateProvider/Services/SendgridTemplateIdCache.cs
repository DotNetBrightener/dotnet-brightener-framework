using System.Collections.Concurrent;

namespace DotNetBrightener.TemplateEngine.Data.SendgridTemplateProvider.Services;

/// <summary>
///     In-memory cache implementation for storing Sendgrid template ID mappings.
///     This cache is populated during template registration at application startup.
/// </summary>
internal class SendgridTemplateIdCache : ISendgridTemplateIdCache
{
    private readonly ConcurrentDictionary<string, string> _templateIdsByTypeName = new();

    /// <inheritdoc />
    public bool TryGetTemplateId(string templateTypeName, out string templateId)
    {
        return _templateIdsByTypeName.TryGetValue(templateTypeName, out templateId!);
    }

    /// <inheritdoc />
    public void SetTemplateId(string templateTypeName, string templateId)
    {
        _templateIdsByTypeName[templateTypeName] = templateId;
    }

    /// <inheritdoc />
    public void Clear()
    {
        _templateIdsByTypeName.Clear();
    }
}

