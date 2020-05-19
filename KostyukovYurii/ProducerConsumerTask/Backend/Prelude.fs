[<AutoOpen>]
module Prelude
open System

let private printfnMutex = obj()
let printfn f = lock printfnMutex (fun () -> Printf.kprintf (fun o -> Console.Out.WriteLine(o); Console.Out.Flush()) f)

type OptionBuilder() =
    member this.Bind(x, f) =
        match x with
        | Some x -> f x
        | None -> None
    member this.Return(x) = Some x
    member this.ReturnFrom(x) = x
    member this.Combine(a, b) =
        match a with
        | Some _ -> a
        | None -> b
    member this.Delay(f) = f()
let opt = OptionBuilder()
