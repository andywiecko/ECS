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
    [CreateAssetMenu(fileName = "DefaultActionsOrder", menuName = "ECS/Solver/Default Actions Order")]
    public class DefaultActionsOrder : ActionsOrder, IEnumerable<KeyValuePair<SolverAction, IReadOnlyList<(MethodInfo, Type)>>>
    {
        [Serializable]
        private class UnconfiguredMethod
        {
            [HideInInspector, SerializeField]
            private string tag = "";

            [field: SerializeField]
            public SerializedMethod Method { get; private set; }

            [field: SerializeField]
            public SolverAction Action { get; private set; } = SolverAction.Undefined;

            public UnconfiguredMethod(MethodInfo method, string guid)
            {
                Method = new(method, guid);
                tag = $"{Method.MethodName.ToNonPascal()} ({Method.Type.Name.ToNonPascal()})";
            }
        }

        [field: SerializeField, HideInInspector]
        public string[] TargetAssemblies { get; private set; } = { };

#if UNITY_EDITOR
        private IEnumerable<(MethodInfo methodInfo, Type type)> TargetTypes => TypeCacheUtils.SolverActions.GetMethods(TargetAssemblies);
        [SerializeField]
        private UnityEditorInternal.AssemblyDefinitionAsset[] targetAssemblies = { };
#endif

        [Space(50)]
        [SerializeField] private List<SerializedMethod> onScheduling = new();
        [SerializeField] private List<SerializedMethod> onJobsCompletion = new();

        [Space(30)]
        [SerializeField]
        private List<UnconfiguredMethod> undefinedMethods = new();

        private readonly Dictionary<SolverAction, IReadOnlyList<(MethodInfo, Type)>> actionsOrder = new();

#if UNITY_EDITOR
        private void Awake() => ValidateMethods();

        private void OnValidate()
        {
            TargetAssemblies = targetAssemblies?.GetNames().ToArray();

            // HACK:
            //   For unknown reason static dicts don't survive when saving asset,
            //   but OnValidate is called during save.
            if (TypeCacheUtils.SolverActions.GuidToType.Count != 0)
            {
                ValidateMethods();
            }
        }

        private void ValidateMethods()
        {
            RemoveBadTypes();
            AssignEnabledTypes();
            FillUndefinedTypes();
            ValidateEnabledMethods(onScheduling);
            ValidateEnabledMethods(onJobsCompletion);

            actionsOrder.Clear();
            actionsOrder.Add(SolverAction.OnScheduling, onScheduling.Select(i => i.Value).ToArray());
            actionsOrder.Add(SolverAction.OnJobsCompletion, onJobsCompletion.Select(i => i.Value).ToArray());
        }

        private void RemoveBadTypes()
        {
            onScheduling.RemoveAll(i => !TypeCacheUtils.SolverActions.GuidToType.ContainsKey(i.SerializedType.Guid));
            onJobsCompletion.RemoveAll(i => !TypeCacheUtils.SolverActions.GuidToType.ContainsKey(i.SerializedType.Guid));
            undefinedMethods.RemoveAll(i => !TypeCacheUtils.SolverActions.GuidToType.ContainsKey(i.Method.SerializedType.Guid));

            onScheduling = onScheduling.DistinctBy(i => i.Value).ToList();
            onJobsCompletion = onJobsCompletion.DistinctBy(i => i.Value).ToList();
        }

        private void AssignEnabledTypes()
        {
            var methodsToAssign = undefinedMethods.Where(t => t.Action != SolverAction.Undefined);
            foreach (var u in methodsToAssign)
            {
                switch (u.Action)
                {
                    case SolverAction.OnScheduling:
                        onScheduling.Add(u.Method);
                        break;

                    case SolverAction.OnJobsCompletion:
                        onJobsCompletion.Add(u.Method);
                        break;
                }
            }
        }

        private void FillUndefinedTypes()
        {
            undefinedMethods.Clear();

            foreach (var (m, t) in TargetTypes)
            {
                if (!onScheduling.Select(i => i.Value).Contains((m, t)) &&
                    !onJobsCompletion.Select(i => i.Value).Contains((m, t)) &&
                    TypeCacheUtils.SolverActions.TypeToGuid.TryGetValue(t, out var guid))
                {
                    undefinedMethods.Add(new(m, guid));
                }
            }
        }

        private void ValidateEnabledMethods(IEnumerable<SerializedMethod> target)
        {
            foreach (var i in target)
            {
                i.Validate(TypeCacheUtils.Systems.GuidToType[i.SerializedType.Guid]);
            }
        }
#endif

        public override void GenerateActions(ISolver solver, IWorld world)
        {
            foreach (var (method, type) in onScheduling)
            {
                if (world.SystemsRegistry.TryGetSystem(type, out var system))
                {
                    solver.OnScheduling += () => method.Invoke(system, default);
                }
            }

            foreach (var (method, type) in onJobsCompletion)
            {
                if (world.SystemsRegistry.TryGetSystem(type, out var system))
                {
                    solver.OnJobsComplete += () => method.Invoke(system, default);
                }
            }
        }

        public IEnumerator<KeyValuePair<SolverAction, IReadOnlyList<(MethodInfo, Type)>>> GetEnumerator() => actionsOrder.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => (this as IEnumerable<KeyValuePair<SolverAction, IReadOnlyList<(MethodInfo, Type)>>>).GetEnumerator();
    }
}