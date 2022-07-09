using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace andywiecko.ECS.Editor
{
    [CustomEditor(typeof(SystemsManager))]
    public class SystemsManagerEditor : UnityEditor.Editor
    {
        public SystemsManager Target => target as SystemsManager;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var imgui = new IMGUIContainer(base.OnInspectorGUI);
            root.Add(imgui);

            var worldProperty = serializedObject.FindProperty("<World>k__BackingField");
            var worldField = new PropertyField(worldProperty);
            worldField.SetEnabled(!Application.isPlaying);
            root.Add(worldField);

            var categories = new Dictionary<string, VisualElement>();
            var systems = new VisualElement() { name = "serializedSystems" };
            RebuildSystems(systems, categories);
            root.Add(systems);

            worldField.RegisterValueChangeCallback(evt =>
            {
                EditorUtility.SetDirty(this);
                serializedObject.ApplyModifiedProperties();
                serializedObject.UpdateIfRequiredOrScript();
                RebuildSystems(systems, categories);
            });

            return root;
        }

        private void RebuildSystems(VisualElement systems, Dictionary<string, VisualElement> categories)
        {
            systems.Clear();
            categories.Clear();
            foreach (SerializedProperty i in serializedObject.FindProperty("serializedSystems"))
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

                var valueProperty = i.FindPropertyRelative("Value");
                var valueField = new PropertyField(valueProperty) { label = "" };
                valueField.Bind(valueProperty.serializedObject);

                var typeProperty = i.FindPropertyRelative("Type");
                var typeField = new PropertyField(typeProperty);
                typeField.Bind(typeProperty.serializedObject);

                var assemblyQualifiedName = typeProperty.FindPropertyRelative("<AssemblyQualifiedName>k__BackingField").stringValue;
                var type = Type.GetType(assemblyQualifiedName);
                var categoryName = TypeCacheUtils.Categories.TypeToCategory.TryGetValue(type, out var attribute) ? attribute.Name : "Others";

                valueField.RegisterCallback<ChangeEvent<bool>>(evt =>
                {
                    if (Application.isPlaying)
                    {
                        Target.SetSystemActive(type, evt.newValue);
                    }
                });

                line.Add(valueField);
                line.Add(typeField);

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
                systems.Add(c);
            }
        }
    }
}
