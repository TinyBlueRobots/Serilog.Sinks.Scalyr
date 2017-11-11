export FrameworkPathOverride=$(dirname $(which mono))/../lib/mono/4.5/
NUGETVERSION=2.0.0
dotnet pack Serilog.Sinks.Scalyr -c Release /p:PackageVersion=$NUGETVERSION
dotnet nuget push Serilog.Sinks.Scalyr/bin/Release/Serilog.Sinks.Scalyr.$NUGETVERSION.nupkg -k $NUGETKEY -s nuget.org