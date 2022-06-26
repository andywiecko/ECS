using UnityEngine;

namespace andywiecko.ECS
{
    public abstract class SolverJobsOrder : ScriptableObject
    {
        public abstract void GenerateJobs(ISolver solver, IWorld world);
    }
}