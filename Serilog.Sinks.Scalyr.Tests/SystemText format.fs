module SystemTextFormat

open System.Text.Json
open Expecto
open Serilog
open System
open Newtonsoft.Json.Linq
open Serilog.Sinks.Scalyr

type Test = { Foo: string }

[<Tests>]
let tests =

    use testApi = new TestApi()

    let createLogger (engine : Engine) = 
        LoggerConfiguration()
            .MinimumLevel.Verbose()
               .WriteTo.Scalyr(
                "token",
                "app",
                Nullable 1,
                TimeSpan.FromMilliseconds 100. |> Nullable,
                scalyrUri = testApi.Scalyr.Uri,
                engine = engine
            )
            .CreateLogger()
            
    let newtonsoftLogger = createLogger Engine.Newtonsoft
            
    let systemTextJsonLogger = createLogger Engine.SystemTextJson
            
    newtonsoftLogger.Error(exn "BOOM", "{@foo}", { Foo = "Bar" })
    testApi.Continue.WaitOne(1000) |> ignore
    
    testApi.Continue.Reset |> ignore
    systemTextJsonLogger.Error(exn "BOOM", "{@foo}", { Foo = "Bar" })
    testApi.Continue.WaitOne(1000) |> ignore


    let n_Received = testApi.NewtonsoftReceived.[0]
    let n_foo = n_Received |> getFirstJEvent |> getJAttrs |> getObject "foo"
    let n_ex = n_Received |> getFirstJEvent |> getJAttrs |> getObject "Exception"
    
    let s_Received = testApi.SystemTextJsonReceived.[0]
    let s_foo = s_Received.RootElement |> getFirstEvent |> getAttrs |> getProperty "foo"
    let s_ex = s_Received.RootElement |> getFirstEvent |> getAttrs |> getProperty "Exception"

    let raw = testApi.Raw.[0]
    
    let n_expected_foo = "{\"Foo\":\"Bar\"}" |> JObject.Parse 
    let n_expected_ex = exn "BOOM" |> JObject.FromObject
    let s_expected_foo = "{\"Foo\":\"Bar\"}" |> JsonDocument.Parse 
    let s_expected_ex = (exn "BOOM") |> JsonSerializer.SerializeToDocument
    

    testList "Information" [
        
        testCase "foo output is identical" <| fun _ -> Expect.equal (s_foo |> serializeElementIndented) (n_foo |> string) $"{n_foo} : {s_foo} ({raw})"
        testCase "exn output is identical" <| fun _ -> Expect.equal (s_ex |> serializeElementIndented) (n_ex |> string) $"{n_ex} : {s_ex} ({raw})"
        
        testCase "foo result is identical" <| fun _ -> Expect.equal (s_expected_foo |> serializeDocumentIndented) (n_expected_foo |> string) $"{n_expected_foo} : {s_expected_foo} ({raw})"
        testCase "exn result is identical" <| fun _ -> Expect.equal (s_expected_ex |> serializeDocumentIndented) (n_expected_ex |> string) $"{n_expected_ex} : {s_expected_ex} ({raw})"
        
        
        testCase "received is identical" <| fun _ -> Expect.equal (s_Received |> serializeDocumentIndented ) (n_Received |> string) $"{n_Received} : {s_Received} ({raw})" 
                                                                 
    ]
