import task.IMyTask
import task.Task
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
                val task = lock.withLock {
                    while (taskQueue.isEmpty() && !isStopped) hasTask.await()
                    if (isStopped) {
                        this.interrupt()
                        return
                    }
                    taskQueue.poll()
                }
                withRun { task.call() }
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
            val tasks = generateSequence(task as IMyTask<Any?>) { (it as? Task)?.previousTask }.filterNotNull()
            taskQueue.addAll(tasks.toList().reversed())
            hasTask.signalAll()
        }
    }

    override fun close() {
        isStopped = true
        lock.withLock { hasTask.signalAll() }
    }
}