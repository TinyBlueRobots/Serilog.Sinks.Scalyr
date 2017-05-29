using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.Scalyr
{
    class ScalyrSink : PeriodicBatchingSink
    {
        readonly ScalyrClient _scalyrClient;
        public const int DefaultBatchPostingLimit = 10;
        public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(5);

        public ScalyrSink(ScalyrClient scalyrClient, int batchSizeLimit, TimeSpan period) : base (batchSizeLimit, period)
        {
            _scalyrClient = scalyrClient;
        }

        public ScalyrSink(ScalyrClient scalyrClient, int batchSizeLimit, TimeSpan period, int queueLimit) : base (batchSizeLimit, period, queueLimit)
        {
            _scalyrClient = scalyrClient;
        }

        protected override Task EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            return _scalyrClient.Log(events);
        }
    }
}