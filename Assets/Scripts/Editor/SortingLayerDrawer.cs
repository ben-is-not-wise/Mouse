using UnityEditor;
using UnityEngine;

namespace HackedDesign.Editor
{
    [CustomPropertyDrawer(typeof(SortingLayerAttribute))]
    public class SortingLayerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            var layers = SortingLayer.layers;
            var names = new string[layers.Length];
            int selected = -1;
            for (int i = 0; i < layers.Length; i++)
            {
                names[i] = layers[i].name;
                if (names[i] == property.stringValue)
                {
                    selected = i;
                }
            }

            if (selected < 0)
            {
                selected = 0;
            }

            EditorGUI.BeginProperty(position, label, property);
            int result = EditorGUI.Popup(position, label.text, selected, names);
            if (result >= 0 && result < names.Length)
            {
                property.stringValue = names[result];
            }
            EditorGUI.EndProperty();
        }
    }
}
