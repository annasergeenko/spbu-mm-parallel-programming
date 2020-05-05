package threadPool

// An interface that user needs to implement to be able to add
// his own tasks to thread pool.
type Callable interface {
	Call()
	ContinueWith(func (interface{}) (interface{}, error)) Callable
}