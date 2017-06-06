# Serilog.Sinks.Scalyr

[![NuGet Version](http://img.shields.io/nuget/v/Serilog.Sinks.Scalyr.svg?style=flat)](https://www.nuget.org/packages/Serilog.Sinks.Scalyr/)


[Scalyr](https://www.scalyr.com/) is a cloud based log management service. This sink uses the [addEvents API](https://www.scalyr.com/help/api#addEvents) to insert structured log events.

```csharp
var log = new LoggerConfiguration()
    .WriteTo.Scalyr("token", "serverHost", "logfile")
    .CreateLogger();
```

**token** "Write Logs" API token. Find API tokens at [https://www.scalyr.com/keys]().

**serverHost** Hostname or some other stable server identifier. Scalyr uses this value to organize events from different servers.

**logfile** The name of the log file being written to. This will probably be the name of your app.