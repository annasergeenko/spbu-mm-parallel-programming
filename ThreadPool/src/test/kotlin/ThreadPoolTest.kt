import org.junit.jupiter.api.Assertions
import org.junit.jupiter.api.Test
import org.junit.jupiter.api.TestInstance
import task.AggregateException
import task.Task
import kotlin.test.assertFailsWith

@TestInstance(TestInstance.Lifecycle.PER_METHOD)
class ThreadPoolTest {
    @Test
    fun `single test completed`() {
        val pool = ThreadPool(1)
        val task = Task { 1 }

        Assertions.assertEquals(false, task.isCompleted)
        pool.enqueue(task)

        Assertions.assertEquals(1, task.result)
        Assertions.assertEquals(true, task.isCompleted)
        pool.close()
    }

    @Test
    fun `single worker, ten tasks`() {
        val pool = ThreadPool(1)
        val tasks = (0..9).map { Task { it } }
        tasks.forEach { pool.enqueue(it) }

        Thread.sleep(100)
        Assertions.assertEquals((0..9).toList(), tasks.map { it.result })
        pool.close()
    }

    @Test
    fun `correct close`() {
        val pool = ThreadPool(1)
        val tasks = (0..9).map { Task { it } }
        tasks.forEach { pool.enqueue(it) }

        pool.close()
        assertFailsWith<IllegalStateException> { pool.enqueue(Task { 10 }) }
    }

    @Test
    fun `with exception in task`() {
        val pool = ThreadPool(1)
        val task = Task { 1 / 0 }
        pool.enqueue(task)
        assertFailsWith<AggregateException> { task.result }
        Assertions.assertEquals(true, task.isCompleted)
        pool.close()
    }

    @Test
    fun `simple continue with`() {
        val pool = ThreadPool(1)

        val task = Task { 1 }.continueWith { it * 2 }.continueWith { it * 2 }.continueWith { it * 2 }
        pool.enqueue(task)

        Assertions.assertEquals(8, task.result)
        pool.close()
    }

    @Test
    fun `pool contains exactly n threads`() {
        val pool = ThreadPool(5)
        val list = generateSequence(1) { it + 1 }.take(1_000_000).toList()
        val tasks = (0..19).map { Task { list.filter { it % 3 == 0 }.average() } }
        tasks.forEach { pool.enqueue(it) }

        Thread.sleep(10)
        tasks[0].result
        tasks[1].result
        tasks[2].result
        tasks[3].result
        tasks[4].result
        Assertions.assertEquals(5, tasks.count { it.isCompleted })

        pool.close()
        tasks[5].result
        tasks[6].result
        tasks[7].result
        tasks[8].result
        tasks[9].result
        Assertions.assertEquals(10, tasks.count { it.isCompleted })
    }

    @Test
    fun `complete after close`() {
        val pool = ThreadPool(1)
        val list = generateSequence(1) { it + 1 }.take(1_000_000).toList()
        val tasks = (0..2).map { Task { list.filter { it % 3 == 0 }.average() } }
        tasks.forEach { pool.enqueue(it) }

        Thread.sleep(10)
        pool.close()
        tasks[0].result
        Assertions.assertTrue(tasks[0].isCompleted)
        Assertions.assertFalse(tasks[1].isCompleted)
    }
}