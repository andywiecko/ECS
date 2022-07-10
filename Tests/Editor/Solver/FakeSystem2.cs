using Unity.Jobs;

namespace andywiecko.ECS.Editor.Tests
{
    [Category("Fake Category 2")]
    public class FakeSystem2 : BaseSystem
    {
        [SolverAction]
        private void Method1() { }

        public override JobHandle Schedule(JobHandle dependencies) => dependencies;
    }
}