using System.Collections.Generic;
using Serilog.Events;

namespace Serilog.Sinks.Scalyr;

/// <summary>
///     Converts Serilog <see cref="LogEvent" />s to <see cref="ScalyrEvent" />s.
/// </summary>
interface IScalyrFormatter
{
    /// <summary>
    ///     Convert a Serilog <see cref="LogEvent" /> to a <see cref="ScalyrEvent" />.
    /// </summary>
    /// <param name="logEvent">Serilog log event to convert.</param>
    /// <param name="index">Relative index of <paramref name="logEvent" /> used to generate a timestamp.</param>
    /// <returns>Converted <see cref="ScalyrEvent" />.</returns>
    ScalyrEvent MapToScalyrEvent(LogEvent logEvent, int index);

    /// <summary>
    ///     Serializes a sequence of Serilog <see cref="LogEvent" />s.
    /// </summary>
    /// <param name="events">Sequence of log events to serialize.</param>
    /// <returns>Serialized form of <paramref name="events" /></returns>
    string Format(IEnumerable<LogEvent> events);
}