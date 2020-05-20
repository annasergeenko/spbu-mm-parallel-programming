namespace ThreadPool

[<AutoOpen>]
module public Prelude =
    let public internalfail message = "Internal error: " + message |> failwith
    let public internalfailf format = Printf.ksprintf internalfail format
