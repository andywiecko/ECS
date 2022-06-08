using System;
using System.Collections.Generic;

namespace andywiecko.ECS
{
    public class ConfigurationsRegistry
    {
        public event Action OnRegistryChange;
        private readonly Dictionary<Type, IConfiguration> configs = new();

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
    }
}