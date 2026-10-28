using HackedDesign.UI;
using UnityEditor;
using UnityEditor.UI;

namespace HackedDesign.Editor
{
    [CustomEditor(typeof(WikiButton))]
    public class WikiButtonEditor : ButtonEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            this.serializedObject.Update();
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.serializedObject.FindProperty("leftLabel"));
            EditorGUILayout.PropertyField(this.serializedObject.FindProperty("rightLabel"));
            EditorGUILayout.PropertyField(this.serializedObject.FindProperty("activeColor"));
            this.serializedObject.ApplyModifiedProperties();
        }
    }
}
