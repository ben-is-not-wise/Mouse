using HackedDesign.UI;
using UnityEditor;
using UnityEngine;

namespace HackedDesign.Editor
{
    [CustomEditor(typeof(AvatarPresenter))]
    public class AvatarPresenterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            this.DrawDefaultInspector();
            EditorGUILayout.Space();
            if (GUILayout.Button("Update Avatar"))
            {
                var faceImage = this.serializedObject.FindProperty("faceImage").objectReferenceValue;
                var hairImage = this.serializedObject.FindProperty("hairImage").objectReferenceValue;

                if (faceImage == null || hairImage == null)
                {
                    Debug.LogWarning("Face or hair image not assigned", this.target);
                    return;
                }

                Undo.RecordObjects(new Object[] { faceImage, hairImage }, "Update Avatar");
                ((AvatarPresenter)this.target).Refresh();
                EditorUtility.SetDirty(faceImage);
                EditorUtility.SetDirty(hairImage);
            }
        }
    }
}
