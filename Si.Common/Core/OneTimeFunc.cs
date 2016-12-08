using System;

namespace Si.Common.Core
{
    public class OneTimeFunc<TResult>
    {
        private readonly OneTimeFunc<bool, TResult> _func;

        public OneTimeFunc(Func<TResult> func)
        {
            _func = new OneTimeFunc<bool, TResult>(b => func());
        }

        public bool HasExecuted => _func.HasExecuted;

        public TResult Execute() => _func.Execute(false);

    }

    public class OneTimeFunc<T1, TResult>
    {
        private readonly Func<T1, TResult> _func;
        private readonly object _lock;

        private TResult _result;

        public OneTimeFunc(Func<T1, TResult> func)
        {
            _func = func;
            _lock = new object();
            HasExecuted = false;
        }

        public bool HasExecuted { get; private set; }

        public TResult Execute(T1 t1)
        {
            if (HasExecuted) return _result;
            lock (_lock)
            {
                if (HasExecuted) return _result;

                _result = _func(t1);
                HasExecuted = true;

                return _result;
            }
        }

    }
}
