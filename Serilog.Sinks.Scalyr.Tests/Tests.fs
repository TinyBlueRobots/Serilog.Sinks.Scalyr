module Tests

open Expecto
open Serilog
open System
open System.Text.RegularExpressions
open System.Net

type SessionInfo = { location : string }

[<Tests>]
let tests =

  let isGuid value = Regex.IsMatch(value, "[a-f0-9]{32}")

  let startTs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000000L

  use testApi = new TestApi()
  
  let logger =
    LoggerConfiguration()
        .MinimumLevel.Verbose()
        .WriteTo.Scalyr(
            "token",
            "app",
            Nullable 1,
            TimeSpan.FromMilliseconds 100. |> Nullable,
            scalyrUri = testApi.Scalyr.Uri,
            sessionInfo = { location = "Earth" }
        )
        .CreateLogger()

  logger.Verbose("Verbose {foo}", "bar")
  logger.Debug "Debug"
  logger.Information "Information"
  logger.Warning "Warning"
  logger.Error "Error"
  logger.Fatal "Fatal"

  testApi.Continue.WaitOne(1000) |> ignore

  let verboseLog = testApi.NewtonsoftReceived.[0]
  
  let sessionInfo = verboseLog.["sessionInfo"].ToObject()
  
  testList "Information" [

    testCase "token is set" <| fun _ -> Expect.equal (verboseLog |> getValue "token") "token" ""

    testCase "session is a guid" <| fun _ -> Expect.isTrue (verboseLog |> getValue "session" |> isGuid) ""

    testCase "serverHost is set" <| fun _ -> Expect.equal (sessionInfo |> getValue "serverHost") (Dns.GetHostName()) ""

    testCase "logfile is set" <| fun _ -> Expect.equal (sessionInfo |> getValue "logfile") "app" ""

    testCase "location is set from sessioninfo object" <| fun _ -> Expect.equal (sessionInfo |> getValue "location") "Earth" ""

    testCase "api received 6 logs" <| fun _ -> Expect.equal testApi.NewtonsoftReceived.Length 6 ""

    testCase "events have incrementing ts" <| fun _ -> Expect.all (startTs :: (testApi.NewtonsoftReceived |> Seq.map (getFirstEvent >> getValue "ts" >> int64)|> Seq.toList)  |> Seq.pairwise) (fun pair -> fst pair < snd pair) ""

    testCase "events have incrementing sev" <| fun _ -> Expect.equal (testApi.NewtonsoftReceived |> Seq.map (getFirstEvent >> getValue "sev" >> int) |> Seq.toList) [ 1; 2; 3; 4; 5; 6 ] ""

    testCase "message attr is set" <| fun _ -> Expect.equal (verboseLog |> getFirstEvent |> getAttrs |> getValue "message") "Verbose \"bar\"" ""

    testCase "foo attr is set" <| fun _ -> Expect.equal (verboseLog |> getFirstEvent |> getAttrs |> getValue "foo") "bar" ""
    
  ]
