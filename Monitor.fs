namespace ConsumerProducer

open System
open System.Collections.Generic
open System.Threading

module Monitor =

    type LockedBuffer<'T> () =
        let sync = Object()
        let items = List<'T>()

        member this.put (elem : 'T) (cToken : CancellationToken) =
            Monitor.Enter sync
            try
                if not cToken.IsCancellationRequested then
                    items.Add elem
                    Monitor.PulseAll sync
            finally
                Monitor.Exit sync

        member this.take (cToken : CancellationToken) : 'T option =
            let cutElem i =
                let elem = items.[i]
                items.RemoveAt i
                elem
            Monitor.Enter sync
            try
                while items.Count = 0 && not cToken.IsCancellationRequested do
                    Monitor.Wait(sync, 1000) |> ignore
                if not cToken.IsCancellationRequested then cutElem 0 |> Some else None
            finally
                Monitor.Exit sync

        member this.isEmpty = items.Count = 0
