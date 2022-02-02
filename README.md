# Serilog.Sinks.Scalyr

[![NuGet Version](http://img.shields.io/nuget/v/Serilog.Sinks.Scalyr.svg?style=flat)](https://www.nuget.org/packages/Serilog.Sinks.Scalyr/)

Writes [Serilog](https://serilog.net) events to [Scalyr](https://www.scalyr.com/). This sink uses the [addEvents API](https://www.scalyr.com/help/api#addEvents) to insert structured log events.

### Getting started

Install the [Serilog.Sinks.Scalyr](https://www.nuget.org/packages/Serilog.Sinks.Scalyr) package from NuGet:

```powershell
Install-Package Serilog.Sinks.Scalyr
```

To configure the sink in C# code, call `WriteTo.Scalyr()` during logger configuration:

```csharp
var log = new LoggerConfiguration()
    .WriteTo.Scalyr("token", "logfile")
    .CreateLogger();
```

**token** "Write Logs" API token. Find API tokens at [https://www.scalyr.com/keys]().

**logfile** The name of the log file being written to. This will probably be the name of your app.

### Optional parameters

**batchSizeLimit** The maximum number of events to include in a single batch.

**period** The time to wait between checking for event batches.

**queueLimit** Maximum number of events in the queue.

**sessionInfo** Additional information about the session. See https://www.scalyr.com/help/api.

**scalyrUri** The base URI for Scalyr. Defaults to https://scalyr.com.

**outputTemplate** A message template describing the output messages. See https://github.com/serilog/serilog/wiki/Formatting-Output.

**restrictedToMinimumLevel** The minimum log event level required in order to write an event to the sink.