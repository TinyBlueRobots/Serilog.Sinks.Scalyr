[<AutoOpen>]
module Helpers

open System.IO
open System.Text
open System.Text.Encodings.Web
open System.Text.Json
open System.Text.Unicode
open Newtonsoft.Json.Linq

let getObject (name: string) (jObject: JObject) : JToken = jObject.[name]

let getJValue (name: string) (jObject: JObject) : string = jObject.[name].Value<string>()

let getFirstJEvent (jObject: JObject) : JObject =
    jObject.["events"].Value<JArray>().[0].ToObject()

let getJAttrs (jObject: JObject) : JObject = jObject.["attrs"].ToObject()


let getProperty (name: string) (element: JsonElement) : JsonElement = element.GetProperty(name)

let getValue (name: string) (element: JsonElement) : string = element.GetProperty(name).GetString()

let getFirstEvent (element: JsonElement) : JsonElement =
    element.GetProperty("events").EnumerateArray()
    |> Seq.head

let getAttrs (element: JsonElement) : JsonElement = element.GetProperty("attrs")

let indentedWriterOptions =
    JsonWriterOptions(Indented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping)
    
let serializeDocumentIndented (doc: JsonDocument) : string =
    use stream = new MemoryStream()
    using (new Utf8JsonWriter(stream, indentedWriterOptions)) doc.WriteTo
    Encoding.UTF8.GetString(stream.ToArray())

let serializeElementIndented (element: JsonElement) : string =
    use stream = new MemoryStream()
    using (new Utf8JsonWriter(stream, indentedWriterOptions)) element.WriteTo
    Encoding.UTF8.GetString(stream.ToArray())
