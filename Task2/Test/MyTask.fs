namespace ThreadPool.Test

open System
open System.Threading
open ThreadPool
open TaskScheduler
open IMyTask

module MyTask =

    type 'a FunctionResult =
        | NotReady
        | Value of 'a
        | Exception of Exception

    type MyTask<'a> (scheduler : ITaskScheduler, f : Func<'a>) =
        let sync = Object()
        let resultNotReadyHandle = new ManualResetEvent(false)
        let scheduler : ITaskScheduler = scheduler
        let mutable isCompleted = false
        let mutable result : 'a FunctionResult = NotReady
        member private this.Result
            with get() =
                Monitor.Enter sync
                let res = result
                Monitor.Exit sync
                res
            and set value =
                Monitor.Enter sync
                result <- value
                Monitor.Exit sync

        interface IMyTask<'a> with
            member this.IsCompleted = isCompleted // Reads and writes of the bool data types are atomic
            member this.Result =
                if not isCompleted then resultNotReadyHandle.WaitOne() |> ignore
                match this.Result with
                | NotReady -> internalfail "Trying to get result, which is not ready!"
                | Value x -> x
                | Exception e -> AggregateException e |> raise

            member this.Run() : unit =
                try
                    try
                        let v = f.Invoke()
                        this.Result <- Value v
                    finally
                        isCompleted <- true
                        Common.pulse resultNotReadyHandle
                with
                | :? SystemException as e ->
                    this.Result <- Exception e

            member this.ContinueWith<'b> (f : Func<'a, 'b>) =
                match this.Result with
                | NotReady -> internalfail "Trying to call ContinueWith when result is not ready!"
                | Value x ->
                    let task = new MyTask<'b>(scheduler, Func<'b>(fun () -> f.Invoke(x)))
                    let iTask = task :> IMyTask<'b>
                    scheduler.ScheduleAndRun iTask
                    iTask
                | Exception e -> internalfailf "Trying to call ContinueWith when result is %O!" e

        interface IDisposable with
            member this.Dispose() =
                resultNotReadyHandle.Close()
