import org.junit.jupiter.api.Assertions
import org.junit.jupiter.api.Test
import org.junit.jupiter.api.TestInstance
import java.util.concurrent.atomic.AtomicInteger
import kotlin.concurrent.thread
import kotlin.test.assertFailsWith

@TestInstance(TestInstance.Lifecycle.PER_CLASS)
class ProducerConsumerTest {
    @Test
    fun `one thread channel`() {
        val channel = Channel<String>()
        val expected = "Some string"

        channel.produce(expected)
        Assertions.assertEquals(expected, channel.consume())

        channel.close()
    }

    @Test
    fun `two thread channel, first consume`() {
        val channel = Channel<String>()
        val expected = "Some string"

        var actual = ""
        thread { actual = channel.consume() }
        channel.produce(expected)

        Thread.sleep(20)
        Assertions.assertEquals(expected, actual)
        channel.close()
    }

    @Test
    fun `one producer, two consumers`() {
        val channel = Channel<Int>()
        var i = 0
        val repeatCount = 10

        val expected = (0 until repeatCount).toList()
        thread { repeat(repeatCount) { channel.produce(i++) } }

        val actual = mutableListOf<Int>()
        repeat(2) {
            thread {
                repeat(repeatCount / 2) {
                    actual += channel.consume()
                    Thread.sleep(10)
                }
            }
        }

        Thread.sleep(200)
        Assertions.assertEquals(expected, actual.sorted())
        channel.close()
    }

    @Test
    fun `two producers, one consumer`() {
        val channel = Channel<Int>()
        val i = AtomicInteger()
        val repeatCount = 10

        val expected = (0 until repeatCount).toList()
        repeat(2) {
            thread {
                repeat(repeatCount / 2) {
                    channel.produce(i.getAndIncrement())
                    Thread.sleep(10)
                }
            }
        }

        val actual = mutableListOf<Int>()
        thread { repeat(repeatCount) { actual += channel.consume() } }

        Thread.sleep(200)
        Assertions.assertEquals(expected, actual.sorted())
        channel.close()
    }

    @Test
    fun `three producers, three consumers`() {
        val channel = Channel<Int>()
        val i = AtomicInteger()
        val repeatCount = 30

        val expected = (0 until repeatCount).toList()
        repeat(3) {
            thread {
                repeat(repeatCount / 3) {
                    channel.produce(i.getAndIncrement())
                    Thread.sleep(10)
                }
            }
        }

        val actual = mutableListOf<Int>()
        repeat(3) {
            thread {
                repeat(repeatCount / 3) {
                    actual += channel.consume()
                    Thread.sleep(10)
                }
            }
        }

        Thread.sleep(500)
        Assertions.assertEquals(expected, actual.sorted())
        channel.close()
    }

    @Test
    fun `exception after close`() {
        val channel = Channel<Int>()
        channel.close()
        assertFailsWith<ChannelClosedException> { channel.produce(0) }
        assertFailsWith<ChannelClosedException> { channel.consume() }
    }
}