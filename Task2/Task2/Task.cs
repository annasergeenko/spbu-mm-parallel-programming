using System;

namespace Task2
{
    public interface ITask
    {
        bool IsCompleted();
        bool CanBeExecuted();
        void Execute();
    }

    public interface ITask<T> : ITask
    {
        T Result();
        ITask<S> ContinueWith<S>(Func<T, S> function);
    }
}
