using Unity.Jobs;

namespace andywiecko.ECS.Editor.Tests
{
    public class FakeSystem1 : ISystem
    {
        [SolverAction]
        private void Method1()
        {

        }

        [SolverAction]
        private void Method2()
        {

        }

        public JobHandle Schedule(JobHandle dependencies)
        {
            throw new System.NotImplementedException();
        }
    }
}