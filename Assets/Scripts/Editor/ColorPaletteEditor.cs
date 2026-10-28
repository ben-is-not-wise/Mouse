using UnityEditor;
using UnityEngine;

namespace HackedDesign.Editor
{
    [CustomEditor(typeof(ColorPalette))]
    public class ColorPaletteEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            this.DrawDefaultInspector();
            EditorGUILayout.Space();
            if (GUILayout.Button("Find uses in Colour Usages"))
            {
                ColorUsageWindow.ShowPalette();
            }
        }
    }
}
