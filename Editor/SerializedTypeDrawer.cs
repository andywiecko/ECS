using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace andywiecko.ECS.Editor
{
    [CustomPropertyDrawer(typeof(SerializedType))]
    public class SerializedTypeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var guid = property.FindPropertyRelative("<Guid>k__BackingField");
            var assemblyQualifiedName = property.FindPropertyRelative("<AssemblyQualifiedName>k__BackingField");
            var path = AssetDatabase.GUIDToAssetPath(guid.stringValue);
            var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

            var obj = EditorGUI.ObjectField(position, monoScript, typeof(MonoScript), allowSceneObjects: false);
            if (obj != null)
            {
                var script = obj as MonoScript;
                var type = script.GetClass();
                guid.stringValue = TypeCacheUtils.Guid.TypeToGuid[type];
                assemblyQualifiedName.stringValue = type.AssemblyQualifiedName;
            }

            property.serializedObject.ApplyModifiedProperties();
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();

            var guid = property.FindPropertyRelative("<Guid>k__BackingField");
            var assemblyQualifiedName = property.FindPropertyRelative("<AssemblyQualifiedName>k__BackingField");
            var path = AssetDatabase.GUIDToAssetPath(guid.stringValue);
            var scriptField = new ObjectField()
            {
                value = AssetDatabase.LoadAssetAtPath<MonoScript>(path),
                objectType = typeof(MonoScript),
            };
            scriptField.Q(className: ObjectField.selectorUssClassName).SetEnabled(false);

            scriptField.RegisterValueChangedCallback(evt =>
            {
                var monoScript = evt.newValue as MonoScript;
                if (monoScript == null)
                {
                    guid.stringValue = default;
                    assemblyQualifiedName.stringValue = default;
                    return;
                }

                var type = monoScript.GetClass();
                var path = AssetDatabase.GetAssetPath(monoScript);
                guid.stringValue = AssetDatabase.AssetPathToGUID(path);
                assemblyQualifiedName.stringValue = type.AssemblyQualifiedName;
                property.serializedObject.ApplyModifiedProperties();
            });

            root.Add(scriptField);

            return root;
        }
    }
}