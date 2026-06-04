using System;
using System.Collections.Generic;

namespace DoenerEmpire.Core
{
    public sealed class EventBus
    {
        private readonly Dictionary<Type, List<Delegate>> handlers = new();

        public IDisposable Subscribe<TEvent>(Action<TEvent> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            Type eventType = typeof(TEvent);
            if (!handlers.TryGetValue(eventType, out List<Delegate> eventHandlers))
            {
                eventHandlers = new List<Delegate>();
                handlers[eventType] = eventHandlers;
            }

            eventHandlers.Add(handler);
            return new Subscription(() => eventHandlers.Remove(handler));
        }

        public void Publish<TEvent>(TEvent value)
        {
            if (!handlers.TryGetValue(typeof(TEvent), out List<Delegate> eventHandlers))
            {
                return;
            }

            foreach (Delegate handler in eventHandlers.ToArray())
            {
                ((Action<TEvent>)handler).Invoke(value);
            }
        }

        private sealed class Subscription : IDisposable
        {
            private Action unsubscribe;

            public Subscription(Action unsubscribe)
            {
                this.unsubscribe = unsubscribe;
            }

            public void Dispose()
            {
                unsubscribe?.Invoke();
                unsubscribe = null;
            }
        }
    }
}
