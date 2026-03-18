using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// One-shot editor utility. Run via ArrowWar > Setup Sprites & Save Scene.
/// Safe to delete after running.
/// </summary>
public static class ArrowWarSetup
{
    [MenuItem("ArrowWar/Setup Sprites and Save Scene")]
    static void SetupSpritesAndSave()
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/WhiteSquare.png");
        if (sprite == null)
        {
            Debug.LogError("[ArrowWarSetup] WhiteSquare.png not found. Did you refresh the asset database?");
            return;
        }

        // --- Scene objects ---
        AssignToScene("PlayerCastle", sprite, new Color(0.2f, 0.5f, 1f, 1f));
        AssignToScene("EnemyCastle",  sprite, new Color(1f,   0.2f, 0.2f, 1f));
        AssignToScene("Ground",       sprite, new Color(0.4f, 0.25f, 0.1f, 1f));

        // --- Prefabs ---
        AssignToPrefab("Assets/Prefabs/Arrows/BasicArrow.prefab",  sprite, new Color(1f, 0.9f, 0f, 1f));
        AssignToPrefab("Assets/Prefabs/Enemies/Soldier.prefab",    sprite, new Color(1f, 0.4f, 0f, 1f));

        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("[ArrowWarSetup] Sprites assigned and scene saved.");
    }

    static void AssignToScene(string goName, Sprite sprite, Color color)
    {
        GameObject go = GameObject.Find(goName);
        if (go == null) { Debug.LogWarning($"[ArrowWarSetup] '{goName}' not found in scene."); return; }

        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) { Debug.LogWarning($"[ArrowWarSetup] '{goName}' has no SpriteRenderer."); return; }

        sr.sprite = sprite;
        sr.color  = color;
        EditorUtility.SetDirty(go);
    }

    static void AssignToPrefab(string path, Sprite sprite, Color color)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) { Debug.LogWarning($"[ArrowWarSetup] Prefab not found: {path}"); return; }

        SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
        if (sr == null) { Debug.LogWarning($"[ArrowWarSetup] Prefab '{path}' has no SpriteRenderer."); return; }

        sr.sprite = sprite;
        sr.color  = color;
        EditorUtility.SetDirty(prefab);
    }
}
