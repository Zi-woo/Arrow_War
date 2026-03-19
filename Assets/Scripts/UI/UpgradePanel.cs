using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ArrowWar.Data;

namespace ArrowWar.UI
{
    /// <summary>
    /// Shown after the full match ends. Reads allArrows from ArrowRegistry and
    /// displays every arrow — owned arrows show as green/Owned, unowned as purchasable.
    /// Entirely self-building — attach to a canvas child GO and assign registry in Inspector.
    ///
    /// GameFlowManager calls Show(); the Next Match button fires OnNextMatchClicked.
    /// </summary>
    public class UpgradePanel : MonoBehaviour
    {
        [Tooltip("The central arrow catalogue. All arrows in it are shown in the grid.")]
        [SerializeField] private ArrowRegistry registry;

        public event Action OnNextMatchClicked;

        private Transform              _gridContainer;
        private Button                 _nextMatchButton;
        private readonly List<UpgradeItemUI> _items = new List<UpgradeItemUI>();
        private bool                   _built;

        // ──────────────────────────────────────────────────────────────────────
        // Awake does nothing — the panel GO should be inactive in the scene.
        // All UI is built lazily on the first Show() call (play-mode only).
        // This avoids Awake running when the component is added in edit mode,
        // which previously caused an exception that destroyed the component.
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>Called by GameFlowManager at the start of each round transition.</summary>
        public void Show()
        {
            // Activate BEFORE building — Unity requires the GO to be active in the Canvas
            // hierarchy when adding UI components like Image; inactive GOs return null.
            gameObject.SetActive(true);

            if (!_built)
            {
                var rt = GetComponent<RectTransform>();
                if (rt == null)
                {
                    Debug.LogError("[UpgradePanel] RectTransform missing. " +
                                   "Make sure this GO is a direct child of the Canvas.");
                    return;
                }
                BuildPanelUI();
                _built = true;
            }
            RefreshItems();
        }

        /// <summary>Called by UpgradeItemUI after a purchase so all items recheck affordability.</summary>
        public void NotifyPurchase()
        {
            foreach (var item in _items)
                item?.RefreshState();
        }

        // ──────────────────────────────────────────────────────────────────────
        // UI construction (runs once in Awake)
        // ──────────────────────────────────────────────────────────────────────

