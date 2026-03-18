using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using ArrowWar.Data;
using ArrowWar.Economy;
using ArrowWar.UI;

/// <summary>
/// One-click editor utility.
/// Run via:  ArrowWar > Setup Arrow Registry
///
/// What it does:
///   1. Creates ArrowRegistry.asset if it doesn't exist.
///   2. Scans the project for every ArrowData asset and adds them all to allArrows[].
///   3. Marks BasicArrow.asset as ownedByDefault = true.
///   4. Assigns the registry to ArrowInventory and UpgradePanel in the open scene.
///   5. Saves assets and the scene.
///
/// Re-running after adding new arrows is safe — it re-scans and updates the list.
/// </summary>
public static class ArrowRegistrySetup
{
    private const string RegistryPath = "Assets/Scripts/Data/ArrowRegistry.asset";

    [MenuItem("ArrowWar/Setup Arrow Registry")]
    public static void Run()
    {
        // ── 1. Find or create the registry asset ────────────────────────────
        ArrowRegistry registry = AssetDatabase.LoadAssetAtPath<ArrowRegistry>(RegistryPath);
        if (registry == null)
        {
            registry = ScriptableObject.CreateInstance<ArrowRegistry>();
            AssetDatabase.CreateAsset(registry, RegistryPath);
            Debug.Log("[ArrowRegistrySetup] Created ArrowRegistry.asset at " + RegistryPath);
        }

        // ── 2. Collect every ArrowData asset in the project ─────────────────
        string[] guids = AssetDatabase.FindAssets("t:ArrowData", new[] { "Assets" });
        var arrows = new ArrowWar.Data.ArrowData[guids.Length];
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            arrows[i] = AssetDatabase.LoadAssetAtPath<ArrowData>(path);
            Debug.Log($"[ArrowRegistrySetup] Found ArrowData: {arrows[i].name} at {path}");
        }

        // ── 3. Write allArrows into the registry ────────────────────────────
        SerializedObject regSO = new SerializedObject(registry);
        SerializedProperty allArrowsProp = regSO.FindProperty("allArrows");
        allArrowsProp.arraySize = arrows.Length;
        for (int i = 0; i < arrows.Length; i++)
            allArrowsProp.GetArrayElementAtIndex(i).objectReferenceValue = arrows[i];
        regSO.ApplyModifiedPropertiesWithoutUndo();

        // ── 4. Mark BasicArrow as ownedByDefault ────────────────────────────
        foreach (var arrow in arrows)
        {
            if (arrow == null) continue;
            if (arrow.name == "BasicArrow")
            {
                SerializedObject arrowSO = new SerializedObject(arrow);
                SerializedProperty prop = arrowSO.FindProperty("ownedByDefault");
                if (prop != null && !prop.boolValue)
                {
                    prop.boolValue = true;
                    arrowSO.ApplyModifiedPropertiesWithoutUndo();
                    Debug.Log("[ArrowRegistrySetup] Marked BasicArrow as ownedByDefault = true");
                }
            }
        }

        AssetDatabase.SaveAssets();

        // ── 5. Assign registry to ArrowInventory in the open scene ──────────
        // FindObjectsByType with includeInactive so we catch disabled GOs too.
        var inventory = Object.FindFirstObjectByType<ArrowInventory>(FindObjectsInactive.Include);
        if (inventory != null)
        {
            SerializedObject invSO = new SerializedObject(inventory);
            invSO.FindProperty("registry").objectReferenceValue = registry;
            invSO.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(inventory);
            Debug.Log("[ArrowRegistrySetup] Assigned registry to ArrowInventory on '" + inventory.gameObject.name + "'");
        }
        else
        {
            Debug.LogWarning("[ArrowRegistrySetup] ArrowInventory not found in scene — assign registry manually.");
        }

        // ── 6. Assign registry to UpgradePanel in the open scene ────────────
        var panel = Object.FindFirstObjectByType<UpgradePanel>(FindObjectsInactive.Include);
        if (panel != null)
        {
            SerializedObject panelSO = new SerializedObject(panel);
            panelSO.FindProperty("registry").objectReferenceValue = registry;
            panelSO.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(panel);
            Debug.Log("[ArrowRegistrySetup] Assigned registry to UpgradePanel on '" + panel.gameObject.name + "'");
        }
        else
        {
            Debug.LogWarning("[ArrowRegistrySetup] UpgradePanel not found in scene — assign registry manually.");
        }

        // ── 7. Save the scene ───────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.Refresh();

        Debug.Log("[ArrowRegistrySetup] Done. ArrowRegistry is set up and wired to the scene.");
    }
}
