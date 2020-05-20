namespace ConsumerProducer

open Monitor
open System.Threading

module Consumer =

    type Consumer<'T> () =
        member public this.consume (buffer : LockedBuffer<'T>) (token : obj) =
            let cToken = Common.getCancellationToken token
            while not cToken.IsCancellationRequested do
                buffer.take cToken |> ignore
                Thread.Sleep 0
            printfn "Consumer thread %O was successfully terminated \n" Thread.CurrentThread.ManagedThreadId
