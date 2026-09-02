using UnityEditor;
using UnityEngine;

namespace HackedDesign.Editor
{
    [CustomEditor(typeof(OperatingSystem))]
    public class OperatingSystemEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (!Application.isPlaying)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Runtime loadout is shown here during Play mode.", MessageType.None);
                return;
            }

            var os = (OperatingSystem)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime Loadout", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Inventory", EditorStyles.miniBoldLabel);
            var inventory = os.Inventory;
            for (int i = 0; i < inventory.Count; i++)
            {
                EditorGUILayout.LabelField($"  {i}", inventory[i] != null ? inventory[i].name : "-");
            }

            EditorGUILayout.LabelField("Hacks", EditorStyles.miniBoldLabel);
            var hackSlots = os.HackSlots;
            for (int i = 0; i < hackSlots.Count; i++)
            {
                var instance = hackSlots[i];
                EditorGUILayout.LabelField($"  {i}", instance?.definition != null ? instance.definition.name : "-");
                if (instance?.installed != null)
                {
                    for (int j = 0; j < instance.installed.Length; j++)
                    {
                        var sub = instance.installed[j];
                        EditorGUILayout.LabelField($"      sub {j}", sub != null ? sub.name : "-");
                    }
                }
            }

            EditorGUILayout.LabelField("Hacks Current", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField($"  slot {os.CurrentHackSlot}", os.CurrentHack != null ? os.CurrentHack.name : "-");

            EditorGUILayout.LabelField("Upgrades", EditorStyles.miniBoldLabel);
            var upgradeSlots = os.UpgradeSlots;
            for (int i = 0; i < upgradeSlots.Count; i++)
            {
                var instance = upgradeSlots[i];
                EditorGUILayout.LabelField($"  {i}", instance?.definition != null ? instance.definition.name : "-");
            }

            EditorGUILayout.LabelField("Weapons", EditorStyles.miniBoldLabel);
            var weaponSlots = os.WeaponSlots;
            for (int i = 0; i < weaponSlots.Count; i++)
            {
                string label = $"  {(WeaponSlotId)i}{(i == os.CurrentWeaponSlot ? " *" : "")}";
                DrawWeaponSlot(label, weaponSlots[i]);
            }

            Repaint();
        }

        private static void DrawWeaponSlot(string label, WeaponInstance instance)
        {
            EditorGUILayout.LabelField(label, instance?.definition != null ? instance.definition.name : "-");
            if (instance?.installed == null)
            {
                return;
            }
            for (int i = 0; i < instance.installed.Length; i++)
            {
                var mod = instance.installed[i];
                EditorGUILayout.LabelField($"      mod {i}", mod != null ? mod.name : "-");
            }
        }
    }
}
