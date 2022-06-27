using UnityEngine;

namespace andywiecko.ECS
{
    public abstract class ConfigurationHolder<T> : MonoBehaviour where T : class, IConfiguration
    {
        [field: SerializeField]
        public World World { get; private set; } = default;

        [field: SerializeField]
        public T Configuration { get; private set; } = default;

        protected void Awake()
        {
            World.ConfigurationsRegistry.Set(Configuration);
        }
    }
}