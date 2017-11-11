using Newtonsoft.Json.Linq;

namespace Serilog.Sinks.Scalyr
{
  class ScalyrEvent
  {
    public string Ts { get; set; }
    public int Sev { get; set; }
    public JObject Attrs { get; set; }
  }
}