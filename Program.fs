namespace ConsumerProducer

open System
open System.Threading
open Monitor
open Consumer
open Producer

    module Main =

        let initializeThreads buffer consumersCount producersCount =
            let consumers = List.init consumersCount (fun _ -> Consumer())
            let producers = List.init producersCount (fun _ -> Producer(42))
            let consumerThreads = consumers |> List.map (fun consumer -> Thread(ParameterizedThreadStart(consumer.consume buffer)))
            let producerThreads = producers |> List.map (fun producer -> Thread(ParameterizedThreadStart(producer.produce buffer)))
            consumerThreads, producerThreads

        [<EntryPoint>]
        let main _ =
            let consumersCount = 6
            let producersCount = 8
            let buffer = LockedBuffer()
            let source = new CancellationTokenSource()
            let cThreads, pThreads = initializeThreads buffer consumersCount producersCount
            cThreads @ pThreads |> List.iter (fun thread -> thread.Start(source.Token))
            Console.ReadKey() |> ignore
            Console.WriteLine ""
            source.Cancel()
            source.Dispose()
            0 // return an integer exit code
