namespace andywiecko.ECS.Editor.Tests
{
    internal class FakeSolverJobsOrder : SolverJobsOrder
    {
        public int GenerationCount { get; private set; } = 0;
        public int UpdateCount { get; private set; } = 0;

        public void Reset() => UpdateCount = GenerationCount = 0;

        public override void GenerateJobs(ISolver solver, IWorld world)
        {
            GenerationCount++;

            solver.Jobs.Add(_ => { UpdateCount++; return default; });
        }
    }
}