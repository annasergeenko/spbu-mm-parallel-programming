class Producer<in T>(private val pool: MutableList<T>) {
    var isActive: Boolean = true

    public fun produce(value: T) {
        TODO("Not implemented")
    }
}

