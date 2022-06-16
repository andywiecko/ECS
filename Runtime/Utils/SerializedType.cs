using System;
using UnityEngine;

namespace andywiecko.ECS
{
    [Serializable]
    public class SerializedType
    {
        public Type Type => Type.GetType(AssemblyQualifiedName);

        [field: SerializeField, HideInInspector]
        public string Guid { get; private set; } = default;

        [field: SerializeField, HideInInspector]
        public string AssemblyQualifiedName { get; private set; } = default;

        public SerializedType(Type t, string guid)
        {
            Validate(t);
            Guid = guid;
        }

        public void Validate(Type t)
        {
            AssemblyQualifiedName = t.AssemblyQualifiedName;
        }
    }
}