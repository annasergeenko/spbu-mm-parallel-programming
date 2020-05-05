package threadPool

import (
	"testing"
	"time"
)

func TestThreadPool_ThreadCount(t *testing.T) {
	numThreads := 128
	tp := NewThreadPool(0, numThreads)

	// Wait for 0.5s to let thread pool create initialize all goroutines.
	time.Sleep(500 * time.Millisecond)
	if len(tp.workerPool) != numThreads {
		t.Errorf(
			"Wrong threads number: got %d, expected %d",
			len(tp.workerPool),
			numThreads)
	}
	tp.Close()
}

func TestThreadPool_AddJob(t *testing.T) {
	tp := NewThreadPool(1, 0)
	job := NewTask(func() (interface{}, error) { return nil, nil })

	if err := tp.AddJob(job); err != nil {
		t.Error(err.Error())
	}
	tp.Close()
}

func TestThreadPool_FullJobQueue(t *testing.T) {
	tp := NewThreadPool(0, 0)
	job := NewTask(func() (interface{}, error) { return nil, nil })

	if err := tp.AddJob(job); err == nil {
		t.Errorf("Test must not be able to add a job to empty job queue")
	}
	tp.Close()
}

func TestThreadPool_AddManyJobs(t *testing.T) {
	queueSize, numThreads := 3, 1
	tp := NewThreadPool(queueSize, numThreads)

	for i := 0; i < queueSize; i++ {
		err := tp.AddJob(NewTask(func() (interface{}, error) {
			time.Sleep(100 * time.Millisecond)
			return nil, nil
		}))
		if err != nil {
			t.Errorf("Failed to add job #%d", i)
		}
	}
	tp.Close()
}

func TestTask_ContinueWith(t *testing.T) {
	queueSize, numThreads := 20, 2
	tp := NewThreadPool(queueSize, numThreads)
	referenceMap := make(map[*Task]int)

	for i := 0; i < queueSize / 2; i++ {
		job := NewTask(func() (interface{}, error) {
			time.Sleep(100 * time.Millisecond)
			return i, nil
		})
		if err := tp.AddJob(job); err != nil {
			t.Errorf("Failed to add main job #%d", i)
		}

		continueWithJob := job.ContinueWith(func(res interface{}) (interface{}, error) {
			time.Sleep(50 * time.Millisecond)
			return res, nil
		})
		// Save a result we need to check later.
		referenceMap[continueWithJob.(*Task)] = i
		if err := tp.AddJob(continueWithJob); err != nil {
			t.Errorf("Failed to add continueWithJob #%d", i)
		}
	}

	// Let all jobs finish.
	//
	// Current closing politic is to let all jobs that are already assigned to a worker finish
	// and do not execute remaining ones. So, just for the test let all jobs finish.
	time.Sleep(500 * time.Millisecond)

	tp.Close()

	// Check the results.
	for job, referenceResult := range referenceMap {
		if job.Result != referenceResult {
			t.Errorf(
				"Failed to compare reference and actual results: got %d, expected %d",
				job.Result,
				referenceResult)
		}
	}
}