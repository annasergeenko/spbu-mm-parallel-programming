package hw;

import java.util.concurrent.Callable;
import java.util.concurrent.CancellationException;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.atomic.AtomicInteger;

public class iMyTask<V> {
    private final Callable task;
    private final AtomicInteger status = new AtomicInteger(TaskStatus.NOTSTARTED.getValue());
    private Exception exception;
    private V result;
    private Thread thread;
    private boolean isCompleted=false;


    iMyTask(Callable task) {
        this.task = task;
    }

    TaskStatus Result() {
        return TaskStatus.get(status.get());
    }

    boolean cancel() {
        TaskStatus s = Result();

        if (s == TaskStatus.DONE) {
            return true;
        }

        if (s == TaskStatus.CANCELLED) {
            return false;
        }

        if (casStatus(TaskStatus.NOTSTARTED, TaskStatus.CANCELLED)) {
            return false;
        }
        casStatus(TaskStatus.RUNNING, TaskStatus.CANCELLED);
        casStatus(TaskStatus.NOTSTARTED, TaskStatus.CANCELLED);
        if (Result() == TaskStatus.CANCELLED) {
            thread.interrupt();
            notifyTask();
        }
        return false;
    }

    public void get() throws InterruptedException, ExecutionException {
        if (casStatus(TaskStatus.NOTSTARTED, TaskStatus.RUNNING)) {
            try {
                thread = Thread.currentThread();
                result = (V) task.call();
            } catch (Exception ex) {
                exception = ex;
            } finally {
                casStatus(TaskStatus.RUNNING, TaskStatus.DONE);
                isCompleted=true;
                notifyTask();
            }
        } else {
            boolean fromPool = Thread.currentThread() instanceof FixedThreadPool.FixedThreadPoolThread;
            if (!fromPool) {
                while (!(isCompleted() || Result() == TaskStatus.CANCELLED)) {
                    waitTask();
                }
            }
        }

        if ((exception instanceof InterruptedException) ||Result() == TaskStatus.CANCELLED) {
            throw new CancellationException();
        }

        if (exception != null) {
            throw new ExecutionException(exception);
        }

    }

    private boolean casStatus(TaskStatus from, TaskStatus to) {
        return status.compareAndSet(from.getValue(), to.getValue());
    }

    private void notifyTask() {
        synchronized (task) {
            task.notify();
        }
    }

    private void waitTask() throws InterruptedException {
        synchronized (task) {
            task.wait();
        }
    }
    private boolean isCompleted() {
        return isCompleted;
    }
}