using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace andywiecko.ECS.Editor
{
    public static class ISystemUtils
    {
        public static readonly IReadOnlyList<Type> Types;
        public static readonly IReadOnlyDictionary<Type, string> TypeToGuid;
        public static readonly IReadOnlyDictionary<string, Type> GuidToType;
        public static readonly IReadOnlyDictionary<Assembly, IReadOnlyList<Type>> AssemblyToTypes;
        public static readonly IReadOnlyDictionary<Type, CategoryAttribute> TypeToCategory;

        static ISystemUtils()
        {
            AssemblyToTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Select(i => (assembly: i, types: i.GetTypes()
                    .Where(i => !i.IsAbstract)
                    .Where(i => typeof(ISystem).IsAssignableFrom(i))
                    .ToArray() as IReadOnlyList<Type>))
                .ToDictionary(i => i.assembly, i => i.types);

            Types = AssemblyToTypes
                .SelectMany(i => i.Value)
                .ToArray();

            TypeToCategory = Types
                .Select(i => (type: i, category: i
                    .GetCustomAttribute<CategoryAttribute>() ?? new CategoryAttribute("Others")))
                .ToDictionary(i => i.type, i => i.category);

            TypeToGuid = Types
                .Select(i => (type: i, guid: GuidUtils.TypeToGuid.TryGetValue(i, out var g) ? g : default))
                .Where(i => i.guid != null) // TODO: add warning here when guid is not found?
                .ToDictionary(i => i.type, i => i.guid);

            GuidToType = TypeToGuid.ToDictionary(i => i.Value, i => i.Key);
        }
    }
}
