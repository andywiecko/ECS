using UnityEditor;
using UnityEngine;

namespace andywiecko.ECS.Editor
{
    [CustomPropertyDrawer(typeof(SerializedMethod))]
    public class SerializedMethodDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var methodName = property.FindPropertyRelative("<MethodName>k__BackingField");
            var serializedType = property.FindPropertyRelative("<SerializedType>k__BackingField");

            var width = 0.5f * position.width;
            var height = position.height;

            EditorGUI.LabelField(new(x: position.x, y: position.y, width, height), methodName.stringValue.ToNonPascal());
            EditorGUI.PropertyField(new(x: position.x + width, y: position.y, width, height), serializedType);

            property.serializedObject.ApplyModifiedProperties();
        }

        // TODO: UIElements
    }
}