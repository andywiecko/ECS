using andywiecko.ECS.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace andywiecko.ECS
{
    public class SystemsManagerSolver : MonoBehaviour
    {
        [Serializable]
        public class SerializedType2
        {
            [HideInInspector, SerializeField]
            private string tag = "";

            [field: SerializeField, HideInInspector]
            public string Guid { get; private set; } = default;

            [field: SerializeField, HideInInspector]
            public string AssemblyQualifiedName { get; private set; } = default;

            public Type Type => Type.GetType(AssemblyQualifiedName);

            public SerializedType2(Type t, string guid)
            {
                Validate(t);
                Guid = guid;
            }

            public void Validate(Type t)
            {
                tag = t.Name.ToNonPascal();
                AssemblyQualifiedName = t.AssemblyQualifiedName;
            }
        }

        [Serializable]
        public class SerializedTypeBoolTuple
        {
            public SerializedType2 type;
            public bool value = true;
        }

        [field: SerializeField]
        public World World { get; private set; } = default;

        [field: SerializeField, HideInInspector]
        public List<SerializedTypeBoolTuple> Systems { get; private set; } = new();

        private readonly Dictionary<Type, ISystem> systems = new();

        private void Awake()
        {
            foreach (var t in Systems.Select(t => t.type.Type))
            {
                var s = Activator.CreateInstance(t) as BaseSystem;
                s.World = World;
                systems.Add(t, s);
            }
        }

        private void Start()
        {
            foreach (var (t, s) in systems)
            {
                World.SystemsRegistry.Add(s);
            }
        }

        private void OnValidate()
        {
            Systems.Clear();

            if (World == null)
            {
                return;
            }

            var types = World.TargetAssemblies
                .SelectMany(i => ISystemUtils.AssemblyToTypes[Assembly.Load(i)]);

            foreach (var t in types)
            {
                SerializedTypeBoolTuple tuple = new() { type = new(t, ISystemUtils.TypeToGuid[t]) };
                Systems.Add(tuple);
            }
        }
    }
}