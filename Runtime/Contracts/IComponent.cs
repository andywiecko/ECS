using andywiecko.BurstCollections;

namespace andywiecko.ECS
{
    public interface IComponent
    {
        Id<IComponent> ComponentId { get; }
    }
}