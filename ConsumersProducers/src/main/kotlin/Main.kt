import java.util.*
import kotlin.concurrent.thread
import kotlin.random.Random

const val CONSUMERS_COUNT = 2
const val PRODUCERS_COUNT = 1

fun main() {
    val channel = Channel<Int>()
    repeat(PRODUCERS_COUNT) {
        thread {
            while (channel.isActive) {
                channel.produce(Random.nextInt())
                Thread.sleep(1000)
            }
        }
    }
    repeat(CONSUMERS_COUNT) {
        thread {
            while (channel.isActive) {
                println(channel.consume())
                Thread.sleep(1000)
            }
        }
    }

    println("Press ENTER key to stop and exit")
    Scanner(System.`in`).nextLine()

    channel.close()
}