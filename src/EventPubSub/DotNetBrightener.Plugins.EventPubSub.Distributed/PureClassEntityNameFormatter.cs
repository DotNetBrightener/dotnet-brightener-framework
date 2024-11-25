using MassTransit;

namespace DotNetBrightener.Plugins.EventPubSub.Distributed;

internal class PureClassEntityNameFormatter : IEntityNameFormatter
{
    public string FormatEntityName<T>()
    {
        return typeof(T).Name;
    }
}