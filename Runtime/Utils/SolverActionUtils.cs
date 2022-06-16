using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace andywiecko.ECS.Editor
{
    public static class SolverActionUtils
    {
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

            var methods = AssemblyToMethods.Values
                .SelectMany(i => i)
                .ToArray();

            var methodToType = new Dictionary<MethodInfo, Type>();
            foreach (var m in methods)
            {
                var t = m.ReflectedType;
                methodToType.Add(m, t);
            }

            var typeToGuid = new Dictionary<Type, string>();
            var guidToType = new Dictionary<string, Type>();
            var types = methodToType.Values.Distinct();

            foreach (var type in types)
            {
                var guid = AssetDatabaseUtils.TryGetTypeGUID(type);

                if (guid is string)
                {
                    RegisterMapping(type, guid);
                }
                else
                {
                    throw new NotImplementedException($"This Type-GUID case is not handled yet ({type}).");
                }
            }

            MethodToType = methodToType;
            TypeToGuid = typeToGuid;
            GuidToType = guidToType;

            void RegisterMapping(Type type, string guid)
            {
                typeToGuid.Add(type, guid);
                guidToType.Add(guid, type);
            }
        }
    }
}