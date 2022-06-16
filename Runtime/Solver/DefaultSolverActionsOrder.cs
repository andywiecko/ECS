using andywiecko.ECS.Editor;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace andywiecko.ECS
{
    [Serializable]
    public class SerializedMethod
    {
        [HideInInspector, SerializeField]
        private string tag = "";

        [field: HideInInspector, SerializeField]
        public string Name { get; private set; } = "";

        [field: HideInInspector, SerializeField]
        public SerializedType SerializedType { get; private set; } = default;

#if UNITY_EDITOR
        [SerializeField]
        private MonoScript script = default;
#endif

        public (MethodInfo MethodInfo, Type Type) Value => (SolverActionUtils.GuidToType[SerializedType.Guid].GetMethod(Name), SolverActionUtils.GuidToType[SerializedType.Guid]);

        public void Deconstruct(out MethodInfo m, out Type t) => (m, t) = Value;

        public SerializedMethod(MethodInfo methodInfo, Type type, string guid)
        {
            Name = methodInfo.Name;
            tag = Name.ToNonPascal() + $" ({type.Name.ToNonPascal()})";
            this.SerializedType = new(type, guid);
            Validate(type);
        }

        public void Validate(Type type)
        {
            //this.SerializedType.Validate(type);
#if UNITY_EDITOR
            var path = AssetDatabase.GUIDToAssetPath(SerializedType.Guid);
            script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
#endif 
        }
    }

    [Serializable]
    public class UnconfiguredMethod
    {
        [HideInInspector, SerializeField]
        private string tag = "";

        [field: HideInInspector, SerializeField]
        public SerializedMethod Method { get; private set; }

        [field: SerializeField]
        public SolverAction Action { get; private set; } = SolverAction.Undefined;

        public UnconfiguredMethod(SerializedMethod method)
        {
            var (m, t) = method.Value;
            tag = m.Name.ToNonPascal() + $" ({t.Name.ToNonPascal()})";
            Method = method;
        }

        public UnconfiguredMethod(MethodInfo method, Type type, string guid) : this(new(method, type, guid)) { }
    }

    [CreateAssetMenu(fileName = "DefaultSolverActionsOrder", menuName = "ECS/Solver/Default Solver Actions Order")]
    public class DefaultSolverActionsOrder : SolverActionsOrder
    {
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

        private readonly Dictionary<SolverAction, List<(MethodInfo, Type)>> actionOrder = new();

        public void RegenerateActionOrder()
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
        [SerializeField]
        private UnityEditorInternal.AssemblyDefinitionAsset[] targetAssemblies = { };
#endif

        [Space(50)]
        [SerializeField] private List<SerializedMethod> onScheduling = new();
        [SerializeField] private List<SerializedMethod> onJobsCompletion = new();

        [Space(30)]
        [SerializeField]
        private List<UnconfiguredMethod> undefinedMethods = new();

        private void Awake() => ValidateMethods();

        private void OnValidate()
        {
            TargetAssemblies = targetAssemblies?
                .Where(i => i != null)
                .Select(i => JObject.Parse(i.text)["name"].ToString()).ToArray();

            ValidateMethods();
        }

        private void ValidateMethods()
        {
            // HACK:
            //   For unknown reason static dicts don't survive when saving asset,
            //   but OnValidate is called during save.
            if (SolverActionUtils.GuidToType.Count == 0)
            {
                return;
            }

            var methodsToAssign = undefinedMethods.Where(t => t.Action != SolverAction.Undefined);
            foreach (var u in methodsToAssign)
            {
                GetListAtAction(u.Action).Add(u.Method);
            }

            foreach (var action in SystemExtensions.GetValues<SolverAction>().Except(new[] { SolverAction.Undefined }))
            {
                var list = GetListAtAction(action)
                    .DistinctBy(i => (i.Name, i.SerializedType.Guid))
                    .Where(i => i is not null)
                    .ToList();

                list.RemoveAll(i => !SolverActionUtils.GuidToType.ContainsKey(i.SerializedType.Guid));

                SetListAtAction(action, list);
            }

            undefinedMethods.Clear();
            var serializedMethods = GetSerializedMethods();

            var methodsToTypes = TargetAssemblies
                .Select(i => Assembly.Load(i))
                .SelectMany(i => SolverActionUtils.AssemblyToMethods[i])
                .Select(i => (methodInfo: i, type: SolverActionUtils.MethodToType[i]));

            foreach (var (m, t) in methodsToTypes)
            {
                if (!serializedMethods.Contains((m, t)))
                {
                    undefinedMethods.Add(new(m, t, SolverActionUtils.TypeToGuid[t]));
                }
            }

            foreach (var action in SystemExtensions.GetValues<SolverAction>())
            {
                var list = GetListAtAction(action);
                if (list is not null)
                {
                    foreach (var l in list)
                    {
                        l.Validate(l.Value.Type);
                    }
                }
            }
        }

        public override void GenerateActions(Solver solver, World world)
        {
            solver.ResetActions();
            RegenerateActionOrder();

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
    }
}