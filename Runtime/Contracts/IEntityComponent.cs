using andywiecko.BurstCollections;

namespace andywiecko.ECS
{
    public interface IEntityComponent : IComponent
    {
        Id<IEntity> EntityId { get; }
    }
}