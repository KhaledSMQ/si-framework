using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Si.Service
{
    public abstract class ApplicationService : IService
    {
        // Rx stream used to ensure one service action at a time is processed.
        private readonly Subject<ServiceAction> _serviceActions;

        protected ApplicationService()
        {
            _serviceActions = new Subject<ServiceAction>();

            State = ServiceState.Stopped;
            _serviceActions.Subscribe(ProcessServiceAction);
        }

        protected ServiceState State { get; private set; }

        public Task StartAsync()
        {
            var action = new ServiceAction(
                DoStartAsync,
                "start service",
                expectedCurrentState: ServiceState.Stopped,
                transientState: ServiceState.Starting,
                successState: ServiceState.Running);

            _serviceActions.OnNext(action);

            return AwaitCompletion(action);
        }

        public Task StopAsync()
        {
            var action = new ServiceAction(
                DoStopAsync,
                "stop service",
                expectedCurrentState: ServiceState.Running,
                transientState: ServiceState.Stopping,
                successState: ServiceState.Stopped);

            _serviceActions.OnNext(action);

            return AwaitCompletion(action);
        }

        protected virtual Task OnStartAsync()
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnStopAsync()
        {
            return Task.CompletedTask;
        }

        private void ProcessServiceAction(ServiceAction action)
        {
            if (State != action.ExpectedCurrentState)
            {
                // Suppress service action if service is not in expected state.
                action.Result = ServiceActionResult.SuccessfulAction();
            }
            else
            {
                State = action.TransientState;
                bool complete = false;

                var tasks = new Task[]
                {
                    // Run service action.
                    action.Action()
                        .ContinueWith(t => complete = true),
                    // Timeout task.
                    Task.Delay(TimeSpan.FromSeconds(10))
                        .ContinueWith(t =>
                        {
                            if (!complete)
                            {
                                throw new Exception("Timed out waiting for action to complete.");
                            }
                        }),
                };

                int index = Task.WaitAny(tasks);
                var completed = tasks[index];

                if (completed.IsFaulted &&
                    completed.Exception != null)
                {
                    action.Result = ServiceActionResult.FailedAction(
                        completed.Exception.Flatten());

                    // Action failed or timed out. Return service to previous state.
                    State = action.ExpectedCurrentState;
                }
                else
                {
                    action.Result = ServiceActionResult.SuccessfulAction();
                    State = action.SuccessState;
                }
            }

            action.Completed.Release(1);
        }

        private static Task AwaitCompletion(ServiceAction serviceAction)
        {
            return serviceAction.Completed
                .WaitAsync()
                .ContinueWith(t =>
                {
                    if (!serviceAction.Result?.Success ?? false)
                    {
                        throw new Exception(
                            $"Failed to {serviceAction.Description}. See inner exception for details.",
                            serviceAction.Result?.Error);
                    }
                });
        }

        private Task DoStartAsync()
        {
            return OnStartAsync();
        }

        private Task DoStopAsync()
        {
            return OnStopAsync();
        }

        protected enum ServiceState
        {
            Stopped,
            Starting,
            Running,
            Stopping,
        }

        private class ServiceAction
        {
            public ServiceAction(
                Func<Task> action,
                string description,
                ServiceState expectedCurrentState,
                ServiceState transientState,
                ServiceState successState)
            {
                Completed = new SemaphoreSlim(0, 1);
                Action = action;
                Description = description;
                ExpectedCurrentState = expectedCurrentState;
                TransientState = transientState;
                SuccessState = successState;
            }

            public SemaphoreSlim Completed { get; }
            public Func<Task> Action { get; }
            public string Description { get; }
            public ServiceState ExpectedCurrentState { get; }
            public ServiceState TransientState { get; }
            public ServiceState SuccessState { get; }
            public ServiceActionResult Result { get; set; }

        }

        private class ServiceActionResult
        {
            private ServiceActionResult(bool success, Exception error)
            {
                Success = success;
                Error = error;
            }

            public bool Success { get; }
            public Exception Error { get; }

            public static ServiceActionResult SuccessfulAction()
                => new ServiceActionResult(true, null);

            public static ServiceActionResult FailedAction(Exception errpr)
                => new ServiceActionResult(false, errpr);
        }

    }
}
