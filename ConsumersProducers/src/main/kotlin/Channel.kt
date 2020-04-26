import java.util.concurrent.locks.ReentrantLock
import kotlin.concurrent.withLock

class Channel<T>: Producer<T>, Consumer<T> {
    private val pool = mutableListOf<T>()
    private val lock = ReentrantLock()
    private val emptyCondition = lock.newCondition()
    var isActive = true

    override fun produce(value: T) {
        if (!isActive) throw ChannelClosedException()
        lock.withLock {
            pool.add(value)
            emptyCondition.signalAll()
        }
    }

    override fun consume(): T {
        return lock.withLock {
            while (pool.isEmpty() && isActive) emptyCondition.await()
            if (!isActive) throw ChannelClosedException()
            pool.first().apply { pool.removeAt(0) }
        }
    }

    override fun close() {
        isActive = false
        lock.withLock { emptyCondition.signalAll() }
    }
}