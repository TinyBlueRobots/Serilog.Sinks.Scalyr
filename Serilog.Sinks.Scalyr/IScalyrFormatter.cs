using System.Collections.Generic;
using Serilog.Events;

namespace Serilog.Sinks.Scalyr
{
    internal interface IScalyrFormatter
    {
        ScalyrEvent MapToScalyrEvent(LogEvent logEvent, int index);
        string Format(IEnumerable<LogEvent> events);
    }
}