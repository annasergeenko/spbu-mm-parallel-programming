class Consumer<out T>(private val pool: MutableList<T>) {
    var isActive: Boolean = true

    public fun consume(): T {
        TODO("Not implemented")
    }
}