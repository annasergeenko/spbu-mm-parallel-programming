import java.util.*
import kotlin.concurrent.thread
import kotlin.random.Random

const val CONSUMERS_COUNT = 2
const val PRODUCERS_COUNT = 2

val pool = mutableListOf<Int>()

fun main() {
    val producers = (1..PRODUCERS_COUNT).map { Producer(pool) }
    val consumers = (1..CONSUMERS_COUNT).map { Consumer(pool) }

    producers.forEach {
        thread(start = true) {
            while(it.isActive) {
                it.produce(Random.nextInt())
                Thread.sleep(1000)
            }
        }
    }

    consumers.forEach {
        thread(start = true) {
            while(it.isActive) {
                println(it.consume())
                Thread.sleep(1000)
            }
        }
    }

    println("Press ENTER key to stop and exit")
    Scanner(System.`in`).nextLine()

    producers.forEach { it.isActive = false }
    consumers.forEach { it.isActive = false }
}