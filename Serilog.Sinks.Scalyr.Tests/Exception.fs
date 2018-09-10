module Exception

open Expecto
open Serilog
open System
open Newtonsoft.Json.Linq

[<Tests>]
let tests =

  use testApi = new TestApi()

  let logger = LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Scalyr("token", "app", Nullable 1, TimeSpan.FromMilliseconds 100. |> Nullable, scalyrUri = testApi.Scalyr.Uri).CreateLogger()

  logger.Error(exn "BOOM", "Error")

  testApi.Continue.WaitOne(1000) |> ignore

  let actual = testApi.Received.[0] |> getFirstEvent |> getAttrs |> getObject "Exception"

  let expected = exn "BOOM" |> JObject.FromObject

  test "Exception is set" {
    Expect.isTrue (JToken.DeepEquals(actual, expected)) (sprintf "%O : %O" actual expected)
  }