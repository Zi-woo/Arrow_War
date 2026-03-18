using System;
using UnityEngine;
using ArrowWar.Core;
using ArrowWar.Data;
using ArrowWar.Economy;

namespace ArrowWar.Archery
{
    [Serializable]
    public class ArrowSlot
    {
        // arrowData is populated from ArrowInventory at runtime, not assigned in Inspector.
        [HideInInspector] public ArrowData arrowData;
        [HideInInspector] public float cooldownRemaining;

        public bool IsReady => arrowData != null && cooldownRemaining <= 0f;

        /// <summary>0 = on cooldown, 1 = fully ready. Used to drive the UI fill image.</summary>
        public float ReadyFraction
        {
            get
            {
                if (arrowData == null || arrowData.cooldownTime <= 0f) return 1f;
                return 1f - (cooldownRemaining / arrowData.cooldownTime);
            }
        }
    }

    /// <summary>
    /// Owns the 5 arrow slots. Slot contents are driven by ArrowInventory (not the Inspector).
    /// Handles number-key selection, per-slot cooldown timers, and delegates spawn to ArrowSpawner.
    /// </summary>
    public class ArrowSlotManager : MonoBehaviour
    {
        public const int SlotCount = 5;

        [SerializeField] private ArrowSpawner arrowSpawner;
        [SerializeField] private Camera gameCamera;

        private readonly ArrowSlot[] _slots = new ArrowSlot[SlotCount];
        private int _selectedSlotIndex;

        public int SelectedSlotIndex => _selectedSlotIndex;
        public ArrowSlot[] Slots => _slots;

        /// <summary>Fired when the player switches slots. Passes new slot index.</summary>
        public event Action<int> OnSlotSelected;

        /// <summary>Fired every frame a slot's cooldown changes. Passes (slotIndex, remainingSeconds).</summary>
        public event Action<int, float> OnCooldownChanged;

        /// <summary>Fired when slot contents change due to inventory update. ArrowSlotUI refreshes on this.</summary>
        public event Action OnSlotsRefreshed;

        private void Awake()
        {
            for (int i = 0; i < SlotCount; i++)
                _slots[i] = new ArrowSlot();
        }

        private void Start()
        {
            if (ArrowInventory.Instance != null)
            {
                ArrowInventory.Instance.OnInventoryChanged += SyncFromInventory;
                SyncFromInventory();
            }
            else
            {
                Debug.LogWarning("[ArrowSlotManager] ArrowInventory.Instance is null. Slots will stay empty.");
            }
        }

        private void OnDestroy()
        {
            if (ArrowInventory.Instance != null)
                ArrowInventory.Instance.OnInventoryChanged -= SyncFromInventory;
        }

        private void SyncFromInventory()
        {
            var owned = ArrowInventory.Instance.OwnedArrows;
            for (int i = 0; i < SlotCount; i++)
            {
                ArrowData newData = (i < owned.Count) ? owned[i] : null;
                if (_slots[i].arrowData != newData)
                {
                    _slots[i].arrowData = newData;
                    _slots[i].cooldownRemaining = 0f;
                }
            }
            OnSlotsRefreshed?.Invoke();
        }

        private void Update()
        {
            if (GameFlowManager.Instance != null &&
                GameFlowManager.Instance.CurrentState != GameState.Battle)
                return;

            HandleSlotSelection();
            TickCooldowns();
            HandleFireInput();
        }

        private void HandleSlotSelection()
        {
            for (int i = 0; i < SlotCount; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                    SelectSlot(i);
            }
        }

        private void TickCooldowns()
        {
            for (int i = 0; i < SlotCount; i++)
            {
                if (_slots[i].cooldownRemaining > 0f)
                {
                    _slots[i].cooldownRemaining -= Time.deltaTime;
                    if (_slots[i].cooldownRemaining < 0f)
                        _slots[i].cooldownRemaining = 0f;
                    OnCooldownChanged?.Invoke(i, _slots[i].cooldownRemaining);
                }
            }
        }

        private void HandleFireInput()
        {
            if (!Input.GetMouseButtonDown(0)) return;
            Vector2 worldPos = gameCamera.ScreenToWorldPoint(Input.mousePosition);
            TryFire(worldPos);
        }

        public void SelectSlot(int index)
        {
            if (index < 0 || index >= SlotCount) return;
            _selectedSlotIndex = index;
            OnSlotSelected?.Invoke(_selectedSlotIndex);
        }

        /// <summary>
        /// Attempts to fire the currently selected slot toward worldTarget.
        /// Returns false if the slot is empty or on cooldown.
        /// </summary>
        public bool TryFire(Vector2 worldTarget)
        {
            ArrowSlot slot = _slots[_selectedSlotIndex];
            if (!slot.IsReady) return false;

            arrowSpawner.Fire(slot.arrowData, worldTarget);
            slot.cooldownRemaining = slot.arrowData.cooldownTime;
            OnCooldownChanged?.Invoke(_selectedSlotIndex, slot.cooldownRemaining);
            return true;
        }
    }
}
