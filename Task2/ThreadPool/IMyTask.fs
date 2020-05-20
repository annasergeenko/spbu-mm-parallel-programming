namespace ThreadPool

open System

module IMyTask =

    type IMyTask =
        inherit IDisposable
        abstract member IsCompleted : bool
        abstract member Run : unit -> unit

    type IMyTask<'a> =
        inherit IMyTask
        abstract member Result : 'a
        abstract member ContinueWith<'b> : Func<'a, 'b> -> IMyTask<'b>
