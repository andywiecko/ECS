#if UNITY_EDITOR
using andywiecko.ECS.Editor;
#endif
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace andywiecko.ECS
{
    public class World : MonoBehaviour, IWorld
    {
        public IdCounter<IEntity> EntitiesCounter { get; } = new();
        public ConfigurationsRegistry ConfigurationsRegistry { get; } = new();
        public ComponentsRegistry ComponentsRegistry => componentsRegistry ??= new(TargetAssemblies.Select(i => Assembly.Load(i)));
        private ComponentsRegistry componentsRegistry;
        public SystemsRegistry SystemsRegistry { get; } = new();

        [field: SerializeField, HideInInspector]
        public string[] TargetAssemblies { get; private set; } = { };

#if UNITY_EDITOR
        [SerializeField]
        private UnityEditorInternal.AssemblyDefinitionAsset[] targetAssemblies = { };
#endif

        public void Clear()
        {
            ConfigurationsRegistry.Clear();
            ComponentsRegistry.Clear();
            SystemsRegistry.Clear();
        }

#if UNITY_EDITOR
        private void OnValidate() => TargetAssemblies = targetAssemblies?.GetNames().ToArray();
#endif
    }
}