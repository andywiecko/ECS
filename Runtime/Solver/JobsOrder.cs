using UnityEngine;

namespace andywiecko.ECS
{
    public abstract class JobsOrder : ScriptableObject
    {
        public abstract void GenerateJobs(ISolver solver, IWorld world);
    }
}