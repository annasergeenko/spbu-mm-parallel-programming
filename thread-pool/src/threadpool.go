package threadPool

import (
	"fmt"
	"sync"
)

type FullQueueError struct {
	c Callable
}

func (e *FullQueueError) Error() string {
	return fmt.Sprintf("job queue is full, not able to add new job: %v", e.c)
}

// Simple thread pool implementation
type ThreadPool struct {
	QueueSize   int
	NumWorkers  int
	waitGroup   sync.WaitGroup

	jobQueue    chan Callable
	workerPool  chan chan Callable
	closeHandle chan bool
}

func NewThreadPool(queueSize int, numWorkers int) *ThreadPool {
	tp := &ThreadPool{
		QueueSize:   queueSize,
		NumWorkers:  numWorkers,
		jobQueue:    make(chan Callable, queueSize),
		workerPool:  make(chan chan Callable, numWorkers),
		closeHandle: make(chan bool),
	}

	tp.init()
	return tp
}

// Creates and runs workers, starts looking for available jobs.
func (tp *ThreadPool) init() {
	for i := 0; i < tp.NumWorkers; i++ {
		worker := NewWorker(tp.workerPool, tp.closeHandle, &tp.waitGroup)
		tp.waitGroup.Add(1)
		worker.Start()
	}
	go tp.dispatch()
}

func (tp *ThreadPool) dispatch() {
	for {
		select {
		case job := <-tp.jobQueue:
			<-tp.workerPool<-job
		case <-tp.closeHandle:
			// ThreadPool has been closed
			return
		}
	}
}

// Wait for all workers finishing their tasks (that are already executing)
// and close the pool.
func (tp *ThreadPool) Close() {
	close(tp.closeHandle)
	tp.waitGroup.Wait()

	close(tp.workerPool)
	close(tp.jobQueue)
}

// Add a job to the thread pool. Return error if job queue was full.
func (tp *ThreadPool) AddJob(c Callable) error {
	if len(tp.jobQueue) == tp.QueueSize {
		return &FullQueueError{c}
	}
	tp.jobQueue <- c
	return nil
}