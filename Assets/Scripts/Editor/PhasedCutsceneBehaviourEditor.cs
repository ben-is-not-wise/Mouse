using System.Reflection;
using UnityEditor;
using UnityEngine;
using HackedDesign;

namespace HackedDesign.Editor
{
    [CustomEditor(typeof(PhasedCutsceneBehaviour), true)]
    public class PhasedCutsceneBehaviourEditor : UnityEditor.Editor
    {
        private static readonly MethodInfo GoToPhaseMethod = typeof(PhasedCutsceneBehaviour)
            .GetMethod("GoToPhase", BindingFlags.Instance | BindingFlags.NonPublic);

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var behaviour = (PhasedCutsceneBehaviour)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Phase Preview (Edit Mode)", EditorStyles.boldLabel);

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Phase preview buttons are for edit mode only.", MessageType.Info);
                return;
            }

            var phases = serializedObject.FindProperty("phases");

            for (int i = 0; i < phases.arraySize; i++)
            {
                var phaseName = phases.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue;
                var label = string.IsNullOrEmpty(phaseName) ? $"Phase {i}" : $"Phase {i}: {phaseName}";

                if (GUILayout.Button(label))
                {
                    PreviewPhase(behaviour, i);
                }
            }
        }

        internal static void PreviewPhase(PhasedCutsceneBehaviour behaviour, int index)
        {
            GoToPhaseMethod.Invoke(behaviour, new object[] { index });
            EditorUtility.SetDirty(behaviour);
        }
    }
}
