using Unity.Jobs;

namespace andywiecko.ECS.Editor.Tests
{
    [Category("Fake Category 1")]
    public class FakeSystem1 : BaseSystem
    {
        [SolverAction]
        private void Method1() { }

        [SolverAction]
        private void Method2() { }

        public override JobHandle Schedule(JobHandle dependencies) => dependencies;
    }
}