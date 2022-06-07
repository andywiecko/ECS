using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

namespace andywiecko.ECS
{
    public class Solver : MonoBehaviour
    {
        [field: SerializeField]
        public World World { get; private set; } = default;

        [field: SerializeField]
        public SolverJobsOrder JobsOrder { get; private set; } = default;

        [field: SerializeField]
        public SolverActionsOrder ActionsOrder { get; private set; } = default;

        public event Action OnScheduling;
        public event Action OnJobsComplete;

        private List<Func<JobHandle, JobHandle>> jobs = new();
        private JobHandle dependencies = new();

        public void ResetActions()
        {
            OnScheduling = null;
            OnJobsComplete = null;
        }

        private void Awake()
        {
            World.SystemsRegistry.OnRegistryChange += RegenerateSolverTasks;
        }

        public void Start()
        {
            RegenerateSolverTasks();
        }

        public void Update()
        {
            OnScheduling?.Invoke();
            ScheduleJobs().Complete();
            OnJobsComplete?.Invoke();
        }

        private JobHandle ScheduleJobs()
        {
            foreach (var job in jobs)
            {
                dependencies = job(dependencies);
            }
            return dependencies;
        }

        public void OnDestroy()
        {
            World.SystemsRegistry.OnRegistryChange -= RegenerateSolverTasks;
        }

        private void RegenerateSolverTasks()
        {
            jobs = JobsOrder.GenerateJobs(World);
            ActionsOrder.GenerateActions(this, World);
        }
    }
}
