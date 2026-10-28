using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HackedDesign.Editor
{
    public partial class ColorUsageWindow
    {
        private const int RawMode = 0;
        private const int PaletteMode = 1;
        private const string NewPalettePath = "Assets/Settings/ColorPalette.asset";

        private static readonly string[] ModeNames = { "Raw colours", "Palette" };

        private readonly HashSet<string> paletteExpanded = new HashSet<string>();
        private List<ColorPalette> palettes = new List<ColorPalette>();
        private int mode;

        public static void ShowPalette()
        {
            var window = GetWindow<ColorUsageWindow>("Colour Usages");
            window.mode = PaletteMode;
            if (window.uses.Count == 0)
            {
                window.Scan();
            }
        }

        private void DrawModeToolbar()
        {
            this.mode = GUILayout.Toolbar(this.mode, ModeNames);
        }

        private void RefreshPalettes()
        {
            this.palettes = AssetDatabase.FindAssets("t:" + nameof(ColorPalette))
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => AssetDatabase.LoadAssetAtPath<ColorPalette>(path))
                .Where(p => p != null)
                .ToList();
        }

        private void DrawPaletteUses()
        {
            using var scrollScope = new EditorGUILayout.ScrollViewScope(this.scroll);
            this.scroll = scrollScope.scrollPosition;

            if (this.palettes.Count == 0)
            {
                EditorGUILayout.HelpBox("No colour palettes yet. Create one with the button below or from Create › Mouse › Colour Palette.", MessageType.Info);
                if (GUILayout.Button("Create palette", GUILayout.Width(120)))
                {
                    CreatePalette();
                    this.RefreshPalettes();
                }

                return;
            }

            foreach (var palette in this.palettes)
            {
                if (palette == null)
                {
                    continue;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(palette.name, EditorStyles.boldLabel);
                    if (GUILayout.Button("Edit", GUILayout.Width(50)))
                    {
                        Selection.activeObject = palette;
                        EditorGUIUtility.PingObject(palette);
                    }
                }

                EditorGUI.indentLevel++;
                for (var index = 0; index < palette.Entries.Count; index++)
                {
                    var entry = palette.Entries[index];
                    var entryUses = this.uses.Where(u => this.IsNear(entry.Color, u.Value)).ToList();
                    if (!this.MatchesEntry(entry, entryUses))
                    {
                        continue;
                    }

                    this.DrawEntry(palette, index, entryUses);
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }

        private bool IsNear(Color entryColor, Color value)
        {
            value.a = entryColor.a;
            return this.tolerance > 0f ? Distance(entryColor, value) <= this.tolerance : Key(entryColor) == Key(value);
        }

        private bool MatchesEntry(ColorPaletteEntry entry, List<ColorUse> entryUses)
        {
            var text = this.filter.Trim();
            if (text.Length == 0)
            {
                return true;
            }

            return (entry.Name ?? string.Empty).IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0 ||
                FormatColor(entry.Color).IndexOf(text.TrimStart('#'), StringComparison.OrdinalIgnoreCase) >= 0 ||
                entryUses.Any(u => u.ObjectPath.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0 || u.ContainerPath.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void DrawEntry(ColorPalette palette, int index, List<ColorUse> entryUses)
        {
            var entry = palette.Entries[index];
            var key = AssetDatabase.GetAssetPath(palette) + entry.Id;
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(EditorGUI.indentLevel * 15f);
                DrawSwatch(GUILayoutUtility.GetRect(40, 18, GUILayout.Width(40)), entry.Color);
                var isExpanded = EditorGUILayout.Foldout(this.paletteExpanded.Contains(key), $"{entry.Name}   {FormatColor(entry.Color)}   ({entryUses.Count} uses)", true);
                if (isExpanded)
                {
                    this.paletteExpanded.Add(key);
                }
                else
                {
                    this.paletteExpanded.Remove(key);
                }
            }

            if (!this.paletteExpanded.Contains(key))
            {
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space((EditorGUI.indentLevel + 1) * 15f);
                var replacement = this.replacements.TryGetValue(key, out var stored) ? stored : entry.BaseColor;
                this.replacements[key] = EditorGUILayout.ColorField(new GUIContent("Replace with"), replacement, true, true, false);
                if (GUILayout.Button("Replace all", GUILayout.Width(90)))
                {
                    this.ReplaceEntry(palette, index, entryUses, this.replacements[key]);
                    GUIUtility.ExitGUI();
                }
            }

            if (entryUses.Count == 0)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space((EditorGUI.indentLevel + 1) * 15f);
                    EditorGUILayout.LabelField("Not used anywhere.", EditorStyles.miniLabel);
                }

                return;
            }

            foreach (var use in entryUses)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space((EditorGUI.indentLevel + 1) * 15f);
                    DrawSwatch(GUILayoutUtility.GetRect(14, 14, GUILayout.Width(14)), use.Value);
                    EditorGUILayout.LabelField(new GUIContent($"{use.ContainerPath}  ›  {use.ObjectPath}  ›  {use.Label}", FormatColor(use.Value)));
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        this.Select(use);
                        GUIUtility.ExitGUI();
                    }
                }
            }
        }

        private void ReplaceEntry(ColorPalette palette, int index, List<ColorUse> entryUses, Color newBase)
        {
            var entry = palette.Entries[index];
            var scale = Mathf.Pow(2f, entry.Intensity);
            var newColor = new Color(newBase.r * scale, newBase.g * scale, newBase.b * scale, newBase.a);
            var message = $"Change \"{entry.Name}\" to {FormatColor(newColor)} and replace its {entryUses.Count} uses?";
            if (!ConfirmReplace(message, entryUses))
            {
                return;
            }

            var serializedPalette = new SerializedObject(palette);
            serializedPalette.FindProperty("entries").GetArrayElementAtIndex(index).FindPropertyRelative("color").colorValue = newBase;
            serializedPalette.ApplyModifiedProperties();
            EditorUtility.SetDirty(palette);

            this.ReplaceUses(entryUses, newColor);
            AssetDatabase.SaveAssets();
            this.Scan();
        }

        private static ColorPalette CreatePalette()
        {
            var palette = CreateInstance<ColorPalette>();
            AssetDatabase.CreateAsset(palette, AssetDatabase.GenerateUniqueAssetPath(NewPalettePath));
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(palette);
            return palette;
        }
    }
}
