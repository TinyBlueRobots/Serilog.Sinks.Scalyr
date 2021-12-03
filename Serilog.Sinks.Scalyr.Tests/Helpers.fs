[<AutoOpen>]
module Helpers

open Newtonsoft.Json.Linq

let getObject name (jObject: JObject) = jObject.[name]

let getValue name (jObject: JObject) = jObject.[name].Value<string>()

let getFirstEvent (jObject: JObject) =
    jObject.["events"].Value<JArray>().[0].ToObject()

let getAttrs (jObject: JObject) = jObject.["attrs"].ToObject()
