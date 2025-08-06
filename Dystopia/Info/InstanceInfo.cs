using System.Net;
using System.Reflection;

namespace Dystopia.Info;

public class InstanceInfo
{
    public string Service { get; init; } = "Dystopia";
    public string InstanceName { get; init; }
    public string Version { get; init; }
    public string Uptime { get; init; }
    public DateTime TimestampUtc { get; init; }

    public static InstanceInfo Create(DateTime startTime, InstanceInfoSettings settings)
    {
        var uptimeSpan = DateTime.UtcNow - startTime;
        var uptime = $"{(int)uptimeSpan.TotalDays}d:{uptimeSpan.Hours}h:{uptimeSpan.Minutes}m:{uptimeSpan.Seconds}s";

        return new InstanceInfo
        {
            InstanceName = settings.InstanceName,
            Version = settings.Version,
            Uptime = uptime,
            TimestampUtc = DateTime.UtcNow
        };
    }
}