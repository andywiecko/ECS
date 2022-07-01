using System.Collections.Generic;
using Unity.Jobs;

namespace andywiecko.ECS
{
    public abstract class BaseSystem : ISystem
    {
        public IWorld World { get; set; }
        public abstract JobHandle Schedule(JobHandle dependencies);
        public void Run() => Schedule(default).Complete();
    }

    public abstract class BaseSystem<TComponent> : BaseSystem where TComponent : IComponent
    {
        protected IReadOnlyList<TComponent> References => World.ComponentsRegistry.GetComponents<TComponent>();
    }

    public abstract class BaseSystemWithConfiguration<TConfiguration> : BaseSystem
        where TConfiguration : class, IConfiguration
    {
        protected TConfiguration Configuration => World.ConfigurationsRegistry.Get<TConfiguration>();
    }

    public abstract class BaseSystemWithConfiguration<TComponent, TConfiguration> : BaseSystem<TComponent>
        where TComponent : IComponent
        where TConfiguration : class, IConfiguration
    {
        protected TConfiguration Configuration => World.ConfigurationsRegistry.Get<TConfiguration>();
    }
}