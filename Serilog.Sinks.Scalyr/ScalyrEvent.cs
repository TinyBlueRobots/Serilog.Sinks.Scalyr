namespace Serilog.Sinks.Scalyr
{
  internal class ScalyrEvent
  {
    public string Ts { get; set; }
    public int Sev { get; set; }
    public object Attrs { get; set; }
  }
}