using Newtonsoft.Json.Linq;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace andywiecko.ECS
{
    public class World : MonoBehaviour, IWorld
    {
        //public IdCounter<IEntity> EntityCounter { get; } = new(); // TODO: move to EntitesRegistry
        public ConfigurationsRegistry ConfigurationsRegistry { get; } = new();
        public ComponentsRegistry ComponentsRegistry => componentsRegistry ??= new(assembliesNames.Select(i => Assembly.Load(i)));
        private ComponentsRegistry componentsRegistry;
        public SystemsRegistry SystemsRegistry { get; } = new();

#if UNITY_EDITOR
        [SerializeField]
        private UnityEditorInternal.AssemblyDefinitionAsset[] targetAssemblies = { };
#endif

        [SerializeField, HideInInspector]
        private string[] assembliesNames = { };

        private void OnValidate()
        {
            assembliesNames = targetAssemblies?
                .Where(i => i != null)
                .Select(i => JObject.Parse(i.text)["name"].ToString()).ToArray();
        }
    }
}