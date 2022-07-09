using Unity.Jobs;

namespace andywiecko.ECS.Editor.Tests
{
    public class FakeSystem2 : BaseSystem
    {
        [SolverAction]
        private void Method1() { }

        public override JobHandle Schedule(JobHandle dependencies) => dependencies;
    }
}