package threadPool

import "sync"

// Worker that executes tasks from thread pool's job queue
type Worker struct {
	jobChannel  chan Callable
	workerPool  chan chan Callable
	closeHandle chan bool
	waitGroup   *sync.WaitGroup
}

func NewWorker(
	workerPool  chan chan Callable,
	closeHandle chan bool,
	waitGroup   *sync.WaitGroup) *Worker {
	return &Worker{
		jobChannel:  make(chan Callable),
		workerPool:  workerPool,
		closeHandle: closeHandle,
		waitGroup:   waitGroup,
	}
}

func (w Worker) Start() {
	go func() {
		defer w.waitGroup.Done()

		for {
			// Add worker to the pool
			w.workerPool <- w.jobChannel
			select {
				case job := <-w.jobChannel:
					job.Call()
				case <-w.closeHandle:
					// Worker stops its work
					return
			}
		}
	}()
}