using Unity.Jobs;

namespace andywiecko.ECS.Editor.Tests
{
    public class FakeSystem2 : ISystem
    {
        [SolverAction]
        private void Method1()
        {

        }

        public JobHandle Schedule(JobHandle dependencies)
        {
            throw new System.NotImplementedException();
        }
    }
}