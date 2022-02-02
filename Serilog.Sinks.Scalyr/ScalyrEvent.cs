namespace Serilog.Sinks.Scalyr;

class ScalyrEvent
{
    public string Ts { get; set; }
    public int Sev { get; set; }
    public object Attrs { get; set; }
}