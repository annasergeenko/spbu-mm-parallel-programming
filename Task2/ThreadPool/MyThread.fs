namespace ThreadPool

open System.Collections.Concurrent
open System.Threading
open IMyTask

module MyThread =

    type MyThread(globalTaskQueue : ConcurrentQueue<IMyTask>, localTaskQueues : ConcurrentDictionary<int, ConcurrentQueue<IMyTask>>, emptyQueueHandle : AutoResetEvent) =
        let id = Thread.CurrentThread.ManagedThreadId
        let emptyQueueHandle = emptyQueueHandle
        let globalTaskQueue = globalTaskQueue
        let localTaskQueues = localTaskQueues
        let localTaskQueue = ConcurrentQueue<IMyTask>()
        do localTaskQueues.[id] <- localTaskQueue

        let dequeue (queue : ConcurrentQueue<IMyTask>) =
            let hasValue, value = queue.TryDequeue()
            if hasValue then Some value else None

        member this.Run (token : obj) =
            let cToken = Common.getCancellationToken token
            while not cToken.IsCancellationRequested do
                match dequeue localTaskQueue with
                | Some x ->
                    x.Run()
                | None ->
                    match dequeue globalTaskQueue with
                    | Some x ->
                        x.Run()
                    | None ->
                         match localTaskQueues |> Seq.tryPick (fun kvp -> dequeue kvp.Value) with
                         | Some x ->
                            x.Run()
                         | None -> emptyQueueHandle.WaitOne() |> ignore
            printfn "Pool thread %O was successfully terminated \n" id
