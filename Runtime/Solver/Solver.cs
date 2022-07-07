using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

namespace andywiecko.ECS
{
    public class Solver : MonoBehaviour, ISolver
    {
        public event Action OnScheduling;
        public event Action OnJobsComplete;
        public List<Func<JobHandle, JobHandle>> Jobs { get; } = new();

        [field: SerializeField]
        public World World { get; private set; } = default;

        [SerializeField]
        private JobsOrder jobsOrder = default;

        [SerializeField]
        private ActionsOrder actionsOrder = default;

        private JobHandle dependencies = new();

        private void Awake() => World.SystemsRegistry.OnRegistryChange += RegenerateSolverTasks;
        private void Start() => RegenerateSolverTasks();

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

        private void OnDestroy() => World.SystemsRegistry.OnRegistryChange -= RegenerateSolverTasks;

        private void RegenerateSolverTasks()
        {
            Jobs.Clear();
            OnScheduling = null;
            OnJobsComplete = null;

            jobsOrder.GenerateJobs(this, World);
            actionsOrder.GenerateActions(this, World);
        }
    }
}
