using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

namespace andywiecko.ECS
{
    public class Solver : MonoBehaviour, ISolver
    {
        [field: SerializeField]
        public World World { get; private set; } = default;

        [field: SerializeField]
        public SolverJobsOrder JobsOrder { get; private set; } = default;

        [field: SerializeField]
        public SolverActionsOrder ActionsOrder { get; private set; } = default;

        public List<Func<JobHandle, JobHandle>> Jobs { get; } = new();
        public event Action OnScheduling;
        public event Action OnJobsComplete;

        private JobHandle dependencies = new();

        private void Awake()
        {
            World.SystemsRegistry.OnRegistryChange += RegenerateSolverTasks;
        }

        private void Start()
        {
            RegenerateSolverTasks();
        }

        private void Update()
        {
            OnScheduling?.Invoke();
            ScheduleJobs().Complete();
            OnJobsComplete?.Invoke();
        }

        private JobHandle ScheduleJobs()
        {
            foreach (var job in Jobs)
            {
                dependencies = job(dependencies);
            }
            return dependencies;
        }

        private void OnDestroy()
        {
            World.SystemsRegistry.OnRegistryChange -= RegenerateSolverTasks;
        }

        private void RegenerateSolverTasks()
        {
            Jobs.Clear();
            OnScheduling = null;
            OnJobsComplete = null;

            JobsOrder.GenerateJobs(this, World);
            ActionsOrder.GenerateActions(this, World);
        }
    }
}
