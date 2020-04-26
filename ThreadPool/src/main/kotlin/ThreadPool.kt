import task.IMyTask
import java.io.Closeable
import java.util.*
import java.util.concurrent.locks.ReentrantLock
import kotlin.concurrent.withLock

class ThreadPool(threadCount: Int): Closeable {
    private val taskQueue: Queue<IMyTask<Any?>> = LinkedList()
    private val lock = ReentrantLock()
    private val hasTask = lock.newCondition()
    private val workers = (0 until threadCount).map { Worker().apply { this.start() } }
    private var isStopped = false

    private inner class Worker: Thread() {
        private var running: Boolean = false

        override fun run() {
            while (true) {
                lock.withLock {
                    while (taskQueue.isEmpty() && !isStopped) hasTask.await()
                    if (isStopped) return
                    withRun { taskQueue.poll().call() } // TODO handle exception
                }
            }
        }

        private fun withRun(block: () -> Any?): Any? {
            return try {
                running = true
                block()
            } finally {
                running = false
            }
        }
    }

    @Suppress("UNCHECKED_CAST")
    fun <T> enqueue(task: IMyTask<T>) {
        if (isStopped) throw IllegalStateException("ThreadPool was stopped")
        lock.withLock {
            taskQueue.offer(task as IMyTask<Any?>)
            hasTask.signalAll()
        }
    }

    override fun close() {
        isStopped = true
        lock.withLock { hasTask.signalAll() }
    }
}