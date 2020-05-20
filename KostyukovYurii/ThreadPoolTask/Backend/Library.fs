module ThreadPoolTask.Backend

open System
open System.Threading
open System.Collections

module Option =
    let rec unfold f =
        let result = f ()
        match result with
        | Some x -> x
        | None -> unfold f

module List =
    let count p xs =
        let rec count n = function
            | [] -> n
            | x::xs when p x -> count (n + 1) xs
            | _::xs -> count n xs
        count 0 xs

type internal TaskResult<'r> =
    | Ok of 'r
    | Error of Exception
    | NotYet

type private IMyTask =
    abstract member Execute : unit -> unit

type ThreadPool(numberOfThreads : int) as this =
    let cts = new CancellationTokenSource()
    let taskPool = Generic.Queue<IMyTask>()
    let allThreads = List.init numberOfThreads (fun _ -> Thread(this.ThreadRunner))
    do allThreads |> List.iter (fun t -> t.Name <- sprintf "Swimmer %i" t.ManagedThreadId;t.Start())
    static member Empty = new ThreadPool(0)
    member private x.ThreadRunner () =
        let ct = cts.Token
        ct.Register(fun () -> Monitor.Enter(cts); Monitor.PulseAll(cts); Monitor.Exit(cts)) |> ignore
        while not ct.IsCancellationRequested do
            let task =
                Monitor.Enter(cts)
                try
                    while not ct.IsCancellationRequested && taskPool.Count = 0 do
                        Monitor.Wait(cts) |> ignore
                    if taskPool.Count = 0 then None else Some <| taskPool.Dequeue()
                finally Monitor.Exit(cts)
            match task with
            | Some task -> task.Execute()
            | None -> ()
    member x.Enqueue(task : IMyTask<'TResult>) : unit =
        task.SetThreadPool(x)
        Monitor.Enter(cts)
        try
            taskPool.Enqueue(task)
            Monitor.PulseAll(cts)
        finally
            Monitor.Exit(cts)
    member x.ThreadCount
        with get () = allThreads |> List.count (fun (t : Thread) -> (ThreadState.Running ||| ThreadState.WaitSleepJoin).HasFlag(t.ThreadState))
    interface IDisposable with
        member x.Dispose() =
            cts.Cancel()
            for thread in allThreads do
                thread.Join()
            cts.Dispose()

and [<AbstractClass>] IMyTask<'TResult>() =
    let mutable _result : 'TResult TaskResult = NotYet
    let mutable _threadPool : ThreadPool = ThreadPool.Empty
    let hasResult = new ManualResetEvent(false)
    member internal x.SetThreadPool(pool : ThreadPool) = _threadPool <- pool
    member x.Result
        with get () =
            Option.unfold (fun () ->
            match _result with
            | Ok r -> Some r
            | Error e -> raise <| AggregateException(Array.singleton e)
            | NotYet -> hasResult.WaitOne() |> ignore; None)
    interface IMyTask with
        override x.Execute() =
            try
                let result = x.Run()
                _result <- Ok result
            with e -> _result <- Error e
            hasResult.Set() |> ignore
    member x.IsCompleted
        with get () =
            match _result with
            | NotYet -> false
            | _ -> true
    member x.ContinueWith(f : 'TResult -> 'TNewResult) : IMyTask<'TNewResult> =
        let task = new MyTask<'TNewResult>(fun () -> f x.Result) :> IMyTask<'TNewResult>
        _threadPool.Enqueue(task)
        task
    abstract member Run : unit -> 'TResult
    interface IDisposable with
        member x.Dispose() = hasResult.Dispose()

and private MyTask<'TResult>(task) =
    inherit IMyTask<'TResult>()
    override x.Run () = task ()
