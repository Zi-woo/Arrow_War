using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ArrowWar.Archery;

namespace ArrowWar.UI
{
    /// <summary>
    /// Drives the 5-slot arrow bar. Slots must be pre-created in the scene as direct
    /// children of this GameObject named "Slot_1" through "Slot_5". Each slot needs:
    ///   - Image component on the root (background)
    ///   - Child "Icon"     : Image (preserveAspect = true)
    ///   - Child "Cooldown" : Image (Filled, Radial360, FillOrigin = Top)
    ///   - Child "KeyLabel" : TextMeshProUGUI
    /// </summary>
    public class ArrowSlotUI : MonoBehaviour
    {
        [SerializeField] private ArrowSlotManager slotManager;

        [Header("Colors")]
        [SerializeField] private Color slotEmptyColor    = new Color(0.20f, 0.20f, 0.20f, 0.95f);
        [SerializeField] private Color slotSelectedColor = new Color(0.85f, 0.75f, 0.10f, 0.40f);
        [SerializeField] private Color slotFilledColor   = new Color(0.10f, 0.75f, 0.20f, 0.40f);
        [SerializeField] private Color slotCooldownColor = new Color(0f,    0f,    0f,    0.70f);

        private struct SlotWidgets
        {
            public Image           background;
            public Image           arrowIcon;
            public Image           cooldownOverlay;
            public TextMeshProUGUI keyLabel;
        }

        private SlotWidgets[] _widgets;

        private void Start()
        {
            if (!FindSlotWidgets()) return;

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
        // Slot wiring (reads pre-existing scene children)
        // ──────────────────────────────────────────────────────────────────────

        private bool FindSlotWidgets()
        {
            _widgets = new SlotWidgets[ArrowSlotManager.SlotCount];

            for (int i = 0; i < ArrowSlotManager.SlotCount; i++)
            {
                string    slotName = $"Slot_{i + 1}";
                Transform slot     = transform.Find(slotName);

                if (slot == null)
                {
                    Debug.LogError($"[ArrowSlotUI] Child '{slotName}' not found. " +
                                   "Pre-create Slot_1 … Slot_5 under ArrowSllotPanel.");
                    return false;
                }

                var bg       = slot.GetComponent<Image>();
                var iconImg  = slot.Find("Icon")?.GetComponent<Image>();
                var cdImg    = slot.Find("Cooldown")?.GetComponent<Image>();
                var label    = slot.Find("KeyLabel")?.GetComponent<TextMeshProUGUI>();

                if (bg == null || iconImg == null || cdImg == null || label == null)
                {
                    Debug.LogError($"[ArrowSlotUI] Slot '{slotName}' is missing one or more " +
                                   "required components (Image on root, Icon/Image, " +
                                   "Cooldown/Image, KeyLabel/TMP).");
                    return false;
                }

                // Enforce Filled/Radial360 on the cooldown overlay at runtime
                // so it is always correct regardless of Inspector state.
                cdImg.color         = slotCooldownColor;
                cdImg.type          = Image.Type.Filled;
                cdImg.fillMethod    = Image.FillMethod.Radial360;
                cdImg.fillOrigin    = (int)Image.Origin360.Top;
                cdImg.fillClockwise = false;
                cdImg.fillAmount    = 0f;

                label.text = (i + 1).ToString();

                _widgets[i] = new SlotWidgets
                {
                    background      = bg,
                    arrowIcon       = iconImg,
                    cooldownOverlay = cdImg,
                    keyLabel        = label,
                };
            }

            return true;
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
                    _widgets[i].arrowIcon.sprite = slot.arrowData.icon;

                UpdateSlotVisuals(i);
            }
        }

        private void HandleSlotSelected(int selectedIndex)
        {
            if (_widgets == null) return;
            for (int i = 0; i < _widgets.Length; i++)
                UpdateSlotVisuals(i);
        }

        private void HandleCooldownChanged(int slotIndex, float remaining)
        {
            if (_widgets == null || slotIndex < 0 || slotIndex >= _widgets.Length) return;
            UpdateSlotVisuals(slotIndex);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Visual state
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Background = selection state (gray / yellow / green).
        /// Cooldown overlay = dark radial, only visible while on cooldown (1 → 0 as it expires).
        /// </summary>
        private void UpdateSlotVisuals(int i)
        {
            ArrowSlot slot       = slotManager.Slots[i];
            bool      isSelected = (i == slotManager.SelectedSlotIndex);
            bool      hasArrow   = slot.arrowData != null;

            // Background reflects selection state.
            if (!hasArrow)
                _widgets[i].background.color = slotEmptyColor;
            else if (isSelected)
                _widgets[i].background.color = slotSelectedColor;
            else
                _widgets[i].background.color = slotFilledColor;

            // Radial overlay: only shown while on cooldown.
            if (!hasArrow || slot.cooldownRemaining <= 0f)
            {
                _widgets[i].cooldownOverlay.fillAmount = 0f;
            }
            else
            {
                float fill = slot.cooldownRemaining / slot.arrowData.cooldownTime;
                _widgets[i].cooldownOverlay.fillAmount = Mathf.Clamp01(fill);
            }
        }
    }
}
