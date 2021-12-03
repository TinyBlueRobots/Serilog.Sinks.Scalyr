using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Formatting.Json;

namespace Serilog.Sinks.Scalyr
{
  internal class SystemTextJsonScalyrFormatter : IScalyrFormatter
  {
    private readonly JsonSerializerOptions _jsonSerializerSettings;
    private readonly JsonValueFormatter _jsonValueFormatter = new JsonValueFormatter(null);
    private readonly MessageTemplateTextFormatter _messageTemplateTextFormatter;
    private readonly ScalyrSession _session;
    private long _lastTimeStamp;


    public SystemTextJsonScalyrFormatter(string token, string logfile, object sessionInfo, MessageTemplateTextFormatter messageTemplateTextFormatter)
    {
      _jsonSerializerSettings = new JsonSerializerOptions
      {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      };
      _messageTemplateTextFormatter = messageTemplateTextFormatter;
      _session = new ScalyrSession { Token = token, Session = Guid.NewGuid().ToString("N") };
      JsonNode sessionObject = JsonSerializer.SerializeToNode(sessionInfo ?? new object(), _jsonSerializerSettings)
                               ?? throw new InvalidOperationException("Could not serialize session info");
      sessionObject["serverHost"] = JsonValue.Create(GetHostName());
      sessionObject["logfile"] = JsonValue.Create(logfile);
      _session.SessionInfo = sessionObject;
    }

    /// <inheritdoc />
    public ScalyrEvent MapToScalyrEvent(LogEvent logEvent, int index) // var attrs_ = new JsonObject();
    {
      var attrs = new JsonObject();
      foreach (KeyValuePair<string, LogEventPropertyValue> property in logEvent.Properties)
      {
        using (var json = new StringWriter())
        {
          _jsonValueFormatter.Format(property.Value, json);
          attrs.Add(property.Key, JsonNode.Parse(json.ToString()));
        }
      }

      if (logEvent.Exception != null)
      {
        attrs.Add("Exception", JsonSerializer.Serialize(logEvent.Exception, _jsonSerializerSettings));
      }

      using (var stringWriter = new StringWriter())
      {
        if (_messageTemplateTextFormatter != null)
        {
          _messageTemplateTextFormatter.Format(logEvent, stringWriter);
        }
        else
        {
          stringWriter.Write(logEvent.RenderMessage());
        }

        attrs.Add("message", stringWriter.ToString());
      }

      long ts = logEvent.Timestamp.ToUnixTimeMilliseconds() * 1000000 + index;
      while (ts <= _lastTimeStamp)
      {
        ts++;
      }

      _lastTimeStamp = ts;
      return new ScalyrEvent
      {
        Ts = ts.ToString(),
        Sev = (int)logEvent.Level + 1,
        Attrs = attrs
      };
    }

    public string Format(IEnumerable<LogEvent> events)
    {
      _session.Events = events.Select(MapToScalyrEvent);
      string json = JsonSerializer.Serialize(_session, _jsonSerializerSettings);
      return json;
    }

    private static string GetHostName()
    {
      try
      {
        return Dns.GetHostName();
      }
      catch
      {
        return new[] { "COMPUTERNAME", "HOSTNAME" }.Select(Environment.GetEnvironmentVariable).FirstOrDefault() ?? "SERVERHOST";
      }
    }
  }
}