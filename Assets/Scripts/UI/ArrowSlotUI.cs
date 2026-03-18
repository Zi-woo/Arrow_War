using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ArrowWar.Archery;

namespace ArrowWar.UI
{
    /// <summary>
    /// Builds and drives the 5-slot arrow bar at the bottom of the screen entirely at runtime.
    /// No manual slot-view wiring is needed — just attach this to a canvas child GO and assign
    /// the slotManager reference in the Inspector.
    ///
    /// The panel anchors itself to the bottom-centre of the canvas. Each slot shows:
    ///   - Background image (colour changes with selection/state)
    ///   - Arrow icon (visible when a slot has an arrow)
    ///   - Radial cooldown overlay (darkens as cooldown runs)
    ///   - Key label ("1"–"5") in the bottom-left corner
    /// </summary>
    public class ArrowSlotUI : MonoBehaviour
    {
        [SerializeField] private ArrowSlotManager slotManager;

        [Header("Layout")]
        [SerializeField] private float slotSize      = 80f;
        [SerializeField] private float slotSpacing   = 10f;
        [SerializeField] private float bottomPadding = 10f;

        [Header("Colors")]
        [SerializeField] private Color emptyColor    = new Color(0.20f, 0.20f, 0.20f, 0.95f);
        [SerializeField] private Color filledColor   = new Color(0.30f, 0.30f, 0.35f, 0.95f);
        [SerializeField] private Color selectedColor = new Color(0.85f, 0.75f, 0.10f, 0.95f);

        // ──────────────────────────────────────────────────────────────────────
        // Per-slot widget references (built at runtime)
        // ──────────────────────────────────────────────────────────────────────
        private struct SlotWidgets
        {
            public Image     background;
            public Image     arrowIcon;
            public Image     cooldownOverlay;
            public TextMeshProUGUI keyLabel;
        }

        private SlotWidgets[] _widgets;

        // ──────────────────────────────────────────────────────────────────────

        private void Start()
        {
            PositionPanel();
            BuildSlots();

            slotManager.OnSlotSelected    += HandleSlotSelected;
            slotManager.OnCooldownChanged += HandleCooldownChanged;
            slotManager.OnSlotsRefreshed  += RefreshAllSlots;

            RefreshAllSlots();
            HandleSlotSelected(slotManager.SelectedSlotIndex);
        }

        private void OnDestroy()
        {
            if (slotManager == null) return;
            slotManager.OnSlotSelected    -= HandleSlotSelected;
            slotManager.OnCooldownChanged -= HandleCooldownChanged;
            slotManager.OnSlotsRefreshed  -= RefreshAllSlots;
        }

        // ──────────────────────────────────────────────────────────────────────
        // Panel & slot construction
        // ──────────────────────────────────────────────────────────────────────

        private void PositionPanel()
        {
            var rt         = GetComponent<RectTransform>();
            float w        = ArrowSlotManager.SlotCount * slotSize
                           + (ArrowSlotManager.SlotCount - 1) * slotSpacing;

            rt.anchorMin        = new Vector2(0.5f, 0f);
            rt.anchorMax        = new Vector2(0.5f, 0f);
            rt.pivot            = new Vector2(0.5f, 0f);
            rt.sizeDelta        = new Vector2(w, slotSize);
            rt.anchoredPosition = new Vector2(0f, bottomPadding);
        }

        private void BuildSlots()
        {
            _widgets = new SlotWidgets[ArrowSlotManager.SlotCount];
            float totalWidth = ArrowSlotManager.SlotCount * slotSize
                             + (ArrowSlotManager.SlotCount - 1) * slotSpacing;

            for (int i = 0; i < ArrowSlotManager.SlotCount; i++)
            {
                float xPos = -totalWidth * 0.5f + i * (slotSize + slotSpacing) + slotSize * 0.5f;
                _widgets[i] = CreateSlotWidgets(i, xPos);
            }
        }

