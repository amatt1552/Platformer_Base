#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ArrayLimitAttribute))]
public class ArrayLimitDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ArrayLimitAttribute limit = (ArrayLimitAttribute)attribute;

        if (property.isArray && property.arraySize > limit.MaxSize)
        {
            property.arraySize = limit.MaxSize; // Enforce max limit in real-time
        }

        EditorGUI.PropertyField(position, property, label, true);
    }
}
#endif
