﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Serilog.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Threading;

namespace Serilog.Sinks.Scalyr
{
    class ScalyrEvent
    {
        public string Ts { get; set; }
        public int Sev { get; set; }
        public JObject Attrs { get; set; }
    }

    class ScalyrSession
    {
        public string Token { get; set; }
        public string Session { get; set; }
        public IEnumerable<ScalyrEvent> Events { get; set; }
        public JObject SessionInfo { get; set; }
    }

    class ScalyrClient : HttpClient
    {
        readonly ScalyrSession _session;
        readonly JsonSerializerSettings _jsonSerializerSettings;

        long lastTimeStamp;

        public ScalyrClient(string token, string serverHost, string logfile, object sessionInfo, Uri scalyrUri)
        {
            _jsonSerializerSettings = new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new DefaultContractResolver {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };
            _session = new ScalyrSession { Token = token, Session = Guid.NewGuid().ToString("N"), SessionInfo = JObject.FromObject(sessionInfo ?? new object()) };
            _session.SessionInfo.Add("serverHost", serverHost);
            _session.SessionInfo.Add("logfile", logfile);
            BaseAddress = scalyrUri ?? new Uri("https://www.scalyr.com");
        }

        ScalyrEvent MapToScalyrEvent(LogEvent logEvent, int index)
        {
            var attrs = new JObject();
            foreach (var p in logEvent.Properties)
            {
                attrs.Add(p.Key, JToken.Parse(p.Value.ToString()));
            }
            attrs.Add("message", logEvent.RenderMessage());
            var ts = logEvent.Timestamp.ToUnixTimeMilliseconds() * 1000000 + index;
            while (ts <= lastTimeStamp) {ts++;}
            lastTimeStamp = ts;
            return new ScalyrEvent {
                Ts = ts.ToString(),
                Sev = ((int)logEvent.Level) + 1,
                Attrs = attrs
            };
        }

        public Task<HttpResponseMessage> Log(IEnumerable<LogEvent> events)
        {
            _session.Events = events.Select(MapToScalyrEvent);
            var json = JsonConvert.SerializeObject(_session, _jsonSerializerSettings);
            return PostAsync("addEvents", new StringContent(json));
        }
    }
}