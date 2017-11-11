using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Serilog.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Threading;
using Serilog.Formatting.Json;
using System.IO;
using Serilog.Formatting.Display;
using System.Net;

namespace Serilog.Sinks.Scalyr
{
  class ScalyrFormatter
  {
    readonly ScalyrSession _session;
    readonly JsonSerializerSettings _jsonSerializerSettings;
    readonly JsonValueFormatter jsonValueFormatter = new JsonValueFormatter(null);
    readonly MessageTemplateTextFormatter _messageTemplateTextFormatter;
    long lastTimeStamp;

    public ScalyrFormatter(string token, string logfile, object sessionInfo, MessageTemplateTextFormatter messageTemplateTextFormatter)
    {
      _jsonSerializerSettings = new JsonSerializerSettings
      {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new DefaultContractResolver
        {
          NamingStrategy = new CamelCaseNamingStrategy()
        }
      };
      _messageTemplateTextFormatter = messageTemplateTextFormatter;
      _session = new ScalyrSession { Token = token, Session = Guid.NewGuid().ToString("N"), SessionInfo = JObject.FromObject(sessionInfo ?? new object()) };
      _session.SessionInfo.Add("serverHost", getHostName());
      _session.SessionInfo.Add("logfile", logfile);
    }

    string getHostName()
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

    ScalyrEvent MapToScalyrEvent(LogEvent logEvent, int index)
    {
      var attrs = new JObject();
      foreach (var property in logEvent.Properties)
      {
        using (var json = new StringWriter())
        {
          jsonValueFormatter.Format(property.Value, json);
          attrs.Add(property.Key, JToken.Parse(json.ToString()));
        }
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
      var ts = logEvent.Timestamp.ToUnixTimeMilliseconds() * 1000000 + index;
      while (ts <= lastTimeStamp) { ts++; }
      lastTimeStamp = ts;
      return new ScalyrEvent
      {
        Ts = ts.ToString(),
        Sev = ((int)logEvent.Level) + 1,
        Attrs = attrs
      };
    }

    public string Format(IEnumerable<LogEvent> events)
    {
      _session.Events = events.Select(MapToScalyrEvent);
      var json = JsonConvert.SerializeObject(_session, _jsonSerializerSettings);
      return json;
    }
  }
}