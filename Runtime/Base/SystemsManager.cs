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
        public class SerializedTypeBoolTuple
        {
            public SerializedType type;
            public bool value = true;
        }

        [field: SerializeField, HideInInspector]
        public World World { get; private set; } = default;

        [field: SerializeField, HideInInspector]
        public List<SerializedTypeBoolTuple> Systems { get; private set; } = new();

        private readonly Dictionary<Type, ISystem> systems = new();
        private readonly Dictionary<Type, int> systemsToBoolTuple = new();

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

            Systems[systemsToBoolTuple[type]].value = value;
        }

        private void Awake()
        {
            var i = 0;
            foreach (var tuple in Systems)
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
                var tuple = Systems[systemsToBoolTuple[t]];
                SetSystemActive(t, tuple.value);
            }
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                if (World == null)
                {
                    Systems.Clear();
                    return;
                }

                var worldAssemblies = World.TargetAssemblies.Select(i => Assembly.Load(i));
                var types = TypeCacheUtils.Systems.AssemblyToTypes
                    .Where(i => worldAssemblies.Contains(i.Key))
                    .SelectMany(i => i.Value);
                Systems.RemoveAll(i => !types.Contains(i.type.Type));
                var serializedSystems = Systems.Select(i => i.type.Type);

                foreach (var t in types.Except(serializedSystems))
                {
                    SerializedTypeBoolTuple tuple = new() { type = new(t, TypeCacheUtils.Systems.TypeToGuid[t]) };
                    Systems.Add(tuple);
                }

                foreach (var t in Systems)
                {
                    var type = t.type;
                    var guid = type.Guid;
                    type.Validate(TypeCacheUtils.Systems.GuidToType[guid]);
                }
            }
        }
    }
}