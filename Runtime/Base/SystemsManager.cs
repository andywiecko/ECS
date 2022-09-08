#if UNITY_EDITOR
using andywiecko.ECS.Editor;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace andywiecko.ECS
{
    public class SystemsManager : MonoBehaviour
    {
        [Serializable]
        private class SerializedTypeBoolTuple
        {
            public SerializedType Type = default;
            public bool Value = true;
            public void Deconstruct(out SerializedType type, out bool value) => (type, value) = (Type, Value);
        }

        [field: SerializeField, HideInInspector]
        public World World { get; private set; } = default;

#if UNITY_EDITOR
        private IEnumerable<Type> TargetTypes => TypeCacheUtils.Systems.GetTypes(World.TargetAssemblies);
#endif

        [SerializeField, HideInInspector]
        private List<SerializedTypeBoolTuple> serializedSystems = new();

        private readonly Dictionary<Type, ISystem> systems = new();
        private readonly Dictionary<Type, int> systemToTupleId = new();

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

            serializedSystems[systemToTupleId[type]].Value = value;
        }

        private void Awake()
        {
            systems.Clear();
            systemToTupleId.Clear();

            var i = 0;
            foreach (var tuple in serializedSystems)
            {
                var t = tuple.Type.Type;
                var s = Activator.CreateInstance(t) as BaseSystem;
                s.World = World;
                systems.Add(t, s);
                systemToTupleId.Add(t, i);

                i++;
            }
        }

        private void Start()
        {
            foreach (var (t, _) in systems)
            {
                var id = systemToTupleId[t];
                var tuple = serializedSystems[id];
                SetSystemActive(t, tuple.Value);
            }
        }

#if UNITY_EDITOR
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
                serializedSystems.Clear();
                return;
            }

            serializedSystems.RemoveAll(i => !TargetTypes.Contains(i.Type.Type));

            foreach (var t in TargetTypes.Except(serializedSystems.Select(i => i.Type.Type)))
            {
                if (TypeCacheUtils.Systems.TypeToGuid.TryGetValue(t, out var guid))
                {
                    serializedSystems.Add(new() { Type = new(t, guid) });
                }
            }

            foreach (var (type, _) in serializedSystems)
            {
                type.Validate(TypeCacheUtils.Systems.GuidToType[type.Guid]);
            }
        }
#endif
    }
}
