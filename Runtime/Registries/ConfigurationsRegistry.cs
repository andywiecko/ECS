using System;
using System.Collections;
using System.Collections.Generic;

namespace andywiecko.ECS
{
    public class ConfigurationsRegistry : IEnumerable<KeyValuePair<Type, IConfiguration>>
    {
        public event Action OnRegistryChange;
        private readonly Dictionary<Type, IConfiguration> configs = new();

        public void Clear()
        {
            configs.Clear();
            OnRegistryChange = default;
        }

        public T GetOrCreate<T>() where T : class, IConfiguration, new()
        {
            T config;
            if (configs.TryGetValue(typeof(T), out var c))
            {
                config = c as T;
            }
            else
            {
                config = new T();
                Set(config);
            }
            return config;
        }
        public T Get<T>() where T : class, IConfiguration => configs[typeof(T)] as T;

        public void Set<T>(T config) where T : class, IConfiguration
        {
            var t = typeof(T);

            if (configs.ContainsKey(t) && config != null)
            {
                throw new ArgumentException($"Configuration of type {t} is already set in the registry!");
            }
            else
            {
                configs[typeof(T)] = config;
                OnRegistryChange?.Invoke();
            }
        }

        public IEnumerator<KeyValuePair<Type, IConfiguration>> GetEnumerator() => configs.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => (this as IEnumerable<KeyValuePair<Type, IConfiguration>>).GetEnumerator();
    }
}