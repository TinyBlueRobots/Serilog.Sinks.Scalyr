using System.Collections.Generic;

namespace Serilog.Sinks.Scalyr;

class ScalyrSession
{
    public string Token { get; set; }
    public string Session { get; set; }
    public IEnumerable<ScalyrEvent> Events { get; set; }
    public object SessionInfo { get; set; }
}