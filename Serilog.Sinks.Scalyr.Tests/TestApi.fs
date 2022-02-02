[<AutoOpen>]
module TestApi

open System
open System.Text.Json
open Newtonsoft.Json.Linq
open System.Threading
open Hornbill
open Hornbill.FSharp

type TestApi() =
  let scalyr = new FakeService()

  let autoResetEvent = new AutoResetEvent(false)

  do
    Response.withStatusCode 200
    |> scalyr.AddResponse "/" Method.GET

    scalyr.Start() |> ignore

  member _.Continue = autoResetEvent

  member _.Scalyr = scalyr

  member _.NewtonsoftReceived =
    scalyr.Requests
    |> Seq.map (fun r -> JObject.Parse r.Body)
    |> Seq.toArray

  member _.SystemTextJsonReceived =
    scalyr.Requests
    |> Seq.map (fun r -> JsonDocument.Parse r.Body)
    |> Seq.toArray

  member _.Raw =
    scalyr.Requests
    |> Seq.map (fun r -> r.Body)
    |> Seq.toArray

  interface IDisposable with
    member _.Dispose() = scalyr.Dispose()
