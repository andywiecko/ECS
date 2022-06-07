namespace andywiecko.ECS
{
    public interface IWorld
    {
        ConfigurationsRegistry ConfigurationsRegistry { get; }
        ComponentsRegistry ComponentsRegistry { get; }
        SystemsRegistry SystemsRegistry { get; }
    }
}
