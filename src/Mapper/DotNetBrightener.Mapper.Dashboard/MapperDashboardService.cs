using System.Reflection;
using System.Text;
using Microsoft.Extensions.Options;

namespace DotNetBrightener.Mapper.Dashboard;

/// <summary>
///     Service that provides dashboard functionality.
/// </summary>
public sealed class MapperDashboardService
{
    private readonly DotNetBrightenerMapperDashboardOptions _options;
    private IReadOnlyList<MappingSourceInfo>? _cachedMappings;

    // SVG icons as constants
    private const string CheckIcon = @"<svg viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""3""><polyline points=""20,6 9,17 4,12""/></svg>";
    private const string XIcon = @"<svg viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2""><line x1=""18"" y1=""6"" x2=""6"" y2=""18""/><line x1=""6"" y1=""6"" x2=""18"" y2=""18""/></svg>";

    /// <summary>
    ///     Creates a new instance of the <see cref="MapperDashboardService"/>.
    /// </summary>
    public MapperDashboardService(IOptions<DotNetBrightenerMapperDashboardOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    ///     Gets all discovered target mappings.
    /// </summary>
    public IReadOnlyList<MappingSourceInfo> GetMappingTypes()
    {
        if (_cachedMappings != null)
            return _cachedMappings;

        var assemblies = new HashSet<Assembly>();

        // Add entry assembly and its references
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly != null)
        {
            assemblies.Add(entryAssembly);
            foreach (var reference in entryAssembly.GetReferencedAssemblies())
            {
                try
                {
                    assemblies.Add(Assembly.Load(reference));
                }
                catch
                {
                    // Skip unloadable assemblies
                }
            }
        }

        // Add additional assemblies from options
        foreach (var assembly in _options.AdditionalAssemblies)
        {
            assemblies.Add(assembly);
        }

        _cachedMappings = MappingDiscovery.DiscoverMappingTypes(assemblies);
        return _cachedMappings;
    }

    /// <summary>
    ///     Gets the dashboard HTML page.
    /// </summary>
    public string GetDashboardHtml()
    {
        var mappings = GetMappingTypes();
        return GenerateDashboardHtml(mappings);
    }

    private string GenerateDashboardHtml(IReadOnlyList<MappingSourceInfo> mappings)
    {
        var totalTargets = mappings.Sum(m => m.MappingTypes.Count);
        var sourceCards = GenerateSourceCards(mappings);
        var emptyState = mappings.Count == 0 ? TemplateEngine.LoadTemplate("empty-state.html") : "";

        var tokens = new Dictionary<string, string>
        {
            { "title", EscapeHtml(_options.Title) },
            { "accentColor", _options.AccentColor },
            { "sourceCount", mappings.Count.ToString() },
            { "targetCount", totalTargets.ToString() },
            { "sourceCards", sourceCards },
            { "emptyState", emptyState }
        };

        return TemplateEngine.RenderTemplate("dashboard.html", tokens);
    }

    private string GenerateSourceCards(IReadOnlyList<MappingSourceInfo> mappings)
    {
        var sb = new StringBuilder();

        foreach (var mapping in mappings)
        {
            var initial = mapping.SourceTypeSimpleName.Length > 0
                ? mapping.SourceTypeSimpleName[0].ToString().ToUpperInvariant()
                : "?";
            var targetCount = mapping.MappingTypes.Count;

            var tokens = new Dictionary<string, string>
            {
                { "initial", initial },
                { "sourceName", EscapeHtml(mapping.SourceTypeSimpleName) },
                { "sourceNamespace", EscapeHtml(mapping.SourceTypeNamespace ?? "") },
                { "targetCount", targetCount.ToString() },
                { "targetPlural", targetCount != 1 ? "s" : "" },
                { "propertyCount", mapping.SourceMembers.Count.ToString() },
                { "membersTable", GenerateMembersTable(mapping.SourceMembers) },
                { "targetCards", GenerateTargetCards(mapping.MappingTypes) }
            };

            sb.AppendLine(TemplateEngine.RenderTemplate("source-card.html", tokens));
        }

        return sb.ToString();
    }

    private string GenerateMembersTable(IReadOnlyList<MappingMemberInfo> members)
    {
        if (members.Count == 0)
            return @"<p style=""color: var(--text-secondary); font-size: 0.875rem;"">No public members found.</p>";

        var sb = new StringBuilder();

        foreach (var member in members)
        {
            var badges = new List<string>();
            if (member.IsNullable) badges.Add(@"<span class=""member-badge member-badge-nullable"">Nullable</span>");
            if (member.IsRequired) badges.Add(@"<span class=""member-badge member-badge-required"">Required</span>");
            if (member.IsInitOnly) badges.Add(@"<span class=""member-badge member-badge-init"">Init</span>");
            if (member.IsCollection) badges.Add(@"<span class=""member-badge member-badge-collection"">Collection</span>");
            if (member.IsNestedTarget) badges.Add(@"<span class=""member-badge member-badge-nested"">Nested Target</span>");

            var rowTokens = new Dictionary<string, string>
            {
                { "memberName", EscapeHtml(member.Name) },
                { "memberType", EscapeHtml(member.TypeName) },
                { "memberBadges", string.Join("", badges) }
            };

            sb.AppendLine(TemplateEngine.RenderTemplate("member-row.html", rowTokens));
        }

        var tableTokens = new Dictionary<string, string>
        {
            { "memberRows", sb.ToString() }
        };

        return TemplateEngine.RenderTemplate("members-table.html", tableTokens);
    }

    private string GenerateTargetCards(IReadOnlyList<MappingTypeInfo> targets)
    {
        var sb = new StringBuilder();

        foreach (var target in targets)
        {
            var exclusions = target.ExcludedProperties.Count > 0
                ? $@"<div class=""target-config"">Excludes: {EscapeHtml(string.Join(", ", target.ExcludedProperties))}</div>"
                : "";

            var inclusions = target.IncludedProperties?.Count > 0
                ? $@"<div class=""target-config"">Includes: {EscapeHtml(string.Join(", ", target.IncludedProperties))}</div>"
                : "";

            var tokens = new Dictionary<string, string>
            {
                { "targetName", EscapeHtml(target.MappingTypeSimpleName) },
                { "typeKind", EscapeHtml(target.TypeKind) },
                { "constructorClass", target.HasConstructor ? "feature-enabled" : "feature-disabled" },
                { "constructorIcon", target.HasConstructor ? CheckIcon : XIcon },
                { "projectionClass", target.HasProjection ? "feature-enabled" : "feature-disabled" },
                { "projectionIcon", target.HasProjection ? CheckIcon : XIcon },
                { "toSourceClass", target.HasToSource ? "feature-enabled" : "feature-disabled" },
                { "toSourceIcon", target.HasToSource ? CheckIcon : XIcon },
                { "exclusions", exclusions },
                { "inclusions", inclusions },
                { "memberCount", target.Members.Count.ToString() },
                { "memberPlural", target.Members.Count != 1 ? "s" : "" }
            };

            sb.AppendLine(TemplateEngine.RenderTemplate("target-card.html", tokens));
        }

        return sb.ToString();
    }

    private static string EscapeHtml(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }
}
