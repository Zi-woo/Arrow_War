using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using ArrowWar.UI;

/// <summary>
/// One-shot editor utility. Adds a castle HP Slider to BattleHUD and wires it.
/// Run via ArrowWar > Add HP Slider to HUD. Safe to delete after running.
/// </summary>
public static class AddHPSlider
{
    [MenuItem("ArrowWar/Add HP Slider to HUD")]
    static void Run()
    {
        GameObject hudGO = GameObject.Find("BattleHUD");
        if (hudGO == null) { Debug.LogError("[AddHPSlider] BattleHUD not found."); return; }

        BattleHUD hud = hudGO.GetComponent<BattleHUD>();
        if (hud == null) { Debug.LogError("[AddHPSlider] BattleHUD component not found."); return; }

        if (GameObject.Find("HPSlider") != null)
        {
            Debug.LogWarning("[AddHPSlider] HPSlider already exists — skipping.");
            return;
        }

        // ── Slider root ──────────────────────────────────────────────────────
        var sliderGO = new GameObject("HPSlider");
        sliderGO.transform.SetParent(hudGO.transform, false);

        var sliderRt = sliderGO.AddComponent<RectTransform>();
        sliderRt.anchorMin        = new Vector2(0f, 1f);
        sliderRt.anchorMax        = new Vector2(0f, 1f);
        sliderRt.pivot            = new Vector2(0f, 1f);
        sliderRt.anchoredPosition = new Vector2(10f, -10f);
        sliderRt.sizeDelta        = new Vector2(200f, 20f);

        var slider          = sliderGO.AddComponent<Slider>();
        slider.minValue     = 0f;
        slider.maxValue     = 1f;
        slider.value        = 1f;
        slider.wholeNumbers = false;
        slider.interactable = false;

        // ── Background ───────────────────────────────────────────────────────
        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(sliderGO.transform, false);
        var bgRt        = bgGO.AddComponent<RectTransform>();
        bgRt.anchorMin  = Vector2.zero;
        bgRt.anchorMax  = Vector2.one;
        bgRt.sizeDelta  = Vector2.zero;
        var bgImg       = bgGO.AddComponent<Image>();
        bgImg.color     = new Color(0.15f, 0.15f, 0.15f, 0.9f);

        // ── Fill Area ────────────────────────────────────────────────────────
        var fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(sliderGO.transform, false);
        var fillAreaRt        = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRt.anchorMin  = Vector2.zero;
        fillAreaRt.anchorMax  = Vector2.one;
        fillAreaRt.sizeDelta  = Vector2.zero;
        fillAreaRt.offsetMin  = Vector2.zero;
        fillAreaRt.offsetMax  = Vector2.zero;

        // ── Fill ─────────────────────────────────────────────────────────────
        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        var fillRt       = fillGO.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.sizeDelta = Vector2.zero;
        var fillImg      = fillGO.AddComponent<Image>();
        fillImg.color    = new Color(0.2f, 0.8f, 0.2f, 1f);

        slider.fillRect     = fillRt;
        slider.handleRect   = null;
        slider.targetGraphic = fillImg;

        // ── Wire hpSlider field on BattleHUD via reflection ──────────────────
        var field = typeof(BattleHUD).GetField("hpSlider",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (field != null)
        {
            field.SetValue(hud, slider);
            EditorUtility.SetDirty(hud);
            Debug.Log("[AddHPSlider] hpSlider wired via reflection.");
        }
        else
        {
            Debug.LogError("[AddHPSlider] Could not find 'hpSlider' field on BattleHUD.");
        }

        EditorUtility.SetDirty(sliderGO);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[AddHPSlider] Done — HPSlider created and scene saved.");
    }
}
