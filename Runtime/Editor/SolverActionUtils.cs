using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace andywiecko.ECS.Editor
{
    public static class SolverActionUtils
    {
        public static readonly IReadOnlyList<MethodInfo> Methods;
        public static readonly IReadOnlyDictionary<MethodInfo, Type> MethodToType;
        public static readonly IReadOnlyDictionary<Type, string> TypeToGuid;
        public static readonly IReadOnlyDictionary<string, Type> GuidToType;
        public static readonly IReadOnlyDictionary<Assembly, IReadOnlyList<MethodInfo>> AssemblyToMethods;

        static SolverActionUtils()
        {
            Methods = TypeCache
                .GetMethodsWithAttribute<SolverActionAttribute>()
                .ToArray();

            MethodToType = Methods.ToDictionary(i => i, i => i.ReflectedType);

            AssemblyToMethods = Methods
                .GroupBy(i => i.ReflectedType.Assembly)
                .ToDictionary(i => i.Key, i => i.ToArray() as IReadOnlyList<MethodInfo>);

            TypeToGuid = MethodToType.Values
                .Distinct()
                .Select(i => (type: i, guid: GuidUtils.TypeToGuid.TryGetValue(i, out var g) ? g : default))
                .Where(i => i.guid != null) // TODO: add warning here when guid is not found?
                .ToDictionary(i => i.type, i => i.guid);

            GuidToType = TypeToGuid.ToDictionary(i => i.Value, i => i.Key);
        }
    }
}
