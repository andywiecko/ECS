using System;
using System.Reflection;
using UnityEngine;

namespace andywiecko.ECS
{
    [Serializable]
    public class SerializedMethod
    {
        [field: SerializeField]
        public string MethodName { get; private set; }

        [field: SerializeField]
        public SerializedType SerializedType { get; private set; }

        public MethodInfo MethodInfo => Type.GetMethod(MethodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
        public Type Type => SerializedType.Type;
        public string Guid => SerializedType.Guid;
        public (MethodInfo methodInfo, Type type) Value => (MethodInfo, Type);
        public void Deconstruct(out MethodInfo m, out Type t) => (m, t) = Value;

        public SerializedMethod(MethodInfo methodInfo, string guid)
        {
            // TODO: add warning about the limitations

            MethodName = methodInfo.Name;
            SerializedType = new(methodInfo.DeclaringType, guid);
        }

        public void Validate(Type t) => SerializedType.Validate(t);
    }
}
