namespace ConsumerProducer

open Monitor
open System.Threading

module Producer =

    type Producer<'T> (product : 'T) =
        member this.product = product
        member this.produce (buffer : LockedBuffer<'T>) (token : obj) =
            let cToken = Common.getCancellationToken token
            while not cToken.IsCancellationRequested do
                buffer.put product cToken
                Thread.Sleep 0
            printfn "Producer thread %O was successfully terminated \n" Thread.CurrentThread.ManagedThreadId
