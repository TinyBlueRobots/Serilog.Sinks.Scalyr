module OutputTemplate

open Expecto
open Serilog
open System

[<Tests>]
let tests =

  use testApi = new TestApi()

  let logger = LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Scalyr("token", "app", Nullable 1, TimeSpan.FromMilliseconds 100. |> Nullable, scalyrUri = testApi.Scalyr.Uri, outputTemplate = "{Level} {Message}").CreateLogger()

  logger.Verbose("HELLO")

  testApi.Continue.WaitOne(1000) |> ignore

  let verboseLog = testApi.Received.[0]

  test "message attr is set" {
    Expect.equal (verboseLog |> getFirstEvent |> getAttrs |> getValue "message") "Verbose HELLO" "message attr"
  }
