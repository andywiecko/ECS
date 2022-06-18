#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace andywiecko.ECS.Editor
{
    [CustomEditor(typeof(SystemsManagerSolver))]
    public class SystemsManagerSolverEditor : UnityEditor.Editor
    {
        public SystemsManagerSolver Target => target as SystemsManagerSolver;

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
            var systems = new VisualElement() { name = "Systems" };
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
            foreach (SerializedProperty i in serializedObject.FindProperty("<Systems>k__BackingField"))
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

                var valueProperty = i.FindPropertyRelative("value");
                var valueField = new PropertyField(valueProperty);
                valueField.label = "";
                valueField.Bind(valueProperty.serializedObject);

                var typeProperty = i.FindPropertyRelative("type");
                var typeField = new PropertyField(typeProperty);
                typeField.Bind(typeProperty.serializedObject);

                var assemblyQualifiedName = typeProperty.FindPropertyRelative("<AssemblyQualifiedName>k__BackingField").stringValue;
                var type = Type.GetType(assemblyQualifiedName);
                var categoryName = ISystemUtils.TypeToCategory[type].Name;

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
#endif
