using System.Collections.Generic;

namespace Event_Bus
{
    public class EventBus<T> where T : IEvent
    {
        private static readonly HashSet<IEventBinding<T>> bindings = new();
        
        public static void Register(EventBinding<T> binding) => bindings.Add(binding);
        public static void Unregister(EventBinding<T> binding) => bindings.Remove(binding);

        public static void Raise(T @event)
        {
            foreach (IEventBinding<T> binding in bindings)
            {
                binding.OnEvent.Invoke(@event);
                binding.OnEventNoArgs.Invoke();
            }
        }
    }
}