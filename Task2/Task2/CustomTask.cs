using System;
using System.Threading;

namespace Task2
{
    public class CustomTask<T> : ITask<T>
    {
        private bool Completed;
        private Func<T> Function;
        private T Res;
        private ITask DependOn;

        public CustomTask(Func<T> function, ITask dependOn = null)
        {
            Completed = false;
            Function = function;
            DependOn = dependOn;
        }

        public ITask<S> ContinueWith<S>(Func<T, S> function)
        {
            return new CustomTask<S>(() => {
                while (!IsCompleted())
                {
                    Thread.Sleep(1);
                };
                return function(Result());
            }, this);
        }

        public bool IsCompleted()
        {
            return Completed;
        }

        public T Result()
        {
            if (!Completed)
            {
                try
                {
                    Res = Function.Invoke();
                }
                catch(Exception e)
                {
                    throw new AggregateException(e);
                }
                Completed = true;
            }
            return Res;
        }

        public void Execute()
        {
            T result = Result();
            Console.WriteLine(result);
        }

        public bool CanBeExecuted()
        {
            return ((DependOn == null) || DependOn.IsCompleted());
        }
    }
}
