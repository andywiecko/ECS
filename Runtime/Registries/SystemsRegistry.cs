using System;
using System.Collections;
using System.Collections.Generic;

namespace andywiecko.ECS
{
    public class SystemsRegistry : IEnumerable<KeyValuePair<Type, ISystem>>
    {
        public event Action OnRegistryChange;
        private readonly Dictionary<Type, ISystem> systems = new();

        public void Clear()
        {
            systems.Clear();
            OnRegistryChange = default;
        }

        public bool TryGetSystem(Type type, out ISystem system) => systems.TryGetValue(type, out system);
        public T Get<T>() where T : class, ISystem => systems[typeof(T)] as T;

        public void Add<T>(T system) where T : class, ISystem
        {
            var type = system.GetType();

            if (!systems.ContainsKey(type))
            {
                systems[type] = system;
                OnRegistryChange?.Invoke();
            }
            else
            {
                throw new ArgumentException($"System of type {type} is already in the registry!");
            }
        }

        public void Remove<T>(T system) where T : class, ISystem
        {
            var type = system.GetType();
            if (systems.ContainsKey(type))
            {
                systems.Remove(type);
                OnRegistryChange?.Invoke();
            }
        }

        public IEnumerator<KeyValuePair<Type, ISystem>> GetEnumerator() => systems.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => (this as IEnumerable<KeyValuePair<Type, ISystem>>).GetEnumerator();
    }
}