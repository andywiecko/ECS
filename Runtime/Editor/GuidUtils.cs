using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace andywiecko.ECS.Editor
{
    public static class GuidUtils
    {
        public static readonly IReadOnlyDictionary<Type, string> TypeToGuid;
        public static readonly IReadOnlyDictionary<string, Type> GuidToType;

        static GuidUtils()
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
}