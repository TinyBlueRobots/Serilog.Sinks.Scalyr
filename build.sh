set -e
dotnet restore
pushd Serilog.Sinks.Scalyr.Tests
dotnet run
popd