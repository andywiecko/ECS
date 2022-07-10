#if UNITY_EDITOR
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;

namespace andywiecko.ECS.Editor
{
    public static class UnityEditorExtensions
    {
        public static string GetName(this AssemblyDefinitionAsset asset) => JObject.Parse(asset.text)["name"].ToString();
        public static IEnumerable<string> GetNames(this IEnumerable<AssemblyDefinitionAsset> assets) => assets
            .Where(i => i != null)
            .Select(i => i.GetName());
    }
}
#endif
