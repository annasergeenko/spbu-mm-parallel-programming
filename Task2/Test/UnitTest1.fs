namespace ThreadPool.Test

module Test =

    open System
    open System.Diagnostics
    open NUnit.Framework
    open ThreadPool
    open ThreadPool.ThreadPool
    open ThreadPool.IMyTask
    open Test.MyTask

    [<Test>]
    let AddOneTaskTest () =
        let poolThreadsCount = 2
        use pool = new Pool(poolThreadsCount)
        let result = 3
        let func = Func<int>(fun () -> result)
        use task = new MyTask<int>(pool, func)
        pool.Enqueue task
        Assert.True((task :> IMyTask<int>).Result = result)

    [<Test>]
    let AddManyTasksTest () =
        let poolThreadsCount = 2
        use pool = new Pool(poolThreadsCount)
        let result = 3
        let func = Func<int>(fun () -> result)
        use task1 = new MyTask<int>(pool, func)
        use task2 = new MyTask<int>(pool, func)
        use task3 = new MyTask<int>(pool, func)
        pool.Enqueue task1
        pool.Enqueue task2
        pool.Enqueue task3
        let checkResult task = (task :> IMyTask<int>).Result = result
        Assert.True(checkResult task1 && checkResult task2 && checkResult task3)

    [<Test>]
    let ContinueWithTest () =
        let poolThreadsCount = 2
        let waitTask (iTask : IMyTask) = while iTask.IsCompleted |> not do ()
        use pool = new Pool(poolThreadsCount)
        let func = Func<int>(fun () -> 3)
        use task1 = new MyTask<int>(pool, func)
        pool.Enqueue task1
        let continuation1 = Func<int, char>(fun i -> "result".[i])
        let continuation2 = Func<char, string>(fun c -> Printf.sprintf "resulting char: %c" c)
        use iTask1 = task1 :> IMyTask<int>
        waitTask iTask1
        use iTask2 = iTask1.ContinueWith continuation1
        waitTask iTask2
        use iTask3 = iTask2.ContinueWith continuation2
        Assert.True(iTask1.Result = 3 && iTask2.Result = 'u' && iTask3.Result = "resulting char: u")

    [<Test>]
    let WePutTaskIntoYourTaskTest () =
        let poolThreadsCount = 2
        use pool = new Pool(poolThreadsCount)
        let innerFunc = Func<string>(fun () -> "hello")
        use innerTask = new MyTask<string>(pool, innerFunc)
        let func () =
            pool.Enqueue innerTask
            (innerTask :> IMyTask<string>).Result = "hello"
        let func = Func<bool>(func)
        use task = new MyTask<bool>(pool, func)
        pool.Enqueue(task)
        Assert.True((task :> IMyTask<bool>).Result = true)

    [<Test>]
    let ThreadCountTest () =
        let threadCountBeforePoolStart = Process.GetCurrentProcess().Threads.Count
        let poolThreadsCount = 2
        use pool = new Pool(poolThreadsCount)
        let threadCountAfterPoolStart = Process.GetCurrentProcess().Threads.Count
        Assert.True(threadCountAfterPoolStart - threadCountBeforePoolStart = poolThreadsCount)
