using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using ArrowWar.Data;
using ArrowWar.Economy;

namespace ArrowWar.UI
{
    /// <summary>
    /// Represents one purchasable arrow in the upgrade shop grid.
    /// Entirely self-building — call Initialize() immediately after AddComponent().
    /// Implements hover-tooltip via IPointerEnterHandler / IPointerExitHandler.
    /// </summary>
    public class UpgradeItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private static readonly Color ColorAffordable  = new Color(0.28f, 0.28f, 0.35f);
        private static readonly Color ColorOwned       = new Color(0.25f, 0.55f, 0.25f);
        private static readonly Color ColorTooExpensive= new Color(0.20f, 0.20f, 0.22f);

        private ArrowData    _data;
        private UpgradePanel _panel;

        private Image              _background;
        private Image              _iconImage;
        private TextMeshProUGUI    _priceText;
        private GameObject         _tooltip;
        private Button             _button;

        // ──────────────────────────────────────────────────────────────────────

        /// <summary>Must be called right after AddComponent&lt;UpgradeItemUI&gt;().</summary>
        public void Initialize(ArrowData data, UpgradePanel panel)
        {
            _data  = data;
            _panel = panel;
            BuildUI();
            _button.onClick.AddListener(HandleClick);
            RefreshState();
        }

        private void BuildUI()
        {
            var rt       = GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(90f, 100f);

            _background        = gameObject.AddComponent<Image>();
            _background.color  = ColorAffordable;

            _button                = gameObject.AddComponent<Button>();
            _button.targetGraphic  = _background;

            // ── Icon ──────────────────────────────────────────────────────────
            var iconGO      = Child("Icon");
            Anchor(iconGO, new Vector2(0.1f, 0.28f), new Vector2(0.9f, 0.92f));
            _iconImage      = iconGO.AddComponent<Image>();
            _iconImage.preserveAspect = true;
            if (_data.icon != null)
                _iconImage.sprite = _data.icon;
            else
                _iconImage.color = new Color(0.6f, 0.6f, 0.6f);

            // ── Price label ────────────────────────────────────────────────────
            var priceGO   = Child("Price");
            Anchor(priceGO, new Vector2(0f, 0f), new Vector2(1f, 0.30f));
            _priceText    = priceGO.AddComponent<TextMeshProUGUI>();
            _priceText.fontSize   = 13f;
            _priceText.alignment  = TextAlignmentOptions.Center;
            _priceText.color      = Color.white;

            // ── Tooltip (shown on hover, appears above the item) ──────────────
            _tooltip       = Child("Tooltip");
            var tipRt      = _tooltip.GetComponent<RectTransform>();
            tipRt.anchorMin        = new Vector2(0f, 1f);
            tipRt.anchorMax        = new Vector2(1f, 1f);
            tipRt.pivot            = new Vector2(0.5f, 0f);
            tipRt.sizeDelta        = new Vector2(40f, 70f);   // extra width via sizeDelta
            tipRt.anchoredPosition = Vector2.zero;
            var tipBg      = _tooltip.AddComponent<Image>();
            tipBg.color    = new Color(0.05f, 0.05f, 0.05f, 0.95f);

            var tipTextGO  = ChildOf(_tooltip.transform, "Text");
            Anchor(tipTextGO, Vector2.zero, Vector2.one, new Vector2(-6f, -4f));
            var tipTMP     = tipTextGO.AddComponent<TextMeshProUGUI>();
            tipTMP.text    = $"<b>{_data.displayName}</b>\n{_data.description}";
            tipTMP.fontSize      = 11f;
            tipTMP.alignment     = TextAlignmentOptions.Center;
            tipTMP.color         = Color.white;
            tipTMP.enableWordWrapping = true;

            _tooltip.SetActive(false);
        }

        // ──────────────────────────────────────────────────────────────────────

        public void RefreshState()
        {
            if (_data == null) return;

            bool owned     = ArrowInventory.Instance != null && ArrowInventory.Instance.OwnsArrow(_data);
            bool canAfford = GoldManager.Instance != null && GoldManager.Instance.MatchGold >= _data.price;

            _button.interactable = !owned && canAfford;
            _background.color    = owned ? ColorOwned
                                 : canAfford ? ColorAffordable
                                 : ColorTooExpensive;

            _priceText.text = owned ? "Owned" : $"{_data.price}g";
        }

        public void OnPointerEnter(PointerEventData _) { if (_tooltip) _tooltip.SetActive(true); }
        public void OnPointerExit (PointerEventData _) { if (_tooltip) _tooltip.SetActive(false); }

        private void HandleClick()
        {
            if (_data == null) return;
            if (ArrowInventory.Instance.OwnsArrow(_data)) return;
            if (!GoldManager.Instance.SpendGold(_data.price)) return;

            ArrowInventory.Instance.AddArrow(_data);
            RefreshState();
            _panel?.NotifyPurchase();
        }

        // ──────────────────────────────────────────────────────────────────────
        // Layout helpers
        // ──────────────────────────────────────────────────────────────────────

        private GameObject Child(string name) => ChildOf(transform, name);

        private static GameObject ChildOf(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private static void Anchor(GameObject go, Vector2 min, Vector2 max,
                                   Vector2 sizeDelta = default)
        {
            var rt            = go.GetComponent<RectTransform>();
            rt.anchorMin      = min;
            rt.anchorMax      = max;
            rt.sizeDelta      = sizeDelta;
            rt.pivot          = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
        }
    }
}
