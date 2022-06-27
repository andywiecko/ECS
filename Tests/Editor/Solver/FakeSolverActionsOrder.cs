namespace andywiecko.ECS.Editor.Tests
{
    internal class FakeSolverActionsOrder : SolverActionsOrder
    {
        public int GenerationCount { get; private set; } = 0;
        public int UpdateCount { get; private set; } = 0;

        public void Reset() => UpdateCount = GenerationCount = 0;

        public override void GenerateActions(ISolver solver, IWorld world)
        {
            GenerationCount++;

            solver.OnScheduling += () => UpdateCount++;
            solver.OnJobsComplete += () => UpdateCount++;
        }
    }
}