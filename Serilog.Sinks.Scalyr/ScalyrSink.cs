using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.Scalyr;

class ScalyrSink : PeriodicBatchingSink
{
    public const int DefaultBatchPostingLimit = 10;
    static readonly HttpClient _httpClient = new();
    public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(5);
    readonly IScalyrFormatter _scalyrFormatter;
    readonly Uri _scalyrUri = new("https://www.scalyr.com");

    public ScalyrSink(IScalyrFormatter scalyrFormatter, Uri scalyrUri, int batchSizeLimit, TimeSpan period) : base(
        batchSizeLimit, period)
    {
        _scalyrFormatter = scalyrFormatter;
        _scalyrUri = new Uri(scalyrUri ?? _scalyrUri, "addEvents");
    }

    public ScalyrSink(IScalyrFormatter scalyrFormatter, Uri scalyrUri, int batchSizeLimit, TimeSpan period,
        int queueLimit) : base(batchSizeLimit, period, queueLimit)
    {
        _scalyrFormatter = scalyrFormatter;
        _scalyrUri = new Uri(scalyrUri ?? _scalyrUri, "addEvents");
    }

    protected override async Task EmitBatchAsync(IEnumerable<LogEvent> events)
    {
        var scalyrEvents = _scalyrFormatter.Format(events);
        using (var content = new StringContent(scalyrEvents))
        {
            await _httpClient.PostAsync(_scalyrUri, content);
        }
    }
}