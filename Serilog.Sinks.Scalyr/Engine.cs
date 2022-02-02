namespace Serilog.Sinks.Scalyr;

/// <summary>
///     Type of serialization method to use when writing log events to Scalyr.
/// </summary>
public enum Engine
{
    /// <summary>
    ///     Use Newtonsoft.Json to serialize log events.
    /// </summary>
    Newtonsoft,

    /// <summary>
    ///     Use System.Text.Json to serialize log events.
    /// </summary>
    SystemTextJson
}