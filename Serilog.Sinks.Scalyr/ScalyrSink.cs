using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;
using System.Net.Http;

namespace Serilog.Sinks.Scalyr
{
  class ScalyrSink : PeriodicBatchingSink
  {
    readonly ScalyrFormatter _scalyrFormatter;
    readonly HttpClient _httpClient = new HttpClient();
    public const int DefaultBatchPostingLimit = 10;
    public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(5);

    public ScalyrSink(ScalyrFormatter scalyrFormatter, Uri scalyrUri, int batchSizeLimit, TimeSpan period) : base(batchSizeLimit, period)
    {
      _scalyrFormatter = scalyrFormatter;
      _httpClient.BaseAddress = scalyrUri ?? new Uri("https://www.scalyr.com");
    }

    public ScalyrSink(ScalyrFormatter scalyrFormatter, Uri scalyrUri, int batchSizeLimit, TimeSpan period, int queueLimit) : base(batchSizeLimit, period, queueLimit)
    {
      _scalyrFormatter = scalyrFormatter;
      _httpClient.BaseAddress = scalyrUri ?? new Uri("https://www.scalyr.com");
    }

    protected override Task EmitBatchAsync(IEnumerable<LogEvent> events)
    {
      var scalyrEvents = _scalyrFormatter.Format(events);
      return _httpClient.PostAsync("addEvents", new StringContent(scalyrEvents));
    }
  }
}