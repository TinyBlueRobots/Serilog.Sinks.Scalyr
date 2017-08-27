SET NUGETVERSION=1.0.6
dotnet pack Serilog.Sinks.Scalyr -c Release /p:PackageVersion=%NUGETVERSION%
dotnet nuget push Serilog.Sinks.Scalyr\bin\Release\Serilog.Sinks.Scalyr.%NUGETVERSION%.nupkg -k %NUGETKEY% -s nuget.org