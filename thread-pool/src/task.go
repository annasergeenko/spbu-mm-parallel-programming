package threadPool

// Example implementation of Callable interface
type Task struct {
	Func        func() (interface{}, error)
	Result      interface{}
	Err         error
	IsCompleted bool
	syncChan    chan struct{}
}

func NewTask(f func() (interface{}, error)) *Task {
	return &Task{Func: f, syncChan: make(chan struct{})}
}

func (t *Task) Call() {
	t.Result, t.Err = t.Func()
	t.IsCompleted = true
	close(t.syncChan)
}

func (t *Task) ContinueWith(f func (interface{}) (interface{}, error)) Callable {
	if !t.IsCompleted { <-t.syncChan }

	return NewTask(func() (interface{}, error) {
		return f(t.Result)
	})
}