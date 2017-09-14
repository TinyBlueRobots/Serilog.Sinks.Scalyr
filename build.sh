export FrameworkPathOverride=$(dirname $(which mono))/../lib/mono/4.5/
dotnet restore && \
dotnet publish Serilog.Sinks.Scalyr.Tests && \
dotnet Serilog.Sinks.Scalyr.Tests/bin/Debug/netcoreapp20/publish/Serilog.Sinks.Scalyr.Tests.dll