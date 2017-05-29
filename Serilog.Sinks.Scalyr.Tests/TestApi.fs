[<AutoOpen>]
module TestApi

open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open System.Net.Sockets
open System.Net
open System
open System.Threading.Tasks
open System.IO
open Newtonsoft.Json.Linq
open System.Threading

let findPort() =
  let listener = TcpListener(IPAddress.Loopback, 0)
  listener.Start()
  let port = (listener.LocalEndpoint :?> IPEndPoint).Port
  listener.Stop()
  port

type TestApi() =
  let port = findPort()
  let mutable webHost = Unchecked.defaultof<_>

  let received = ResizeArray<_>()

  let autoResetEvent = new AutoResetEvent(false)

  let receive (ctx : HttpContext) =
    (new StreamReader(ctx.Request.Body)).ReadToEnd() |> JObject.Parse |> received.Add
    if received.Count = 6 then autoResetEvent.Set() |> ignore
    Task.Delay 0

  member __.Continue = autoResetEvent

  member __.Received = received

  member __.Start() =
    let url = sprintf "http://127.0.0.1:%i" port
    webHost <- WebHostBuilder().UseUrls(url).Configure(fun app -> app.Run(fun ctx -> receive ctx)).UseKestrel().Build()
    webHost.Start()
    Uri url

  member __.Stop() = if isNull webHost |> not then webHost.Dispose()
  member __.Dispose() = __.Stop()

  interface IDisposable with
    member __.Dispose() = __.Stop()