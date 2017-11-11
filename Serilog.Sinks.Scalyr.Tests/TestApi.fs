[<AutoOpen>]
module TestApi

open System
open Newtonsoft.Json.Linq
open System.Threading
open Hornbill
open Hornbill.FSharp

type TestApi() =
  let scalyr = new FakeService()

  let autoResetEvent = new AutoResetEvent(false)

  do
    Response.withStatusCode 200 |> scalyr.AddResponse "/" Method.GET
    scalyr.Start() |> ignore

  member __.Continue = autoResetEvent

  member __.Scalyr = scalyr

  member __.Received with get() = scalyr.Requests |> Seq.map (fun r -> JObject.Parse r.Body) |> Seq.toArray

  interface IDisposable with
    member __.Dispose() = scalyr.Dispose()