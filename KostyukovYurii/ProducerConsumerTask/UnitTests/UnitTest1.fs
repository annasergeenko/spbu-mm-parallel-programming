module ProducerConsumerTask

open System
open System.IO
open System.Text
open System.Text.RegularExpressions
open System.Threading
open NUnit.Framework

type threadId = int
type message = int
type Log =
    | Send of threadId * message
    | Received of threadId * message
    | Start of threadId
    | End of threadId

type StringWriter(sb:StringBuilder, orig:TextWriter) =
    inherit TextWriter()
    override x.Encoding = stdout.Encoding
    override x.Write (s:string) = sb.Append s |> ignore
    override x.WriteLine (s:string) = sb.AppendLine s |> ignore
    override x.WriteLine() = sb.AppendLine() |> ignore
    member x.Value with get() = sb.ToString().Split(Environment.NewLine) |> List.ofArray
    static member Create () =
        let out = new StringWriter(StringBuilder(), stdout)
        Console.SetOut(out)
        out
    interface IDisposable with member x.Dispose() = Console.SetOut(orig)

let private setUpConsole () =
    let stdin = new MemoryStream()
    let r = new StreamReader(stdin)
    let stdout = StringWriter.Create()
    System.Console.SetIn(r)
    stdin, stdout

let private runWith consumers producers (timeoutms : int) =
    let stdin, stdout = setUpConsole ()
    let testThread = Thread(fun () -> ConsoleApp.runTask true consumers producers)
    testThread.Name <- "Unit Testing thread"
    testThread.Start()
    Thread.Sleep(timeoutms)
    use stdinW = new StreamWriter(stdin)
    stdinW.Write('\n')
    stdinW.Flush()
    stdin.Seek(0L, SeekOrigin.Begin) |> ignore
    testThread.Join()
    let r = stdout.Value
    stdin.Close()
    stdout.Close()
    r

let parse lines =
    let start = Regex(@"Thread (\d+) started")
    let ended = Regex(@"Thread (\d+) ended")
    let message = Regex(@"Thread (\d+) (\w+)\: (\d+)")
    let regMatch (r : Regex) line =
        let m = r.Match(line)
        if m.Success
            then m.Groups |> List.ofSeq |> List.map (fun g -> g.Value) |> List.tail |> Some
            else None
    let parseOne line =
        let log =
            opt {
                return! opt { let! groups = regMatch start line in return groups |> List.head |> int |> Start }
                return! opt { let! groups = regMatch ended line in return groups |> List.head |> int |> End }
                let! groups = regMatch message line
                let tid, typ, message = int <| List.item 0 groups, List.item 1 groups, int <| List.item 2 groups
                if typ = "send"
                    then return Send(tid, message)
                    else return Received(tid, message)
            }
        match log with
        | Some log -> log
        | None -> failwithf "Cannot parse: %s" line
    lines
    |> List.filter ((<>) "")
    |> List.map parseOne

let checkLog log =
    let rec check runningThreads messages = function
        | [] -> Assert.IsTrue(Set.isEmpty runningThreads)
        | Start(tid)::log ->
            Assert.IsFalse(Set.contains tid runningThreads)
            check (Set.add tid runningThreads) messages log
        | End(tid)::log ->
            Assert.IsTrue(Set.contains tid runningThreads)
            check (Set.remove tid runningThreads) messages log
        | Send(tid, message)::log ->
            Assert.IsTrue(Set.contains tid runningThreads)
            Assert.IsFalse(Set.contains message messages)
            check runningThreads (Set.add message messages) log
        | Received(tid, message)::log ->
            Assert.IsTrue(Set.contains tid runningThreads)
            Assert.IsTrue(Set.contains message messages)
            check runningThreads (Set.remove message messages) log
    check Set.empty Set.empty log

[<Test>]
let Test1_1 () =
    let out = runWith 1 1 3000
    let log = parse out
    checkLog log

[<Test>]
let Test10_1 () =
    let out = runWith 10 1 5000
    let log = parse out
    checkLog log

[<Test>]
let Test1_10 () =
    let out = runWith 1 10 5000
    let log = parse out
    checkLog log

[<Test>]
let Test5_10 () =
    let out = runWith 5 10 5000
    let log = parse out
    checkLog log

[<Test>]
let Test10_5 () =
    let out = runWith 10 5 5000
    let log = parse out
    checkLog log

[<Test>]
let Test10_10 () =
    let out = runWith 10 10 5000
    let log = parse out
    checkLog log
