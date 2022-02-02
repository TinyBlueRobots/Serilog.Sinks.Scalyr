module JsonFormat

open Expecto
open Serilog
open System
open Newtonsoft.Json.Linq

type Test = { Foo: string }

[<Tests>]
let tests =

  use testApi = new TestApi()

  let logger =
    LoggerConfiguration()
      .MinimumLevel.Verbose()
      .WriteTo
      .Scalyr(
        "token",
        "app",
        Nullable 1,
        TimeSpan.FromMilliseconds 100. |> Nullable,
        scalyrUri = testApi.Scalyr.Uri
      )
      .CreateLogger()

  logger.Information("{@foo}", { Foo = "Bar" })

  testApi.Continue.WaitOne(1000) |> ignore

  let actual =
    testApi.NewtonsoftReceived.[0]
    |> getFirstEvent
    |> getAttrs
    |> getObject "foo"

  let raw = testApi.Raw.[0]

  let expected = JObject.Parse "{\"Foo\":\"Bar\"}"

  test "foo is set" { Expect.isTrue (JToken.DeepEquals(actual, expected)) $"{actual} : {expected} ({raw})" }
