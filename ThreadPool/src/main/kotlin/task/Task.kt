package task

import java.util.concurrent.locks.ReentrantLock
import kotlin.concurrent.withLock

class Task<T>(private val task: () -> T): IMyTask<T> {
    private val lock = ReentrantLock()
    private val resultComputed = lock.newCondition()

    var previousTask: IMyTask<Any?>? = null
    private var _exception: Exception? = null
    private var _result: T? = null

    override var isCompleted: Boolean = false
    override val result: T by lazy {
        if (!isCompleted) lock.withLock { resultComputed.await() }
        return@lazy _exception?.let { throw AggregateException(it) } ?: _result!!
    }

    override fun call(): T {
        try {
            _result = task.invoke()
            lock.withLock { resultComputed.signalAll() }
            return _result!!
        } catch (e: Exception) {
            _exception = e
            throw AggregateException(e)
        } finally {
            isCompleted = true
        }
    }

    @Suppress("UNCHECKED_CAST")
    override fun <E> continueWith(func: (T) -> E): IMyTask<E> {
        return Task { func(result) }.apply { previousTask = this@Task as IMyTask<Any?> }
    }
}