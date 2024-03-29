#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace andywiecko.ECS.Editor
{
    public static class TypeCacheUtils
    {
        #region Categories
        public static class Categories
        {
            public static readonly IReadOnlyDictionary<Type, CategoryAttribute> TypeToCategory;

            static Categories()
            {
                TypeToCategory = TypeCache
                    .GetTypesWithAttribute<CategoryAttribute>()
                    .ToDictionary(i => i, i => i.GetCustomAttribute<CategoryAttribute>());
            }
        }
        #endregion

        #region Components
        public static class Components
        {
            public static readonly IReadOnlyList<Type> Types;
            public static readonly IReadOnlyDictionary<Type, Type> ComponentToEntity;

            static Components()
            {
                Types = TypeCache
                    .GetTypesDerivedFrom<BaseComponent>()
                    .ToArray();

                var componentToEntities = TypeCache
                    .GetTypesWithAttribute<RequireComponent>()
                    .Where(i => i.IsSubclassOf(typeof(BaseComponent)))
                    .Select(i => (component: i, entity: i
                        .GetCustomAttributes<RequireComponent>()
                        .SelectMany(i => new[] { i.m_Type0, i.m_Type1, i.m_Type2 })
                        .Where(i => i != null)
                        .Where(i => i.IsSubclassOf(typeof(Entity)))
                        .Distinct())
                    ).Where(i => i.entity.Any());

                foreach (var (c, e) in componentToEntities.Where(i => i.entity.Count() > 1))
                {
                    Debug.LogError(
                        $"{c}: {nameof(BaseComponent)} cannot require more than one entity type!" +
                        $"Please remove one of the {typeof(RequireComponent)} attribute.");
                }

                ComponentToEntity = componentToEntities.ToDictionary(i => i.component, i => i.entity.First());
            }
        }
        #endregion

        #region Entities
        public static class Entities
        {
            public static readonly IReadOnlyList<Type> Types;
            public static readonly IReadOnlyDictionary<Type, IReadOnlyList<Type>> EntityToComponents;

            static Entities()
            {
                Types = TypeCache.GetTypesDerivedFrom<Entity>().ToArray();

                EntityToComponents = Components.ComponentToEntity
                    .Select(i => (entity: i.Value, component: i.Key))
                    .GroupBy(i => i.entity)
                    .ToDictionary(i => i.Key, i => i
                        .Select(j => j.component).ToList() as IReadOnlyList<Type>);
            }
        }
        #endregion

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
                    .Select(i => (type: i, guid: Guid.TypeToGuid.TryGetValue(i, out var g) ? g : default))
                    .Where(i => i.guid != null)
                    .ToDictionary(i => i.type, i => i.guid);

                GuidToType = TypeToGuid.ToDictionary(i => i.Value, i => i.Key);
            }

            public static IEnumerable<(MethodInfo methodInfo, Type type)> GetMethods(IEnumerable<string> targetAssemblies) => targetAssemblies
                .Select(i => Assembly.Load(i))
                .Where(i => AssemblyToMethods.ContainsKey(i))
                .SelectMany(i => AssemblyToMethods[i])
                .Select(i => (methodInfo: i, type: MethodToType[i]));
        }
        #endregion

        #region Systems
        public static class Systems
        {
            public static readonly IReadOnlyList<Type> Types;
            public static readonly IReadOnlyDictionary<Type, string> TypeToGuid;
            public static readonly IReadOnlyDictionary<string, Type> GuidToType;
            public static readonly IReadOnlyDictionary<Assembly, IReadOnlyList<Type>> AssemblyToTypes;

            static Systems()
            {
                Types = TypeCache
                    .GetTypesDerivedFrom<BaseSystem>()
                    .Where(i => !i.IsAbstract)
                    .ToArray();

                AssemblyToTypes = Types
                    .GroupBy(i => i.Assembly)
                    .ToDictionary(i => i.Key, i => i.ToArray() as IReadOnlyList<Type>);

                TypeToGuid = Types
                    .Select(i => (type: i, guid: Guid.TypeToGuid.TryGetValue(i, out var g) ? g : default))
                    .Where(i => i.guid != null)
                    .ToDictionary(i => i.type, i => i.guid);

                GuidToType = TypeToGuid.ToDictionary(i => i.Value, i => i.Key);
            }

            public static IEnumerable<Type> GetTypes(IEnumerable<string> targetAssemblies) => targetAssemblies
                .Select(i => Assembly.Load(i))
                .Where(i => AssemblyToTypes.ContainsKey(i))
                .SelectMany(i => AssemblyToTypes[i]);
        }
        #endregion

        #region Tooltips
        public static class Tooltips
        {
            public static readonly IReadOnlyDictionary<Type, TooltipAttribute> TypeToTooltip;

            static Tooltips()
            {
                TypeToTooltip = TypeCache
                    .GetTypesWithAttribute<TooltipAttribute>()
                    .ToDictionary(i => i, i => i.GetCustomAttribute<TooltipAttribute>());
            }
        }
        #endregion
    }
}
#endif
