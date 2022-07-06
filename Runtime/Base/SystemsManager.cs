using andywiecko.ECS.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace andywiecko.ECS
{
    public class SystemsManager : MonoBehaviour
    {
        [Serializable]
        private class SerializedTypeBoolTuple
        {
            public SerializedType type;
            public bool value = true;
        }

        [field: SerializeField, HideInInspector]
        public World World { get; private set; } = default;

        [SerializeField, HideInInspector]
        private List<SerializedTypeBoolTuple> serializedSystems = new();

        private readonly Dictionary<Type, ISystem> systems = new();
        private readonly Dictionary<Type, int> systemsToBoolTuple = new();

        public void SetSystemActive<T>(bool value) where T : BaseSystem => SetSystemActive(typeof(T), value);

        public void SetSystemActive(Type type, bool value)
        {
            if (World.SystemsRegistry.TryGetSystem(type, out var _) == value)
            {
                return;
            }

            if (value)
            {
                World.SystemsRegistry.Add(systems[type]);
            }
            else
            {
                World.SystemsRegistry.Remove(systems[type]);
            }

            serializedSystems[systemsToBoolTuple[type]].value = value;
        }

        private void Awake()
        {
            systems.Clear();
            systemsToBoolTuple.Clear();

            var i = 0;
            foreach (var tuple in serializedSystems)
            {
                var t = tuple.type.Type;
                var s = Activator.CreateInstance(t) as BaseSystem;
                s.World = World;
                systems.Add(t, s);
                systemsToBoolTuple.Add(t, i);

                i++;
            }
        }

        private void Start()
        {
            foreach (var (t, _) in systems)
            {
                var tuple = serializedSystems[systemsToBoolTuple[t]];
                SetSystemActive(t, tuple.value);
            }
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                // HACK:
                //   For unknown reason static dicts don't survive when saving asset,
                //   but OnValidate is called during save.
                if (TypeCacheUtils.SolverActions.GuidToType.Count != 0)
                {
                    ValidateSystems();
                }
            }
        }

        private void ValidateSystems()
        {
            if (World == null)
            {
                this.serializedSystems.Clear();
                return;
            }

            var worldAssemblies = World.TargetAssemblies.Select(i => Assembly.Load(i));
            var types = TypeCacheUtils.Systems.AssemblyToTypes
                .Where(i => worldAssemblies.Contains(i.Key))
                .SelectMany(i => i.Value);
            this.serializedSystems.RemoveAll(i => !types.Contains(i.type.Type));
            var serializedSystems = this.serializedSystems.Select(i => i.type.Type);

            foreach (var t in types.Except(serializedSystems))
            {
                if (TypeCacheUtils.Systems.TypeToGuid.TryGetValue(t, out var guid))
                {
                    SerializedTypeBoolTuple tuple = new() { type = new(t, guid) };
                    this.serializedSystems.Add(tuple);
                }
            }

            foreach (var t in this.serializedSystems)
            {
                var type = t.type;
                var guid = type.Guid;
                type.Validate(TypeCacheUtils.Systems.GuidToType[guid]);
            }
        }
    }
}
