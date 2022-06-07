using UnityEngine;

namespace andywiecko.ECS
{
    public class World : MonoBehaviour, IWorld
    {
        //public IdCounter<IEntity> EntityCounter { get; } = new(); // TODO: move to EntitesRegistry
        public ConfigurationsRegistry ConfigurationsRegistry { get; } = new();
        public ComponentsRegistry ComponentsRegistry { get; } = new();
        public SystemsRegistry SystemsRegistry { get; } = new();
    }
}