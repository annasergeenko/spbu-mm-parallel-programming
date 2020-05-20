namespace ThreadPool

open IMyTask

module TaskScheduler =

    type ITaskScheduler =
        abstract member ScheduleAndRun<'a> : IMyTask<'a> -> unit
