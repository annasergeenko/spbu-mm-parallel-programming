module Backend.Threads
open System
open System.Collections.Generic
open System.Threading
open ProducerConsumerTask

type LockedQueue<'a>() =
    let sync = obj()
    let buffer = List<'a>()

    member x.Add(query) =
        Monitor.Enter(sync)
        try
            buffer.Add(query)
            Monitor.PulseAll(sync)
        finally
            Monitor.Exit(sync)

    member x.Remove() =
        Monitor.Enter(sync)
        try
            while buffer.Count = 0 do
                Monitor.Wait(sync) |> ignore
            let query = buffer.[0]
            buffer.RemoveAt(0)
            Monitor.PulseAll(sync)
            query
        finally
            Monitor.Exit(sync)

type Message(message : string) =
    let message = message
    member x.SendMessage() = printfn "Thread %i send: %s" Thread.CurrentThread.ManagedThreadId message; x
    member x.ReceiveMessage() = printfn "Thread %i received: %s" Thread.CurrentThread.ManagedThreadId message

[<AbstractClass>]
type ThreadWrapper() as this =
    let thread = Thread(this.Loop)
    abstract member Step : unit -> unit
    member private x.Loop() =
        try
            printfn "Thread %i started" Thread.CurrentThread.ManagedThreadId
            while true do
                x.Step()
                Thread.Sleep(Options.SleepBetweenWorkMS())
        with
            | :? ThreadInterruptedException -> printfn "Thread %i ended" Thread.CurrentThread.ManagedThreadId
    member x.Run() = thread.Start()
    member x.Abort() = thread.Interrupt()
    member x.Join() = thread.Join()
    member x.Thread() = thread

type ProducerThread (queue : LockedQueue<Message>) as this =
    inherit ThreadWrapper()
    let queue = queue
    let mutable state = this.Thread().ManagedThreadId * 1000
    override x.Step() =
        state <- state + 1
        queue.Add(Message(state.ToString()).SendMessage())

type ConsumerThread (queue : LockedQueue<Message>) =
    inherit ThreadWrapper()
    let queue = queue
    override x.Step() =
        let msg = queue.Remove()
        msg.ReceiveMessage()
