using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HackedDesign.Editor
{
    public partial class ColorUsageWindow : EditorWindow
    {
        private const string ExcludePrefsKey = "HackedDesign.ColorUsage.Exclude";
        private const string DefaultExcludes = "Assets/_External;Assets/TextMesh Pro;Assets/Plugins;Assets/Packages;Assets/_Recovery";

        private static readonly HashSet<string> DefaultColorKeys = new HashSet<string>
        {
            "255,255,255,255",
            "0,0,0,255",
            "0,0,0,0",
            "255,255,255,0",
        };

        private sealed class ColorUse
        {
            public string ContainerPath { get; set; }
            public bool IsScene { get; set; }
            public string ObjectPath { get; set; }
            public string Label { get; set; }
            public string GlobalId { get; set; }
            public string PropertyPath { get; set; }
            public Color Value { get; set; }
        }

        private sealed class ColorCluster
        {
            public Color Representative { get; set; }
            public List<ColorUse> Uses { get; } = new List<ColorUse>();
            public int VariantCount { get; set; }
        }

        private readonly List<ColorUse> uses = new List<ColorUse>();
        private readonly HashSet<string> expanded = new HashSet<string>();
        private readonly Dictionary<string, Color> replacements = new Dictionary<string, Color>();
        private List<ColorCluster> clusters = new List<ColorCluster>();
        private Vector2 scroll;
        private bool includeClosedScenes = true;
        private bool skipInherited = true;
        private bool hideDefaults = true;
        private bool keepAlpha = true;
        private float tolerance;
        private string excludes;
        private string filter = string.Empty;

        [MenuItem("Window/HackedDesign/Colour Usages")]
        public static void ShowWindow()
        {
            GetWindow<ColorUsageWindow>("Colour Usages");
        }

        private void OnEnable()
        {
            this.excludes = EditorPrefs.GetString(ExcludePrefsKey, DefaultExcludes);
        }

        private void OnGUI()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorGUILayout.HelpBox("Exit Play mode to scan or replace colours.", MessageType.Info);
                return;
            }

            this.DrawModeToolbar();
            this.DrawToolbar();
            if (this.mode == RawMode)
            {
                this.DrawClusters();
            }
            else
            {
                this.DrawPaletteUses();
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Scan", GUILayout.Width(80)))
                {
                    this.Scan();
                    GUIUtility.ExitGUI();
                }

                this.includeClosedScenes = EditorGUILayout.ToggleLeft("Closed scenes", this.includeClosedScenes, GUILayout.Width(110));
                this.skipInherited = EditorGUILayout.ToggleLeft(new GUIContent("Skip inherited", "Ignore prefab instance values that aren't overridden; they're counted on the source prefab."), this.skipInherited, GUILayout.Width(110));
                this.keepAlpha = EditorGUILayout.ToggleLeft(new GUIContent("Keep alpha", "When replacing, each use keeps its own alpha."), this.keepAlpha, GUILayout.Width(90));
            }

            EditorGUI.BeginChangeCheck();
            this.excludes = EditorGUILayout.TextField(new GUIContent("Exclude folders", "Semicolon separated."), this.excludes);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetString(ExcludePrefsKey, this.excludes);
            }

            EditorGUI.BeginChangeCheck();
            this.hideDefaults = EditorGUILayout.Toggle(new GUIContent("Hide defaults", "Hide pure white, black and clear."), this.hideDefaults);
            this.tolerance = EditorGUILayout.Slider(new GUIContent("Merge similar", "Group colours within this distance (0-255 per channel)."), this.tolerance, 0f, 32f);
            if (EditorGUI.EndChangeCheck())
            {
                this.RebuildClusters();
            }

            this.filter = EditorGUILayout.TextField(new GUIContent("Filter", "Hex value, component, property or path."), this.filter);
            EditorGUILayout.LabelField($"{this.clusters.Sum(c => c.Uses.Count)} uses across {this.clusters.Count} colours", EditorStyles.miniLabel);
        }

        private void DrawClusters()
        {
            using var scrollScope = new EditorGUILayout.ScrollViewScope(this.scroll);
            this.scroll = scrollScope.scrollPosition;

            foreach (var cluster in this.clusters.ToList())
            {
                if (!this.Matches(cluster))
                {
                    continue;
                }

                var key = Key(cluster.Representative);
                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawSwatch(GUILayoutUtility.GetRect(40, 18, GUILayout.Width(40)), cluster.Representative);
                    var variants = cluster.VariantCount > 1 ? $", {cluster.VariantCount} variants" : string.Empty;
                    var isExpanded = EditorGUILayout.Foldout(this.expanded.Contains(key), $"{FormatColor(cluster.Representative)}   ({cluster.Uses.Count} uses{variants})", true);
                    if (isExpanded)
                    {
                        this.expanded.Add(key);
                    }
                    else
                    {
                        this.expanded.Remove(key);
                    }

                    if (GUILayout.Button("Copy", GUILayout.Width(50)))
                    {
                        EditorGUIUtility.systemCopyBuffer = FormatColor(cluster.Representative);
                    }
                }

                if (!this.expanded.Contains(key))
                {
                    continue;
                }

                EditorGUI.indentLevel++;
                using (new EditorGUILayout.HorizontalScope())
                {
                    var replacement = this.replacements.TryGetValue(key, out var stored) ? stored : cluster.Representative;
                    this.replacements[key] = EditorGUILayout.ColorField(new GUIContent("Replace with"), replacement, true, true, false);
                    if (GUILayout.Button("Replace all", GUILayout.Width(90)))
                    {
                        this.Replace(cluster, this.replacements[key]);
                        GUIUtility.ExitGUI();
                    }
                }

                foreach (var use in cluster.Uses)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Space(EditorGUI.indentLevel * 15f);
                        DrawSwatch(GUILayoutUtility.GetRect(14, 14, GUILayout.Width(14)), use.Value);
                        EditorGUILayout.LabelField(new GUIContent($"{use.ContainerPath}  ›  {use.ObjectPath}  ›  {use.Label}", FormatColor(use.Value)));
                        if (GUILayout.Button("Select", GUILayout.Width(60)))
                        {
                            this.Select(use);
                            GUIUtility.ExitGUI();
                        }
                    }
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }

        private bool Matches(ColorCluster cluster)
        {
            var text = this.filter.Trim();
            if (text.Length == 0)
            {
                return true;
            }

            if (FormatColor(cluster.Representative).IndexOf(text.TrimStart('#'), StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return cluster.Uses.Any(u =>
                FormatColor(u.Value).IndexOf(text.TrimStart('#'), StringComparison.OrdinalIgnoreCase) >= 0 ||
                u.Label.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0 ||
                u.ObjectPath.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0 ||
                u.ContainerPath.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void Scan()
        {
            this.uses.Clear();
            var excluded = this.excludes.Split(';').Select(s => s.Trim().TrimEnd('/')).Where(s => s.Length > 0).ToArray();
            bool IsExcluded(string path) => excluded.Any(e => path.Equals(e, StringComparison.OrdinalIgnoreCase) || path.StartsWith(e + "/", StringComparison.OrdinalIgnoreCase));

            var assetPaths = new[] { "t:Prefab", "t:Material", "t:ScriptableObject" }
                .SelectMany(f => AssetDatabase.FindAssets(f, new[] { "Assets" }))
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Distinct()
                .Where(p => !IsExcluded(p))
                .ToList();
            var scenePaths = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" })
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(p => !IsExcluded(p))
                .ToList();

            try
            {
                var total = (float)(assetPaths.Count + scenePaths.Count);
                var index = 0;
                foreach (var path in assetPaths)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Scanning colours", path, index++ / total))
                    {
                        return;
                    }

                    this.ScanAsset(path);
                }

                foreach (var path in scenePaths)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Scanning colours", path, index++ / total))
                    {
                        return;
                    }

                    this.ScanScene(path);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                this.RefreshPalettes();
                this.RebuildClusters();
                this.Repaint();
            }
        }

        private void ScanAsset(string path)
        {
            if (path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            {
                var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (root == null)
                {
                    return;
                }

                foreach (var component in root.GetComponentsInChildren<Component>(true))
                {
                    if (component != null)
                    {
                        this.ScanObject(component, path, false, HierarchyPath(component.transform));
                    }
                }

                return;
            }

            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset == null || asset is GameObject || asset is Component)
                {
                    continue;
                }

                this.ScanObject(asset, path, false, asset.name);
            }
        }

        private void ScanScene(string path)
        {
            var scene = SceneManager.GetSceneByPath(path);
            var opened = false;
            if (!scene.isLoaded)
            {
                if (!this.includeClosedScenes)
                {
                    return;
                }

                scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                opened = true;
            }

            try
            {
                foreach (var root in scene.GetRootGameObjects())
                {
                    foreach (var component in root.GetComponentsInChildren<Component>(true))
                    {
                        if (component != null)
                        {
                            this.ScanObject(component, path, true, HierarchyPath(component.transform));
                        }
                    }
                }
            }
            finally
            {
                if (opened)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private void ScanObject(UnityEngine.Object target, string containerPath, bool isScene, string objectPath)
        {
            if (target is ColorPalette)
            {
                return;
            }

            var inherited = this.skipInherited && PrefabUtility.IsPartOfPrefabInstance(target);
            var material = target as Material;
            var serializedObject = new SerializedObject(target);
            var iterator = serializedObject.GetIterator();
            string globalId = null;

            while (iterator.Next(true))
            {
                if (iterator.propertyType != SerializedPropertyType.Color)
                {
                    continue;
                }

                if (inherited && !iterator.prefabOverride)
                {
                    continue;
                }

                var label = iterator.displayName;
                if (material != null && iterator.propertyPath.EndsWith(".second", StringComparison.Ordinal))
                {
                    var namePath = iterator.propertyPath.Substring(0, iterator.propertyPath.Length - ".second".Length) + ".first";
                    var shaderProperty = serializedObject.FindProperty(namePath)?.stringValue;
                    if (string.IsNullOrEmpty(shaderProperty) || !material.HasProperty(shaderProperty))
                    {
                        continue;
                    }

                    label = shaderProperty;
                }

                globalId ??= GlobalObjectId.GetGlobalObjectIdSlow(target).ToString();
                this.uses.Add(new ColorUse
                {
                    ContainerPath = containerPath,
                    IsScene = isScene,
                    ObjectPath = objectPath,
                    Label = $"{target.GetType().Name}.{label}",
                    GlobalId = globalId,
                    PropertyPath = iterator.propertyPath,
                    Value = iterator.colorValue,
                });
            }
        }

        private void RebuildClusters()
        {
            var exactGroups = this.uses
                .Where(u => !this.hideDefaults || !DefaultColorKeys.Contains(Key(u.Value)))
                .GroupBy(u => Key(u.Value))
                .OrderByDescending(g => g.Count());

            var result = new List<ColorCluster>();
            foreach (var group in exactGroups)
            {
                var value = group.First().Value;
                var cluster = this.tolerance > 0f ? result.FirstOrDefault(c => Distance(c.Representative, value) <= this.tolerance) : null;
                if (cluster == null)
                {
                    cluster = new ColorCluster { Representative = value };
                    result.Add(cluster);
                }

                cluster.Uses.AddRange(group);
                cluster.VariantCount++;
            }

            this.clusters = result.OrderByDescending(c => c.Uses.Count).ToList();
        }

        private void Select(ColorUse use)
        {
            if (!use.IsScene && use.ContainerPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            {
                var stage = PrefabStageUtility.OpenPrefab(use.ContainerPath);
                var found = stage != null ? FindInRoot(stage.prefabContentsRoot.transform, use.ObjectPath) : null;
                if (found != null)
                {
                    Selection.activeGameObject = found.gameObject;
                    EditorGUIUtility.PingObject(found.gameObject);
                    return;
                }
            }

            var target = Resolve(use);
            if (target == null && use.IsScene)
            {
                EditorSceneManager.OpenScene(use.ContainerPath, OpenSceneMode.Additive);
                target = Resolve(use);
            }

            if (target == null)
            {
                Debug.LogWarning($"Couldn't find {use.ObjectPath} in {use.ContainerPath}. Try scanning again.", this);
                return;
            }

            if (target is Component component)
            {
                Selection.activeGameObject = component.gameObject;
                EditorGUIUtility.PingObject(component.gameObject);
                return;
            }

            if (AssetDatabase.IsSubAsset(target))
            {
                target = AssetDatabase.LoadMainAssetAtPath(use.ContainerPath);
            }

            Selection.activeObject = target;
            EditorGUIUtility.PingObject(target);
        }

        private void Replace(ColorCluster cluster, Color newColor)
        {
            var message = $"Replace {cluster.Uses.Count} uses of {FormatColor(cluster.Representative)} with {FormatColor(newColor)}?";
            if (cluster.VariantCount > 1)
            {
                message += $"\n\nThis includes {cluster.VariantCount} similar colours merged into this group.";
            }

            if (!ConfirmReplace(message, cluster.Uses))
            {
                return;
            }

            this.ReplaceUses(cluster.Uses, newColor);
            AssetDatabase.SaveAssets();
            this.Scan();
        }

        private static bool ConfirmReplace(string message, IEnumerable<ColorUse> uses)
        {
            if (uses.Any(u => u.IsScene && !SceneManager.GetSceneByPath(u.ContainerPath).isLoaded))
            {
                message += "\n\nScenes that aren't open will be opened, changed, saved and closed.";
            }

            return EditorUtility.DisplayDialog("Replace colour", message, "Replace", "Cancel");
        }

        private void ReplaceUses(IEnumerable<ColorUse> uses, Color newColor)
        {
            foreach (var group in uses.GroupBy(u => u.ContainerPath))
            {
                var isScene = group.First().IsScene;
                var scene = default(Scene);
                var opened = false;
                if (isScene)
                {
                    scene = SceneManager.GetSceneByPath(group.Key);
                    if (!scene.isLoaded)
                    {
                        scene = EditorSceneManager.OpenScene(group.Key, OpenSceneMode.Additive);
                        opened = true;
                    }
                }

                try
                {
                    foreach (var use in group)
                    {
                        this.ReplaceUse(use, newColor, isScene);
                    }

                    if (isScene)
                    {
                        EditorSceneManager.MarkSceneDirty(scene);
                        if (opened)
                        {
                            EditorSceneManager.SaveScene(scene);
                        }
                    }
                }
                finally
                {
                    if (opened)
                    {
                        EditorSceneManager.CloseScene(scene, true);
                    }
                }
            }
        }

        private void ReplaceUse(ColorUse use, Color newColor, bool isScene)
        {
            var target = Resolve(use);
            if (target == null)
            {
                Debug.LogWarning($"Couldn't find {use.ObjectPath} in {use.ContainerPath}; skipped.", this);
                return;
            }

            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(use.PropertyPath);
            if (property == null || property.propertyType != SerializedPropertyType.Color)
            {
                Debug.LogWarning($"Couldn't find {use.Label} on {use.ObjectPath} in {use.ContainerPath}; skipped.", this);
                return;
            }

            var color = newColor;
            if (this.keepAlpha)
            {
                color.a = property.colorValue.a;
            }

            property.colorValue = color;
            serializedObject.ApplyModifiedProperties();
            if (!isScene)
            {
                EditorUtility.SetDirty(target);
            }
        }

        private static UnityEngine.Object Resolve(ColorUse use)
        {
            return GlobalObjectId.TryParse(use.GlobalId, out var id) ? GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) : null;
        }

        private static Transform FindInRoot(Transform root, string path)
        {
            var slash = path.IndexOf('/');
            return slash < 0 ? root : root.Find(path.Substring(slash + 1));
        }

        private static string HierarchyPath(Transform transform)
        {
            var path = transform.name;
            for (var parent = transform.parent; parent != null; parent = parent.parent)
            {
                path = parent.name + "/" + path;
            }

            return path;
        }

        private static string Key(Color color)
        {
            return $"{Mathf.RoundToInt(color.r * 255f)},{Mathf.RoundToInt(color.g * 255f)},{Mathf.RoundToInt(color.b * 255f)},{Mathf.RoundToInt(color.a * 255f)}";
        }

        private static float Distance(Color a, Color b)
        {
            var difference = new Vector4(a.r - b.r, a.g - b.g, a.b - b.b, a.a - b.a) * 255f;
            return difference.magnitude;
        }

        internal static string FormatColor(Color color)
        {
            var max = Mathf.Max(color.r, color.g, color.b);
            if (max <= 1f)
            {
                return "#" + ColorUtility.ToHtmlStringRGBA(color);
            }

            var baseColor = new Color(color.r / max, color.g / max, color.b / max, color.a);
            return $"#{ColorUtility.ToHtmlStringRGBA(baseColor)} HDR {Mathf.Log(max, 2f):0.#}";
        }

        internal static void DrawSwatch(Rect rect, Color color)
        {
            var max = Mathf.Max(color.r, color.g, color.b, 1f);
            EditorGUI.DrawRect(rect, Color.gray);
            EditorGUI.DrawRect(rect, new Color(color.r / max, color.g / max, color.b / max, color.a));
        }
    }
}
