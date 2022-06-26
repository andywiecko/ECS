using System;
using System.Collections.Generic;

namespace andywiecko.ECS
{
    public class SystemsRegistry
    {
        public event Action OnRegistryChange;
        private readonly Dictionary<Type, ISystem> systems = new();

        public void Clear()
        {
            systems.Clear();
            OnRegistryChange = default;
        }

        public bool TryGetSystem(Type type, out ISystem system) => systems.TryGetValue(type, out system);

        public void Add<T>(T system) where T : ISystem
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

        public void Remove<T>(T system) where T : ISystem
        {
            var type = system.GetType();
            if (systems.ContainsKey(type))
            {
                systems.Remove(type);
                OnRegistryChange?.Invoke();
            }
        }
    }
}