using andywiecko.BurstCollections;

namespace andywiecko.ECS
{
    public interface IEntity
    {
        Id<IEntity> EntityId { get; }
    }
}