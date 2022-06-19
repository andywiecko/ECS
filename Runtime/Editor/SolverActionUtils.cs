using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
            AssemblyToMethods = AppDomain.CurrentDomain.GetAssemblies()
                .Select(i => (assembly: i, methods: i
                    .GetTypes()
                    .SelectMany(t => t.GetMethods())
                    .Where(m => m.GetCustomAttributes<SolverActionAttribute>().Count() > 0)
                    .ToArray() as IReadOnlyList<MethodInfo>)
                ).ToDictionary(i => i.assembly, i => i.methods);

            Methods = AssemblyToMethods.Values
                .SelectMany(i => i)
                .ToArray();

            MethodToType = Methods.ToDictionary(i => i, i => i.ReflectedType);

            TypeToGuid = MethodToType.Values
                .Distinct()
                .Select(i => (type: i, guid: GuidUtils.TypeToGuid.TryGetValue(i, out var g) ? g : default))
                .Where(i => i.guid != null) // TODO: add warning here when guid is not found?
                .ToDictionary(i => i.type, i => i.guid);

            GuidToType = TypeToGuid.ToDictionary(i => i.Value, i => i.Key);
        }
    }
}
