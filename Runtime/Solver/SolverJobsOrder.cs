using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

namespace andywiecko.ECS
{
    public abstract class SolverJobsOrder : ScriptableObject
    {
        public abstract List<Func<JobHandle, JobHandle>> GenerateJobs(World world);
    }
}