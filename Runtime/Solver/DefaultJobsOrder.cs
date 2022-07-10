#if UNITY_EDITOR
using andywiecko.ECS.Editor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace andywiecko.ECS
{
    [CreateAssetMenu(fileName = "DefaultJobsOrder", menuName = "ECS/Solver/Default Jobs Order")]
    public class DefaultJobsOrder : JobsOrder, IEnumerable<Type>
    {
        private enum SolverJob
        {
            Undefined = -1,
            Enabled
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
        private IEnumerable<Type> TargetTypes => TargetAssemblies
            .Select(i => Assembly.Load(i))
            .SelectMany(i => TypeCacheUtils.Systems.AssemblyToTypes[i]);

        [SerializeField]
        private UnityEditorInternal.AssemblyDefinitionAsset[] targetAssemblies = { };
#endif

        [Space(50)]
        [SerializeField] private List<SerializedType> enabledTypes = new();

        [Space(30)]
        [SerializeField]
        private List<UnconfiguredType> undefinedTypes = new();

#if UNITY_EDITOR
        private void Awake() => ValidateSerializedTypes();

        private void OnValidate()
        {
            TargetAssemblies = targetAssemblies?.GetNames().ToArray();

            // HACK:
            //   For unknown reason static dicts don't survive when saving asset,
            //   but OnValidate is called during save.
            if (TypeCacheUtils.SolverActions.GuidToType.Count != 0)
            {
                ValidateSerializedTypes();
            }
        }

        private void ValidateSerializedTypes()
        {
            RemoveBadTypes();
            AssignEnabledTypes();
            FillUndefinedTypes();
            ValidateEnabledTypes();
        }

        private void RemoveBadTypes()
        {
            enabledTypes.RemoveAll(i => !TypeCacheUtils.Systems.GuidToType.ContainsKey(i.Guid));
            undefinedTypes.RemoveAll(i => !TypeCacheUtils.Systems.GuidToType.ContainsKey(i.Type.Guid));
            enabledTypes = enabledTypes.DistinctBy(i => i.Guid).ToList();
        }

        private void AssignEnabledTypes()
        {
            var typesToAssign = undefinedTypes.Where(t => t.Job == SolverJob.Enabled);
            foreach (var u in typesToAssign)
            {
                enabledTypes.Add(u.Type);
            }
        }

        private void FillUndefinedTypes()
        {
            undefinedTypes.Clear();

            foreach (var t in TargetTypes)
            {
                if (!enabledTypes.Select(i => i.Type).Contains(t) &&
                    TypeCacheUtils.Systems.TypeToGuid.TryGetValue(t, out var guid))
                {
                    undefinedTypes.Add(new(t, guid));
                }
            }
        }

        private void ValidateEnabledTypes()
        {
            foreach (var t in enabledTypes)
            {
                t.Validate(TypeCacheUtils.Systems.GuidToType[t.Guid]);
            }
        }
#endif

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

        public IEnumerator<Type> GetEnumerator() => enabledTypes.Select(i => i.Type).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => (this as IEnumerable<Type>).GetEnumerator();
    }
}