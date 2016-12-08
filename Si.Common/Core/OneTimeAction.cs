using System;

namespace Si.Common.Core
{
    public class OneTimeAction
    {
        private readonly OneTimeAction<bool> _action;

        public OneTimeAction(Action action)
        {
            _action = new OneTimeAction<bool>(b => action());
        }

        public bool HasExecuted => _action.HasExecuted;

        public void Execute() => _action.Execute(false);

    }

    public class OneTimeAction<T>
    {
        private readonly OneTimeFunc<T, bool> _func;

        public OneTimeAction(Action<T> action)
        {
            _func = new OneTimeFunc<T, bool>(
                value =>
                {
                    action(value);
                    return false;
                });
        }

        public bool HasExecuted => _func.HasExecuted;

        public void Execute(T value) => _func.Execute(value);

    }
}