        private void BuildPanelUI()
        {
            // Stretch the root RectTransform to fill the canvas.
            var rootRt       = GetComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.sizeDelta = Vector2.zero;

            // Full-screen dark overlay on a child GO — keeps the root clean and
            // avoids conflicts with any stale components from previous edit-mode runs.
            var overlayGO    = Child(transform, "Overlay");
            StretchFull(overlayGO);
            var overlay      = overlayGO.AddComponent<Image>();
            if (overlay == null)
            {
                Debug.LogError("[UpgradePanel] Failed to add overlay Image. " +
                               "Ensure this GO is active and in a Canvas hierarchy.");
                return;
            }
            overlay.color    = new Color(0f, 0f, 0f, 0.72f);

            // Centred panel box ─────────────────────────────────────────────
            var panelGO   = Child(transform, "Panel");
            var panelRt   = panelGO.GetComponent<RectTransform>();
            panelRt.anchorMin        = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax        = new Vector2(0.5f, 0.5f);
            panelRt.pivot            = new Vector2(0.5f, 0.5f);
            panelRt.sizeDelta        = new Vector2(520f, 360f);
            panelRt.anchoredPosition = Vector2.zero;
            var panelBg   = panelGO.AddComponent<Image>();
            panelBg.color = new Color(0.10f, 0.10f, 0.15f, 0.98f);

            // Title ─────────────────────────────────────────────────────────
            var titleGO   = Child(panelGO.transform, "Title");
            PinTop(titleGO, 50f, -5f);
            var title     = titleGO.AddComponent<TextMeshProUGUI>();
            title.text         = "UPGRADE SHOP";
            title.fontSize     = 26f;
            title.fontStyle    = FontStyles.Bold;
            title.alignment    = TextAlignmentOptions.Center;
            title.color        = Color.white;

            // Gold reminder line ─────────────────────────────────────────────
            var goldLabelGO = Child(panelGO.transform, "GoldHint");
            PinTop(goldLabelGO, 22f, -58f);
            var goldLabel   = goldLabelGO.AddComponent<TextMeshProUGUI>();
            goldLabel.text      = "Click an arrow to purchase it";
            goldLabel.fontSize  = 13f;
            goldLabel.alignment = TextAlignmentOptions.Center;
            goldLabel.color     = new Color(0.8f, 0.8f, 0.8f);

            // Grid container ────────────────────────────────────────────────
            var gridGO    = Child(panelGO.transform, "Grid");
            var gridRt    = gridGO.GetComponent<RectTransform>();
            gridRt.anchorMin        = new Vector2(0.05f, 0.18f);
            gridRt.anchorMax        = new Vector2(0.95f, 0.78f);
            gridRt.sizeDelta        = Vector2.zero;
            gridRt.anchoredPosition = Vector2.zero;
            var grid      = gridGO.AddComponent<GridLayoutGroup>();
            grid.cellSize       = new Vector2(90f, 100f);
            grid.spacing        = new Vector2(10f, 10f);
            grid.padding        = new RectOffset(8, 8, 4, 4);
            grid.childAlignment = TextAnchor.UpperLeft;
            _gridContainer = gridGO.transform;

            // Next Match button (centred) ───────────────────────────────────
            var nmBtnGO   = Child(panelGO.transform, "NextMatchBtn");
            var nmBtnRt   = nmBtnGO.GetComponent<RectTransform>();
            nmBtnRt.anchorMin        = new Vector2(0.5f, 0f);
            nmBtnRt.anchorMax        = new Vector2(0.5f, 0f);
            nmBtnRt.pivot            = new Vector2(0.5f, 0f);
            nmBtnRt.sizeDelta        = new Vector2(170f, 44f);
            nmBtnRt.anchoredPosition = new Vector2(0f, 14f);
            var nmBtnBg   = nmBtnGO.AddComponent<Image>();
            nmBtnBg.color = new Color(0.18f, 0.38f, 0.72f);
            _nextMatchButton               = nmBtnGO.AddComponent<Button>();
            _nextMatchButton.targetGraphic = nmBtnBg;
            _nextMatchButton.onClick.AddListener(HandleNextMatch);

            var nmBtnTextGO = Child(nmBtnGO.transform, "Label");
            StretchFull(nmBtnTextGO);
            var nmBtnText   = nmBtnTextGO.AddComponent<TextMeshProUGUI>();
            nmBtnText.text      = "NEXT MATCH";
            nmBtnText.fontSize  = 19f;
            nmBtnText.fontStyle = FontStyles.Bold;
            nmBtnText.alignment = TextAlignmentOptions.Center;
            nmBtnText.color     = Color.white;
        }

        // ──────────────────────────────────────────────────────────────────────
        // Item population (runs every Show())
        // ──────────────────────────────────────────────────────────────────────

        private void RefreshItems()
        {
            foreach (var item in _items)
                if (item != null) Destroy(item.gameObject);
            _items.Clear();

            if (registry == null || registry.allArrows == null) return;

            foreach (var arrow in registry.allArrows)
            {
                if (arrow == null) continue;
                var go   = new GameObject(arrow.displayName);
                go.transform.SetParent(_gridContainer, false);
                go.AddComponent<RectTransform>();
                var item = go.AddComponent<UpgradeItemUI>();
                item.Initialize(arrow, this);
                _items.Add(item);
            }
        }

        private void HandleNextMatch()
        {
            gameObject.SetActive(false);
            OnNextMatchClicked?.Invoke();
        }

        // ──────────────────────────────────────────────────────────────────────
        // Layout helpers
        // ──────────────────────────────────────────────────────────────────────

        private static GameObject Child(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private static void PinTop(GameObject go, float height, float yOffset)
        {
            var rt            = go.GetComponent<RectTransform>();
            rt.anchorMin      = new Vector2(0f, 1f);
            rt.anchorMax      = new Vector2(1f, 1f);
            rt.pivot          = new Vector2(0.5f, 1f);
            rt.sizeDelta      = new Vector2(0f, height);
            rt.anchoredPosition = new Vector2(0f, yOffset);
        }

        private static void StretchFull(GameObject go)
        {
            var rt            = go.GetComponent<RectTransform>();
            rt.anchorMin      = Vector2.zero;
            rt.anchorMax      = Vector2.one;
            rt.sizeDelta      = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }
    }
}
