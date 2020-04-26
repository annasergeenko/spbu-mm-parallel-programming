interface Producer<in T> {
    fun produce(value: T)
    fun close()
}

