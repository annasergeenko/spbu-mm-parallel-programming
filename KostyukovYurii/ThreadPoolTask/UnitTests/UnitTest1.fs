module ThreadPoolTask.UnitTests

open System
open NUnit.Framework
open System.Threading
open NUnit.Framework.Internal

type MyTask<'TResult>(task) =
    inherit Backend.IMyTask<'TResult>()
    override x.Run () = Thread.Sleep(1000); task ()

[<Test>]
let TestAddOne () =
    let pool = new Backend.ThreadPool(5)
    let t = new MyTask<int>(fun () -> 42)
    pool.Enqueue(t)
    Assert.AreEqual(42, t.Result)
    (pool :> IDisposable).Dispose()

[<Test>]
let TestManyTasksLittleThreads () =
    let n = 5
    let poolSize = 3
    let pool = new Backend.ThreadPool(poolSize)
    let is = List.init n (fun i -> new MyTask<int>(fun () -> i))
    let ss = List.init n (fun i -> new MyTask<string>(fun () -> sprintf "Sample string %i" i))

    for (i, s) in List.zip is ss do
        pool.Enqueue(i)
        pool.Enqueue(s)

    let si = is |> List.map (fun t -> t.ContinueWith(fun x -> sprintf "why not toString: %i" x))

    let isr = is |> List.map (fun t -> t.Result)
    let ssr = ss |> List.map (fun t -> t.Result)
    let sir = si |> List.map (fun t -> t.Result)

    Assert.AreEqual(poolSize, pool.ThreadCount)
    Assert.AreEqual(List.init n id, isr)
    Assert.AreEqual(List.init n (sprintf "Sample string %i"), ssr)
    Assert.AreEqual(List.init n (sprintf "why not toString: %i"), sir)

    (pool :> IDisposable).Dispose()

    for t in is do
        (t :> IDisposable).Dispose()
    for t in ss do
        (t :> IDisposable).Dispose()
    for t in si do
        (t :> IDisposable).Dispose()

[<Test>]
let TestLongCompose () =
    let pool = new Backend.ThreadPool(2)
    let t1 = new MyTask<_>(fun () -> 42)
    pool.Enqueue(t1)
    let t2 = t1.ContinueWith(fun x -> x.ToString())
    let t3 = t2.ContinueWith(fun x -> x.Length)
    let t4 = t3.ContinueWith(fun x -> x.ToString())
    Assert.AreEqual(42, t1.Result)
    Assert.AreEqual("42", t2.Result)
    Assert.AreEqual(2, t3.Result)
    Assert.AreEqual("2", t4.Result)

    (pool :> IDisposable).Dispose()
