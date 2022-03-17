using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Formatting.Json;

namespace Serilog.Sinks.Scalyr;

class NewtonsoftScalyrFormatter : IScalyrFormatter
{
    readonly JsonSerializerSettings _jsonSerializerSettings;
    readonly JsonValueFormatter _jsonValueFormatter = new(null);
    readonly MessageTemplateTextFormatter _messageTemplateTextFormatter;
    readonly ScalyrSession _session;
    long _lastTimeStamp;

    public NewtonsoftScalyrFormatter(string token, string logfile, object sessionInfo,
        MessageTemplateTextFormatter messageTemplateTextFormatter)
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
        var sessionObject = JObject.FromObject(sessionInfo ?? new object())
                            ?? throw new InvalidOperationException("Could not serialize session info");
        sessionObject["serverHost"] = getHostName();
        sessionObject["logfile"] = logfile;
        _session.SessionInfo = sessionObject;
    }

    public ScalyrEvent MapToScalyrEvent(LogEvent logEvent, int index)
    {
        var attrs = new JObject();
        foreach (var property in logEvent.Properties)
            using (var json = new StringWriter())
            {
                _jsonValueFormatter.Format(property.Value, json);
                attrs[property.Key] = JToken.Parse(json.ToString());
            }

        if (logEvent.Exception != null) attrs["Exception"] = JObject.FromObject(logEvent.Exception);

        using (var stringWriter = new StringWriter())
        {
            if (_messageTemplateTextFormatter != null)
                _messageTemplateTextFormatter.Format(logEvent, stringWriter);
            else
                stringWriter.Write(logEvent.RenderMessage());

            attrs["message"] = stringWriter.ToString();
        }

        _lastTimeStamp = Math.Max(_lastTimeStamp + 1, logEvent.Timestamp.ToUnixTimeMilliseconds() * 1000000 + index);

        return new ScalyrEvent
        {
            Ts = _lastTimeStamp.ToString(),
            Sev = (int)logEvent.Level + 1,
            Attrs = attrs
        };
    }

    public string Format(IEnumerable<LogEvent> events)
    {
        _session.Events = events.Select(MapToScalyrEvent);
        var json = JsonConvert.SerializeObject(_session, _jsonSerializerSettings);
        return json;
    }

    string getHostName()
    {
        try
        {
            return Dns.GetHostName();
        }
        catch
        {
            return new[] { "COMPUTERNAME", "HOSTNAME" }.Select(Environment.GetEnvironmentVariable).FirstOrDefault() ??
                   "SERVERHOST";
        }
    }
}