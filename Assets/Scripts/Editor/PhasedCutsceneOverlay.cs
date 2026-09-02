using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;
using HackedDesign;

namespace HackedDesign.Editor
{
    [Overlay(typeof(SceneView), "Cutscene Phases")]
    public class PhasedCutsceneOverlay : Overlay
    {
        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement();
            System.Action onSelectionChanged = null;
            onSelectionChanged = () => Rebuild(root);

            Rebuild(root);
            Selection.selectionChanged += onSelectionChanged;
            root.RegisterCallback<DetachFromPanelEvent>(_ => Selection.selectionChanged -= onSelectionChanged);

            return root;
        }

        private void Rebuild(VisualElement root)
        {
            root.Clear();

            var behaviour = Selection.activeGameObject != null
                ? Selection.activeGameObject.GetComponent<PhasedCutsceneBehaviour>()
                : null;

            if (behaviour == null)
            {
                root.Add(new Label("Select an ICutscene object."));
                return;
            }

            var so = new SerializedObject(behaviour);
            var phases = so.FindProperty("phases");

            for (int i = 0; i < phases.arraySize; i++)
            {
                var phaseName = phases.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue;
                var label = string.IsNullOrEmpty(phaseName) ? $"Phase {i}" : $"Phase {i}: {phaseName}";

                int index = i;
                var button = new Button(() => PhasedCutsceneBehaviourEditor.PreviewPhase(behaviour, index)) { text = label };
                root.Add(button);
            }
        }
    }
}
