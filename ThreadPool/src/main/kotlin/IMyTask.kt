interface IMyTask<T>: Function0<T> {
    val isCompleted: Boolean
    var result: T

    fun <E> continueWith(func: (T) -> E): IMyTask<E>
}