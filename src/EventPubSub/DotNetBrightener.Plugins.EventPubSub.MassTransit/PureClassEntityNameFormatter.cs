using MassTransit;

namespace DotNetBrightener.Plugins.EventPubSub.MassTransit;

internal class PureClassEntityNameFormatter : IEntityNameFormatter
{
    public string FormatEntityName<T>()
    {
        return typeof(T).Name;
    }
}