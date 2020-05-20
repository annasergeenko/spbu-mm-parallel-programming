namespace ThreadPool

open System
open System.Collections.Concurrent
open System.Threading
open TaskScheduler
open IMyTask
open MyThread

module ThreadPool =

    type Pool (threadNum : int) =
        let threadNum = threadNum
        let emptyQueueHandle = new AutoResetEvent(false)
        let globalTaskQueue = ConcurrentQueue<IMyTask>()
        let localTaskQueues = ConcurrentDictionary<int, ConcurrentQueue<IMyTask>>()
        let myThreads = List.init threadNum (fun _ -> MyThread(globalTaskQueue, localTaskQueues, emptyQueueHandle))
        let source = new CancellationTokenSource()
        let threads = myThreads |> List.map (fun thread -> Thread(ParameterizedThreadStart(thread.Run)))
        do threads |> List.iter (fun thread -> thread.Start(source.Token))

        member this.Enqueue<'a> (task : IMyTask<'a>) =
            let id = Thread.CurrentThread.ManagedThreadId
            if localTaskQueues.ContainsKey id
                then localTaskQueues.[id].Enqueue task
                else globalTaskQueue.Enqueue task
            Common.pulse emptyQueueHandle

        interface IDisposable with
            member this.Dispose() =
                source.Cancel()
                while threads |> List.forall (fun thread -> thread.ThreadState = ThreadState.Stopped) |> not do
                    Thread.Sleep(1000)
                    Common.pulse emptyQueueHandle
                source.Dispose()
                emptyQueueHandle.Close()

        interface ITaskScheduler with
            member this.ScheduleAndRun<'a> (task : IMyTask<'a>) =
                this.Enqueue task
