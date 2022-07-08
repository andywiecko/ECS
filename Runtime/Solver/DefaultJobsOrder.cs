using andywiecko.ECS.Editor;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace andywiecko.ECS
{
    [CreateAssetMenu(fileName = "DefaultJobsOrder", menuName = "ECS/Solver/Default Jobs Order")]
    public class DefaultJobsOrder : JobsOrder
    {
        private enum SolverJob
        {
            Enabled,
            Undefined
        }

        [Serializable]
        private class UnconfiguredType
        {
            [HideInInspector, SerializeField]
            private string tag = "";

            [field: SerializeField]
            public SerializedType Type { get; private set; }

            [field: SerializeField]
            public SolverJob Job { get; private set; } = SolverJob.Undefined;

            public UnconfiguredType(Type type, string guid)
            {
                Type = new(type, guid);
                tag = type.Name.ToNonPascal();
            }
        }

        [field: SerializeField, HideInInspector]
        public string[] TargetAssemblies { get; private set; } = { };

#if UNITY_EDITOR
        [SerializeField]
        private UnityEditorInternal.AssemblyDefinitionAsset[] targetAssemblies = { };
#endif

        [Space(50)]
        [SerializeField] private List<SerializedType> enabledTypes = new();

        [Space(30)]
        [SerializeField]
        private List<UnconfiguredType> undefinedTypes = new();

        private void Awake() => ValidateTypes();

        private void OnValidate()
        {
            TargetAssemblies = targetAssemblies?
                .Where(i => i != null)
                .Select(i => JObject.Parse(i.text)["name"].ToString()).ToArray();

            // HACK:
            //   For unknown reason static dicts don't survive when saving asset,
            //   but OnValidate is called during save.
            if (TypeCacheUtils.SolverActions.GuidToType.Count != 0)
            {
                ValidateTypes();
            }
        }

        private void ValidateTypes()
        {
            enabledTypes.RemoveAll(i => !TypeCacheUtils.Systems.GuidToType.ContainsKey(i.Guid));
            undefinedTypes.RemoveAll(i => !TypeCacheUtils.Systems.GuidToType.ContainsKey(i.Type.Guid));
            enabledTypes = enabledTypes.DistinctBy(i => i.Guid).ToList();

            var methodsToAssign = undefinedTypes.Where(t => t.Job == SolverJob.Enabled);
            foreach (var u in methodsToAssign)
            {
                enabledTypes.Add(u.Type);
            }

            undefinedTypes.Clear();
            var targetTypes = TargetAssemblies
               .Select(i => Assembly.Load(i))
               .SelectMany(i => TypeCacheUtils.Systems.AssemblyToTypes[i]);
            foreach (var t in targetTypes)
            {
                if (!enabledTypes.Select(i => i.Type).Contains(t))
                {
                    undefinedTypes.Add(new(t, TypeCacheUtils.Systems.TypeToGuid[t]));
                }
            }

            foreach (var t in enabledTypes)
            {
                t.Validate(TypeCacheUtils.Systems.GuidToType[t.Guid]);
            }
        }

        public override void GenerateJobs(ISolver solver, IWorld world)
        {
            var jobs = solver.Jobs;
            var systems = world.SystemsRegistry;

            foreach (var type in enabledTypes.Select(i => i.Type))
            {
                if (systems.TryGetSystem(type, out var system))
                {
                    jobs.Add(system.Schedule);
                }
            }
        }
    }
}