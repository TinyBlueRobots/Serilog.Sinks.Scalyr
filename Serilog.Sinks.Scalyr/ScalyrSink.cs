using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;
using System.Net.Http;

namespace Serilog.Sinks.Scalyr
{
  class ScalyrSink : PeriodicBatchingSink
  {
    readonly ScalyrFormatter _scalyrFormatter;
    readonly Uri _scalyrUri = new Uri("https://www.scalyr.com");
    static readonly HttpClient _httpClient = new HttpClient();
    public const int DefaultBatchPostingLimit = 10;
    public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(5);

    public ScalyrSink(ScalyrFormatter scalyrFormatter, Uri scalyrUri, int batchSizeLimit, TimeSpan period) : base(batchSizeLimit, period)
    {
      _scalyrFormatter = scalyrFormatter;
      _scalyrUri = new Uri(scalyrUri ?? _scalyrUri, "addEvents");
    }

    public ScalyrSink(ScalyrFormatter scalyrFormatter, Uri scalyrUri, int batchSizeLimit, TimeSpan period, int queueLimit) : base(batchSizeLimit, period, queueLimit)
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
}