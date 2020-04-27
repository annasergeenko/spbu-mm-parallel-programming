package task

import java.util.concurrent.locks.ReentrantLock
import kotlin.concurrent.withLock

class Task<T>(private val task: () -> T): IMyTask<T> {
    private val lock = ReentrantLock()
    private val resultComputed = lock.newCondition()

    override var isCompleted: Boolean = false
    private var _result: T? = null
    override val result: T by lazy {
        if (!isCompleted) lock.withLock { resultComputed.await() }
        return@lazy _result!!
    }

    var previousTask: IMyTask<Any?>? = null

    override fun call(): T {
        _result = task.invoke()
        isCompleted = true
        lock.withLock { resultComputed.signalAll() }
        return _result!!
    }

    @Suppress("UNCHECKED_CAST")
    override fun <E> continueWith(func: (T) -> E): IMyTask<E> {
        return Task { func(result) }.apply { previousTask = this@Task as IMyTask<Any?> }
    }
}