using Unity.Jobs;

namespace andywiecko.ECS.Editor.Tests
{
    public class FakeSystem1 : BaseSystem
    {
        [SolverAction]
        private void Method1() { }

        [SolverAction]
        private void Method2() { }

        public override JobHandle Schedule(JobHandle dependencies) => dependencies;
    }
}