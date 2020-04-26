import java.io.Closeable
import java.util.*
import java.util.concurrent.locks.ReentrantLock
import kotlin.concurrent.withLock

class ThreadPool(threadCount: Int): Closeable {
    private val taskQueue: Queue<IMyTask<Any?>> = LinkedList()
    private val workers = (0 until threadCount).map { Worker().apply { this.start() } }
    private val lock = ReentrantLock()
    private val hasTask = lock.newCondition()
    private var isStopped = false

    private inner class Worker: Thread() {
        private var running: Boolean = false

        override fun run() {
            while (true) {
                while (taskQueue.isEmpty()) hasTask.await()
                lock.withLock {
                    val task = taskQueue.poll()
                    running = true
                    task.result = task.invoke() // TODO handle exception
                    running = false
                }
            }
        }

    }

    fun enqueue(task: IMyTask<Any?>) {
        if (isStopped) throw IllegalStateException("ThreadPool was stopped")
        lock.withLock {
            taskQueue.offer(task)
            hasTask.signalAll()
        }
    }

    override fun close() {
        isStopped = true
    }
}