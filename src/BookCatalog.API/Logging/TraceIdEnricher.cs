using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace BookCatalog.API.Logging;

public class TraceIdEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var traceId = Activity.Current?.TraceId.ToString();

        if(traceId is not null)
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("TraceId", traceId));
        }
    }
}