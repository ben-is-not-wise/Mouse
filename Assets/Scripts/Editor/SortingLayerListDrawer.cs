using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HackedDesign.Editor
{
    [CustomPropertyDrawer(typeof(SortingLayerList))]
    public class SortingLayerListDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var names = property.FindPropertyRelative("names");

            var layers = SortingLayer.layers;
            var layerNames = new string[layers.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                layerNames[i] = layers[i].name;
            }

            int mask = 0;
            for (int i = 0; i < names.arraySize; i++)
            {
                int idx = System.Array.IndexOf(layerNames, names.GetArrayElementAtIndex(i).stringValue);
                if (idx >= 0)
                {
                    mask |= 1 << idx;
                }
            }

            EditorGUI.BeginProperty(position, label, property);
            int newMask = EditorGUI.MaskField(position, label, mask, layerNames);
            if (newMask != mask)
            {
                var selected = new List<string>();
                for (int i = 0; i < layerNames.Length; i++)
                {
                    if ((newMask & (1 << i)) != 0)
                    {
                        selected.Add(layerNames[i]);
                    }
                }

                names.ClearArray();
                for (int i = 0; i < selected.Count; i++)
                {
                    names.InsertArrayElementAtIndex(i);
                    names.GetArrayElementAtIndex(i).stringValue = selected[i];
                }
            }
            EditorGUI.EndProperty();
        }
    }
}
