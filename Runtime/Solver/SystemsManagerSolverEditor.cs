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
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            var categories = new Dictionary<string, VisualElement>();

            root.Add(new IMGUIContainer(base.OnInspectorGUI));

            foreach (SerializedProperty i in serializedObject.FindProperty("<Systems>k__BackingField"))
            {
                var line = new VisualElement()
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Normal)
                    }
                };

                var valueProperty = i.FindPropertyRelative("value");
                var valueField = new PropertyField(valueProperty);
                valueField.label = "";

                var typeProperty = i.FindPropertyRelative("type");
                var typeField = new PropertyField(typeProperty);

                var assemblyQualifiedName = typeProperty.FindPropertyRelative("<AssemblyQualifiedName>k__BackingField").stringValue;
                var type = Type.GetType(assemblyQualifiedName);
                var categoryName = ISystemUtils.TypeToCategory[type].Name;

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
                root.Add(c);
            }

            return root;
        }
    }
}
#endif
