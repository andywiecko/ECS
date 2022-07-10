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
            [field: SerializeField]
            public SerializedMethod Method { get; private set; }

            [field: SerializeField]
            public SolverAction Action { get; private set; } = SolverAction.Undefined;

            public UnconfiguredMethod(MethodInfo method, string guid) => Method = new(method, guid);
        }

        private List<SerializedMethod> GetListAtAction(SolverAction action) => action switch
        {
            SolverAction.OnScheduling => onScheduling,
            SolverAction.OnJobsCompletion => onJobsCompletion,
            _ => default,
        };

        private void SetListAtAction(SolverAction action, List<SerializedMethod> list)
        {
            switch (action)
            {
                case SolverAction.OnScheduling:
                    onScheduling = list;
                    break;

                case SolverAction.OnJobsCompletion:
                    onJobsCompletion = list;
                    break;
            }
        }

        private readonly Dictionary<SolverAction, IReadOnlyList<(MethodInfo, Type)>> actionOrder = new();

        private void RegenerateActionsOrder()
        {
            actionOrder.Clear();
            foreach (var a in SystemExtensions.GetValues<SolverAction>())
            {
                var list = GetListAtAction(a);
                if (list is null) continue;
                var methods = new List<(MethodInfo, Type)>(capacity: list.Count);
                foreach (var (m, t) in list)
                {
                    methods.Add((m, t));
                }
                actionOrder.Add(a, methods);
            }
        }

        private List<(MethodInfo, Type)> GetSerializedMethods() => new[] { onScheduling, onJobsCompletion }
            .SelectMany(i => i)
            .Select(i => i.Value)
            .ToList();

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
            var methodsToAssign = undefinedMethods.Where(t => t.Action != SolverAction.Undefined);
            foreach (var u in methodsToAssign)
            {
                GetListAtAction(u.Action).Add(u.Method);
            }

            foreach (var action in SystemExtensions.GetValues<SolverAction>().Except(new[] { SolverAction.Undefined }))
            {
                var list = GetListAtAction(action)
                    .DistinctBy(i => (i.MethodName, i.SerializedType.Guid))
                    .Where(i => i is not null)
                    .ToList();

                list.RemoveAll(i => !TypeCacheUtils.SolverActions.GuidToType.ContainsKey(i.SerializedType.Guid));

                SetListAtAction(action, list);
            }

            undefinedMethods.Clear();
            var serializedMethods = GetSerializedMethods();

            foreach (var (m, t) in TargetTypes)
            {
                if (!serializedMethods.Contains((m, t)))
                {
                    undefinedMethods.Add(new(m, TypeCacheUtils.SolverActions.TypeToGuid[t]));
                }
            }

            foreach (var action in SystemExtensions.GetValues<SolverAction>())
            {
                var list = GetListAtAction(action);
                if (list is not null)
                {
                    foreach (var l in list)
                    {
                        l.Validate(TypeCacheUtils.Systems.GuidToType[l.SerializedType.Guid]);
                    }
                }
            }
        }
#endif

        public override void GenerateActions(ISolver solver, IWorld world)
        {
            RegenerateActionsOrder();

            foreach (var (method, type) in actionOrder[SolverAction.OnScheduling])
            {
                if (world.SystemsRegistry.TryGetSystem(type, out var system))
                {
                    solver.OnScheduling += () => method.Invoke(system, default);
                }
            }

            foreach (var (method, type) in actionOrder[SolverAction.OnJobsCompletion])
            {
                if (world.SystemsRegistry.TryGetSystem(type, out var system))
                {
                    solver.OnJobsComplete += () => method.Invoke(system, default);
                }
            }
        }

        public IEnumerator<KeyValuePair<SolverAction, IReadOnlyList<(MethodInfo, Type)>>> GetEnumerator() => actionOrder.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => (this as IEnumerable<KeyValuePair<SolverAction, IReadOnlyList<(MethodInfo, Type)>>>).GetEnumerator();
    }
}