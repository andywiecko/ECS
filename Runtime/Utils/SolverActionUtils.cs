using andywiecko.ECS.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace andywiecko.ECS
{
    public static class SolverActionUtils
    {
        public static readonly IReadOnlyDictionary<MethodInfo, Type> MethodToType;
        public static readonly IReadOnlyDictionary<Type, string> TypeToGuid;
        public static readonly IReadOnlyDictionary<string, Type> GuidToType;

        static SolverActionUtils()
        {
            var methods = TypeCache
                .GetMethodsWithAttribute<SolverActionAttribute>()
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

            void RegisterMapping(Type type, string guid)
            {
                typeToGuid.Add(type, guid);
                guidToType.Add(guid, type);
            }

            foreach (var type in types)
            {
                var guid = AssetDatabaseUtils.TryGetTypeGUID(type);

                if (guid is string)
                {
                    RegisterMapping(type, guid);
                }
                else
                {
                    throw new NotImplementedException("This Type-GUID case is not handled yet.");
                }
            }

            MethodToType = methodToType;
            TypeToGuid = typeToGuid;
            GuidToType = guidToType;
        }
    }
}