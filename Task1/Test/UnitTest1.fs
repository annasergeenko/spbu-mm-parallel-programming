module Test

open System.Threading
open NUnit.Framework

module BufferTest =

    open ConsumerProducer.Monitor

    [<TestFixture>]
    type BufferTest () =

        [<Test>]
        member this.putTest() =
            let buffer = LockedBuffer()
            let source = new CancellationTokenSource()
            buffer.put 42 source.Token
            Assert.True(not buffer.isEmpty)
            source.Dispose()

        [<Test>]
        member this.cancelPutTest() =
            let buffer = LockedBuffer()
            let source = new CancellationTokenSource()
            source.Cancel()
            buffer.put 42 source.Token
            Assert.True(buffer.isEmpty)
            source.Dispose()

        [<Test>]
        member this.cancelTakeTest() =
            let buffer = LockedBuffer()
            let source = new CancellationTokenSource()
            source.Cancel()
            let elem = buffer.take source.Token
            Assert.True(Option.isNone elem)
            source.Dispose()

        [<Test>]
        member this.commonTest() =
            let buffer = LockedBuffer()
            let source = new CancellationTokenSource()
            let cToken = source.Token
            buffer.put 42 cToken
            let elem = buffer.take cToken
            Assert.True(Option.contains 42 elem && buffer.isEmpty)
            source.Dispose()

module ConsumerProducerTest =

    open ConsumerProducer.Monitor
    open ConsumerProducer.Consumer
    open ConsumerProducer.Producer
    open ConsumerProducer.Main

    [<TestFixture>]
    type ConsumerProducerTest () =

        let checkThreadState = (=) ThreadState.Stopped

        [<Test>]
        member this.consumeTest() =
            let buffer = LockedBuffer()
            let source = new CancellationTokenSource()
            let cToken = source.Token
            let consumer = Consumer()
            let thread = Thread(ParameterizedThreadStart(consumer.consume buffer))
            thread.Start(cToken)
            source.Cancel()
            Thread.Sleep 1000
            Assert.True(checkThreadState thread.ThreadState)
            source.Dispose()

        [<Test>]
        member this.produceTest() =
            let buffer = LockedBuffer()
            let source = new CancellationTokenSource()
            let producer = Producer(42)
            let thread = Thread(ParameterizedThreadStart(producer.produce buffer))
            thread.Start(source.Token)
            Thread.Sleep 1000
            source.Cancel()
            Thread.Sleep 1000
            Assert.True(checkThreadState thread.ThreadState && not buffer.isEmpty)
            source.Dispose()

        [<Test>]
        member this.commonTest() =
            let buffer = LockedBuffer()
            let producerSource = new CancellationTokenSource()
            let consumerSource = new CancellationTokenSource()
            let producerToken = producerSource.Token
            let consumerToken = consumerSource.Token
            let cThreads, pThreads = initializeThreads buffer 5 5
            pThreads |> List.iter (fun thread -> thread.Start(producerToken))
            cThreads |> List.iter (fun thread -> thread.Start(consumerToken))
            producerSource.Cancel()
            Thread.Sleep 1000
            consumerSource.Cancel()
            Thread.Sleep 1000
            let allStoped = pThreads @ cThreads |> List.forall (fun thread -> checkThreadState thread.ThreadState)
            Assert.True(allStoped && buffer.isEmpty)
            producerSource.Dispose()
            consumerSource.Dispose()