        private SlotWidgets CreateSlotWidgets(int index, float xPos)
        {
            // ── Root slot ────────────────────────────────────────────────────
            var root = new GameObject($"Slot_{index + 1}");
            root.transform.SetParent(transform, false);
            var rootRt = root.AddComponent<RectTransform>();
            rootRt.sizeDelta        = new Vector2(slotSize, slotSize);
            rootRt.anchoredPosition = new Vector2(xPos, 0f);

            var bg    = root.AddComponent<Image>();
            bg.color  = emptyColor;

            // ── Arrow icon ───────────────────────────────────────────────────
            var iconGO = MakeChild(root.transform, "Icon");
            StretchRect(iconGO, new Vector2(0.1f, 0.15f), new Vector2(0.9f, 0.88f));
            var icon            = iconGO.AddComponent<Image>();
            icon.preserveAspect = true;
            icon.enabled        = false;

            // ── Cooldown overlay (filled radial) ─────────────────────────────
            var cdGO  = MakeChild(root.transform, "Cooldown");
            StretchRect(cdGO, Vector2.zero, Vector2.one);
            var cd              = cdGO.AddComponent<Image>();
            cd.color            = new Color(0f, 0f, 0f, 0.65f);
            cd.type             = Image.Type.Filled;
            cd.fillMethod       = Image.FillMethod.Radial360;
            cd.fillOrigin       = (int)Image.Origin360.Top;
            cd.fillClockwise    = false;
            cd.fillAmount       = 0f;

            // ── Key label ────────────────────────────────────────────────────
            var labelGO = MakeChild(root.transform, "KeyLabel");
            StretchRect(labelGO, new Vector2(0f, 0f), new Vector2(0.45f, 0.28f));
            var label               = labelGO.AddComponent<TextMeshProUGUI>();
            label.text              = (index + 1).ToString();
            label.fontSize          = 14f;
            label.alignment         = TextAlignmentOptions.BottomLeft;
            label.color             = new Color(1f, 1f, 1f, 0.7f);
            label.margin            = new Vector4(4f, 0f, 0f, 2f);

            return new SlotWidgets
            {
                background      = bg,
                arrowIcon       = icon,
                cooldownOverlay = cd,
                keyLabel        = label,
            };
        }

        // ──────────────────────────────────────────────────────────────────────
        // Event handlers
        // ──────────────────────────────────────────────────────────────────────

        private void RefreshAllSlots()
        {
            if (_widgets == null) return;

            for (int i = 0; i < _widgets.Length && i < ArrowSlotManager.SlotCount; i++)
            {
                ArrowSlot slot = slotManager.Slots[i];
                bool      has  = slot.arrowData != null;

                _widgets[i].arrowIcon.enabled = has;
                if (has)
                {
                    _widgets[i].arrowIcon.sprite = slot.arrowData.icon;
                    _widgets[i].background.color = (i == slotManager.SelectedSlotIndex)
                                                   ? selectedColor : filledColor;
                }
                else
                {
                    _widgets[i].background.color = emptyColor;
                }

                _widgets[i].cooldownOverlay.fillAmount = 0f;
            }
        }

        private void HandleSlotSelected(int selectedIndex)
        {
            if (_widgets == null) return;

            for (int i = 0; i < _widgets.Length; i++)
            {
                ArrowSlot slot = slotManager.Slots[i];
                if (slot.arrowData != null)
                    _widgets[i].background.color = (i == selectedIndex) ? selectedColor : filledColor;
            }
        }

        private void HandleCooldownChanged(int slotIndex, float remaining)
        {
            if (_widgets == null || slotIndex < 0 || slotIndex >= _widgets.Length) return;

            ArrowSlot slot = slotManager.Slots[slotIndex];
            float fill = (slot.arrowData != null && slot.arrowData.cooldownTime > 0f)
                ? remaining / slot.arrowData.cooldownTime
                : 0f;

            _widgets[slotIndex].cooldownOverlay.fillAmount = fill;
        }

        // ──────────────────────────────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────────────────────────────

        private static GameObject MakeChild(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private static void StretchRect(GameObject go, Vector2 anchorMin, Vector2 anchorMax)
        {
            var rt        = go.GetComponent<RectTransform>();
            rt.anchorMin  = anchorMin;
            rt.anchorMax  = anchorMax;
            rt.sizeDelta  = Vector2.zero;
            rt.pivot      = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
        }
    }
}
