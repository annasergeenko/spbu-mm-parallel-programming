module ConsoleApp
open Backend.Threads
open ProducerConsumerTask

let runTask sandbox producers consumers =
    let queue = LockedQueue<Message>()
    let producers = List.init producers (fun _ -> ProducerThread(queue) :> ThreadWrapper)
    let consumers = List.init consumers (fun _ -> ConsumerThread(queue) :> ThreadWrapper)
    let all = producers @ consumers
    for x in all do x.Run()
    if sandbox
        then
            let mutable res = -1
            while res < 0 do
                res <- System.Console.In.Read()
        else System.Console.ReadKey() |> ignore
    for x in all do x.Abort()
    for x in all do x.Join()

[<EntryPoint>]
let main _ =
    runTask false (Options.Producers()) (Options.Consumers())
    0 // return an integer exit code
