using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace andywiecko.ECS.Editor
{
    public static class TypeCacheUtils
    {
        #region Guid
        public static class Guid
        {
            public static readonly IReadOnlyDictionary<Type, string> TypeToGuid;
            public static readonly IReadOnlyDictionary<string, Type> GuidToType;

            static Guid()
            {
                GuidToType = AssetDatabase.FindAssets($"t:script")
                    .Select(guid =>
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                        var type = monoScript.GetClass();
                        return (guid, type);
                    })
                    .Where(i => i.type != null)
                    .ToDictionary(i => i.guid, i => i.type);

                TypeToGuid = GuidToType.ToDictionary(i => i.Value, i => i.Key);
            }
        }
        #endregion

        #region Solver Actions
        public static class SolverActions
        {
            public static readonly IReadOnlyList<MethodInfo> Methods;
            public static readonly IReadOnlyDictionary<MethodInfo, Type> MethodToType;
            public static readonly IReadOnlyDictionary<Type, string> TypeToGuid;
            public static readonly IReadOnlyDictionary<string, Type> GuidToType;
            public static readonly IReadOnlyDictionary<Assembly, IReadOnlyList<MethodInfo>> AssemblyToMethods;

            static SolverActions()
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
                    .Select(i => (type: i, guid: TypeCacheUtils.Guid.TypeToGuid.TryGetValue(i, out var g) ? g : default))
                    .Where(i => i.guid != null) // TODO: add warning here when guid is not found?
                    .ToDictionary(i => i.type, i => i.guid);

                GuidToType = TypeToGuid.ToDictionary(i => i.Value, i => i.Key);
            }
        }
        #endregion

        #region Systems
        public static class Systems
        {
            public static readonly IReadOnlyList<Type> Types;
            public static readonly IReadOnlyDictionary<Type, string> TypeToGuid;
            public static readonly IReadOnlyDictionary<string, Type> GuidToType;
            public static readonly IReadOnlyDictionary<Assembly, IReadOnlyList<Type>> AssemblyToTypes;
            public static readonly IReadOnlyDictionary<Type, CategoryAttribute> TypeToCategory;

            static Systems()
            {
                Types = TypeCache
                    .GetTypesDerivedFrom<ISystem>()
                    .Where(i => !i.IsAbstract)
                    .ToArray();

                AssemblyToTypes = Types
                    .GroupBy(i => i.Assembly)
                    .ToDictionary(i => i.Key, i => i.ToArray() as IReadOnlyList<Type>);

                TypeToCategory = Types
                    .Select(i => (type: i, category: i
                        .GetCustomAttribute<CategoryAttribute>() ?? new CategoryAttribute("Others")))
                    .ToDictionary(i => i.type, i => i.category);

                TypeToGuid = Types
                    .Select(i => (type: i, guid: Guid.TypeToGuid.TryGetValue(i, out var g) ? g : default))
                    .Where(i => i.guid != null) // TODO: add warning here when guid is not found?
                    .ToDictionary(i => i.type, i => i.guid);

                GuidToType = TypeToGuid.ToDictionary(i => i.Value, i => i.Key);
            }
        }
        #endregion
    }
}
