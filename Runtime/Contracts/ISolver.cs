using System;
using System.Collections.Generic;
using Unity.Jobs;

namespace andywiecko.ECS
{
    public interface ISolver
    {
        List<Func<JobHandle, JobHandle>> Jobs { get; }
        event Action OnScheduling;
        event Action OnJobsComplete;
    }
}