using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Si.Common.Reactive
{
    public interface IEventPublisher
    {
        void Publish<TEvent>(TEvent publishEvent);
        IObservable<TEvent> GetEvent<TEvent>();
    }
    public interface IEventPublisher<in T>
    {
        void Publish<TEvent>(TEvent publishEvent) where TEvent : T;
        IObservable<TEvent> GetEvent<TEvent>() where TEvent : T;
    }

    [Export(typeof(IEventPublisher))]
    public class EventPublisher : IEventPublisher
    {
        private readonly ConcurrentDictionary<Type, object> _subjects = new ConcurrentDictionary<Type, object>();
        private readonly IScheduler _scheduler;

        [ImportingConstructor]
        public EventPublisher() : this(DefaultScheduler.Instance) { }
        public EventPublisher(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public void Publish<TEvent>(TEvent publishEvent)
        {
            object subject;
            if (_subjects.TryGetValue(typeof(TEvent), out subject))
            {
                ((ISubject<TEvent>)subject).OnNext(publishEvent);
            }
        }

        public IObservable<TEvent> GetEvent<TEvent>()
        {
            var subject = (ISubject<TEvent>)_subjects.GetOrAdd(typeof(TEvent), t => new Subject<TEvent>());
            return subject.AsObservable().ObserveOn(_scheduler);
        }

    }

    [Export(typeof(IEventPublisher<>))]
    public class EventPublisher<T> : IEventPublisher<T>
    {
        private readonly IEventPublisher _eventPublisher;

        [ImportingConstructor]
        public EventPublisher(IEventPublisher eventPublisher)
        {
            _eventPublisher = eventPublisher;
        }

        public void Publish<TEvent>(TEvent publishEvent) where TEvent : T
        {
            _eventPublisher.Publish(publishEvent);
        }

        public IObservable<TEvent> GetEvent<TEvent>() where TEvent : T
        {
            return _eventPublisher.GetEvent<TEvent>();
        }

    }
}
