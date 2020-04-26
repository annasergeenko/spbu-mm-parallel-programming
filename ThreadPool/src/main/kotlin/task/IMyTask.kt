package task

import java.util.concurrent.Callable

interface IMyTask<T>: Callable<T> {
    var isCompleted: Boolean
    val result: T

    fun <E> continueWith(func: (T) -> E): IMyTask<E>
}