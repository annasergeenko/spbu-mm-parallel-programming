package hw;
public interface ThreadPool {
    void submit(iMyTask task);
    void awaitCompletion() throws InterruptedException;
}