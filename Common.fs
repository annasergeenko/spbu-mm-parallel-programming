namespace ConsumerProducer

open System.Threading

module Common =

    let getCancellationToken (token : obj) =
        match token with
        | :? CancellationToken as cToken -> cToken
        | obj -> failwithf "Expected cancellation token, but got %O" obj
