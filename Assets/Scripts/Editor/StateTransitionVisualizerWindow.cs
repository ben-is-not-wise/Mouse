using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace HackedDesign
{
    public class StateTransitionVisualizerWindow : EditorWindow
    {
        private struct Family
        {
            public string Name;
            public Type Interface;
            public string[] ExcludeTypeNames;
        }

        private static readonly Family[] Families = new[]
        {
            new Family { Name = "Game (IState)", Interface = typeof(IState), ExcludeTypeNames = new string[0] },
            new Family { Name = "Enemy AI (IEnemyState)", Interface = typeof(IEnemyState), ExcludeTypeNames = new[] { nameof(EnemyUtilityState) } },
            new Family { Name = "Character (ICharacterState)", Interface = typeof(ICharacterState), ExcludeTypeNames = new string[0] },
        };

        private int selectedFamily = 0;

        [MenuItem("Window/HackedDesign/State Transition Visualizer")]
        public static void ShowWindow()
        {
            GetWindow<StateTransitionVisualizerWindow>("State Transitions");
        }

        private void OnGUI()
        {
            selectedFamily = EditorGUILayout.Popup(selectedFamily, Families.Select(f => f.Name).ToArray());

            var family = Families[selectedFamily];

            var stateTypes = family.Interface.Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && family.Interface.IsAssignableFrom(t))
                .Where(t => !family.ExcludeTypeNames.Contains(t.Name))
                .OrderBy(t => t.Name)
                .ToArray();

            if (stateTypes.Length == 0)
            {
                return;
            }

            Vector2 center = new Vector2(position.width / 2f, position.height / 2f + 10f);
            float radius = Mathf.Min(position.width, position.height) * 0.35f;
            Vector2 nodeSize = new Vector2(140, 30);

            var nodePositions = new System.Collections.Generic.Dictionary<Type, Vector2>();
            for (int i = 0; i < stateTypes.Length; i++)
            {
                float angle = i * Mathf.PI * 2f / stateTypes.Length;
                nodePositions[stateTypes[i]] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }

            Handles.BeginGUI();
            Handles.color = Color.gray;
            foreach (var from in stateTypes)
            {
                var attr = from.GetCustomAttribute<TransitionsToAttribute>();
                if (attr == null)
                {
                    continue;
                }

                foreach (var to in attr.States)
                {
                    if (!nodePositions.TryGetValue(to, out var toPos))
                    {
                        continue;
                    }

                    Handles.DrawLine(nodePositions[from], toPos);
                }
            }
            Handles.EndGUI();

            var style = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
            foreach (var t in stateTypes)
            {
                var pos = nodePositions[t];
                GUI.Box(new Rect(pos - nodeSize / 2f, nodeSize), t.Name, style);
            }
        }
    }
}
