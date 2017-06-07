using System;
using Serilog.Configuration;
using Serilog.Sinks.Scalyr;

namespace Serilog
{
    /// <summary>
    /// Extends Serilog configuration to write events to Scalyr.
    /// </summary>
    public static class LoggerConfigurationSerilogExtensions
    {
    /// <summary>
    /// Adds a sink that writes log events to <a href="https://scalyr.com">Scalyr</a>.
    /// </summary>
    /// <param name="loggerSinkConfiguration">The logger configuration.</param>
    /// <param name="token">"Write Logs" API token. Find API tokens at https://www.scalyr.com/keys.</param>
    /// <param name="serverHost">Hostname or some other stable server identifier. Scalyr uses this value to organize events from different servers.</param>
    /// <param name="logfile">The name of the log file being written to.</param>
    /// <param name="batchSizeLimit">The maximum number of events to include in a single batch.</param>
    /// <param name="period">The time to wait between checking for event batches.</param>
    /// <param name="queueLimit">Maximum number of events in the queue.</param>
    /// <param name="sessionInfo">Additional information about the session.</param>
    /// <param name="scalyrUri">The base URI for Scalyr. Defaults to https://scalyr.com.</param>
    public static LoggerConfiguration Scalyr(this LoggerSinkConfiguration loggerSinkConfiguration, string token, string serverHost, string logfile, int? batchSizeLimit = null, TimeSpan? period = null, int? queueLimit = null, object sessionInfo = null, Uri scalyrUri = null)
        {
            var client = new ScalyrClient(token, serverHost, logfile, sessionInfo, scalyrUri);
            var sink =
                queueLimit.HasValue ?
                new ScalyrSink(client, batchSizeLimit ?? ScalyrSink.DefaultBatchPostingLimit, period ?? ScalyrSink.DefaultPeriod, queueLimit.Value) :
                new ScalyrSink(client, batchSizeLimit ?? ScalyrSink.DefaultBatchPostingLimit, period ?? ScalyrSink.DefaultPeriod) ;
            return loggerSinkConfiguration.Sink(sink);
        }
    }
}