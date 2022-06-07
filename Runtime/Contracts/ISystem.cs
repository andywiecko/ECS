using Unity.Jobs;

namespace andywiecko.ECS
{
    public interface ISystem
    {
        IWorld World { get; set; }
        JobHandle Schedule(JobHandle dependencies);
    }
}