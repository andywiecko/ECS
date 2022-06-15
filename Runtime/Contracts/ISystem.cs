using Unity.Jobs;

namespace andywiecko.ECS
{
    public interface ISystem
    {
        JobHandle Schedule(JobHandle dependencies);
    }
}