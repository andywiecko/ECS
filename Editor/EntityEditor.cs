using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace andywiecko.ECS.Editor
{
    [CustomEditor(typeof(Entity), editorForChildClasses: true)]
    public class EntityEditor : UnityEditor.Editor
    {
        private MonoBehaviour Target => target as MonoBehaviour;
        private PrefabStage prefabStage;
        private (bool isPrefabInstance, bool isPrefabAsset, bool isStage) targetStatus;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var imgui = new IMGUIContainer(base.OnInspectorGUI) { name = "imgui" };
            root.Add(imgui);

            root.Add(targetStatus.isPrefabAsset ?
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

            foreach (var c in componentTypes)
            {
                var line = new VisualElement()
                {
                    name = "Line",
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Normal)
                    }
                };

                var categoryName = TypeCacheUtils.Categories.TypeToCategory.TryGetValue(c, out var attribute) ? attribute.Name : "Others";

                var toggle = CreateToggleButtonForType(c);
                line.Add(toggle);

                var guid = TypeCacheUtils.Guid.TypeToGuid[c];
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var scriptField = new ObjectField()
                {
                    value = AssetDatabase.LoadAssetAtPath<MonoScript>(path),
                    objectType = typeof(MonoScript),
                };
                scriptField.Q(className: ObjectField.selectorUssClassName).SetEnabled(false);
                scriptField.Q<Label>().text = scriptField.Q<Label>().text.ToNonPascal();
                scriptField.tooltip = TypeCacheUtils.Tooltips.TypeToTooltip.TryGetValue(c, out var tooltip) ? tooltip.tooltip : default;
                line.Add(scriptField);

                if (!categories.TryGetValue(categoryName, out var category))
                {
                    categories[categoryName] = category = new Foldout()
                    {
                        text = categoryName,
                        style = {
                            color = new StyleColor(new Color(.678f, .847f, .902f)) ,
                            unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                        }
                    };
                }
                category.Add(line);
            }

            foreach (var c in categories.Values)
            {
                components.Add(c);
            }

            return components;
        }

        protected VisualElement CreateToggleButtonForType(Type type)
        {
            var value = Target.GetComponent(type) != null;
            var toggle = new Toggle()
            {
                value = value,
                style = {
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Normal),
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                }
            };

            toggle.RegisterValueChangedCallback((evt) =>
            {
                switch (targetStatus)
                {
                    case (false, false, false):
                        NonPrefabInstanceCase(evt.newValue, type);
                        break;

                    case (true, false, false):
                        PrefabInstanceCase(evt.newValue, type);
                        break;

                    case (false, false, true):
                        PrefabAssetIsolationStageCase(evt.newValue, type);
                        break;

                    case (false, true, false):
                        // TODO: PrefabAssetNonIsolationStageCase
                        break;

                    default:
                        throw new NotImplementedException();
                }
            });
            return toggle;
        }

        private void PrefabAssetIsolationStageCase(bool newValue, Type type)
        {
            switch (newValue)
            {
                case true:
                    Undo.AddComponent(Target.gameObject, type);
                    break;

                case false:
                    Undo.DestroyObjectImmediate(Target.GetComponent(type));
                    break;
            }

            EditorUtility.SetDirty(target);
        }

        private void PrefabInstanceCase(bool value, Type type)
        {
            switch (value)
            {
                case true:
                    var removedComponent = PrefabUtility
                        .GetRemovedComponents(Target.gameObject)
                        .FirstOrDefault(c => c.assetComponent.GetType() == type);

                    if (removedComponent != null)
                    {
                        removedComponent.Revert();
                    }
                    else
                    {
                        Undo.AddComponent(Target.gameObject, type);
                    }

                    break;

                case false:
                    Undo.DestroyObjectImmediate(Target.GetComponent(type));
                    break;
            }
        }

        private void NonPrefabInstanceCase(bool newValue, Type type)
        {
            switch (newValue)
            {
                case true:
                    Undo.AddComponent(Target.gameObject, type);
                    break;

                case false:
                    Undo.DestroyObjectImmediate(Target.GetComponent(type));
                    break;
            }
        }

        private void OnEnable()
        {
            RefreshTargetStatus();
        }

        private void OnValidate()
        {
            RefreshTargetStatus();
        }

        private void RefreshTargetStatus()
        {
            targetStatus.isPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(target);
            targetStatus.isPrefabAsset = PrefabUtility.IsPartOfPrefabAsset(target);
            prefabStage = PrefabStageUtility.GetPrefabStage(Target.gameObject);
            targetStatus.isStage = prefabStage != null;
        }
    }
}