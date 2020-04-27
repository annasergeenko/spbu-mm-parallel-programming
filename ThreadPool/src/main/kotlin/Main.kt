import task.Task

fun main() {
    val pool = ThreadPool(3)
    pool.enqueue(Task { println("Task1") })
    pool.enqueue(Task { println("Task2") })
    pool.enqueue(Task { println("Task3") })

    Thread.sleep(100)
    pool.close()
}