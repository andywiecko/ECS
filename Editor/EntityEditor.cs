using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace andywiecko.ECS.Editor
{
    [CustomEditor(typeof(Entity), editorForChildClasses: true)]
    public class EntityEditor : UnityEditor.Editor
    {
        private MonoBehaviour Target => target as MonoBehaviour;

        [SerializeField]
        private StyleSheet styleSheet = default;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            if (styleSheet == null) // Default reference value does not propagate with inherited classes.
            {
                Debug.LogWarning($"Missing style sheet reference for {GetType().Name}. Loading the defaults.");
                var path = AssetDatabase.GUIDToAssetPath("9ff5cb1686dd0ce4e84b0b7be1f62d68");
                styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
            }
            root.styleSheets.Add(styleSheet);

            var imgui = new IMGUIContainer(base.OnInspectorGUI) { name = "imgui" };
            root.Add(imgui);

            root.Add(PrefabUtility.IsPartOfPrefabAsset(target) ?
                new HelpBox(
                    "Editing in asset view is not supported.\n" +
                    "Please open prefab in isolation mode.", HelpBoxMessageType.Warning)
                : BuildComponents());

            return root;
        }

        private VisualElement BuildComponents()
        {
            var categories = new Dictionary<string, VisualElement>();
            var components = new VisualElement() { name = "components" };

            var type = target.GetType();
            if (!TypeCacheUtils.Entities.EntityToComponents.TryGetValue(type, out var componentTypes))
            {
                return components;
            }

            foreach (var c in componentTypes.Where(t => !t.IsAbstract))
            {
                var line = new VisualElement { name = "script-with-toggle" };

                var categoryName = TypeCacheUtils.Categories.TypeToCategory.TryGetValue(c, out var attribute) ? attribute.Name : "Others";

                var toggle = CreateToggleButtonForType(c);
                line.Add(toggle);

                var guid = TypeCacheUtils.Guid.TypeToGuid[c];
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var scriptField = new ObjectField
                {
                    value = AssetDatabase.LoadAssetAtPath<MonoScript>(path),
                    objectType = typeof(MonoScript),
                };
                scriptField.Q(className: ObjectField.selectorUssClassName).SetEnabled(false);
                scriptField.tooltip = TypeCacheUtils.Tooltips.TypeToTooltip.TryGetValue(c, out var tooltip) ? tooltip.tooltip : default;
                line.Add(scriptField);

                if (!categories.TryGetValue(categoryName, out var category))
                {
                    categories[categoryName] = category = new Foldout { name = "category", text = categoryName };
                }
                category.Add(line);
            }

            foreach (var c in categories
                .OrderBy(i => i.Key)
                .Select(i => i.Value))
            {
                components.Add(c);
            }

            return components;
        }

        protected VisualElement CreateToggleButtonForType(Type type)
        {
            var value = Target.TryGetComponent(type, out _);
            var toggle = new Toggle { value = value };

            toggle.RegisterValueChangedCallback((evt) =>
            {
                switch (evt.newValue)
                {
                    case true:
                        if (!TryAddComponent(type))
                        {
                            toggle.value = false;
                        }
                        return;

                    case false:
                        TryRemoveComponent(type);
                        return;
                }
            });

            return toggle;
        }

        private bool TryAddComponent(Type type)
        {
            if (Target.TryGetComponent(type, out _))
            {
                return false;
            }

            if (PrefabUtility.IsPartOfPrefabInstance(target))
            {
                var removedComponent = PrefabUtility
                    .GetRemovedComponents(Target.gameObject)
                    .FirstOrDefault(c => c.assetComponent.GetType() == type);

                if (removedComponent != null)
                {
                    removedComponent.Revert();
                    return true;
                }
            }

            if (Undo.AddComponent(Target.gameObject, type) == null)
            {
                return false;
            }

            EditorUtility.SetDirty(target);
            return true;
        }

        private void TryRemoveComponent(Type type)
        {
            if (Target.TryGetComponent(type, out var component))
            {
                Undo.DestroyObjectImmediate(component);
            }
        }
    }
}