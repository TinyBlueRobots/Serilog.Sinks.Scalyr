using System;
using System.Collections.Generic;
using System.Linq;
using Serilog.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Serilog.Formatting.Json;
using System.IO;
using Serilog.Formatting.Display;
using System.Net;

namespace Serilog.Sinks.Scalyr
{
  internal class NewtonsoftScalyrFormatter : IScalyrFormatter
  {
    private readonly ScalyrSession _session;
    private readonly JsonSerializerSettings _jsonSerializerSettings;
    private readonly JsonValueFormatter _jsonValueFormatter = new JsonValueFormatter(null);
    private readonly MessageTemplateTextFormatter _messageTemplateTextFormatter;
    private long _lastTimeStamp;

    public NewtonsoftScalyrFormatter(string token, string logfile, object sessionInfo, MessageTemplateTextFormatter messageTemplateTextFormatter)
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
      _session = new ScalyrSession { Token = token, Session = Guid.NewGuid().ToString("N") };
      JObject sessionObject = JObject.FromObject(sessionInfo ?? new object())
                              ?? throw new InvalidOperationException("Could not serialize session info");
      sessionObject.Add("serverHost", getHostName());
      sessionObject.Add("logfile", logfile);
      _session.SessionInfo = sessionObject;
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

    public ScalyrEvent MapToScalyrEvent(LogEvent logEvent, int index)
    {
      var attrs = new JObject();
      foreach (var property in logEvent.Properties)
      {
        using (var json = new StringWriter())
        {
          _jsonValueFormatter.Format(property.Value, json);
          attrs.Add(property.Key, JToken.Parse(json.ToString()));
        }
      }

      if (logEvent.Exception != null)
      {
        attrs.Add("Exception", JObject.FromObject(logEvent.Exception));
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

      _lastTimeStamp = Math.Max(_lastTimeStamp + 1, logEvent.Timestamp.ToUnixTimeMilliseconds() * 1000000 + index);

      return new ScalyrEvent
      {
        Ts = _lastTimeStamp.ToString(),
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