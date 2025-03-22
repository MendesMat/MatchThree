using System.Collections.Generic;
using UnityEngine;

namespace MatchThree.Project.Scripts.Core.EventBus
{
    public static class EventBus<T> where T : IEvent
    {
        private static readonly HashSet<IEventBinding<T>> Bindings = new();
        
        public static void Register(EventBinding<T> binding) => Bindings.Add(binding);
        public static void Unregister(EventBinding<T> binding) => Bindings.Remove(binding);
        
        public static void Publish(T tEvent)
        {
            foreach (var binding in Bindings)
            {
                binding.OnEvent(tEvent); 
                binding.OnEventNoArgs();
            }
        }
        
        private static void ClearBus()
        {
            Debug.Log($"Clearing {typeof(T).Name} bindings");
            Bindings.Clear();
        }
    }
}