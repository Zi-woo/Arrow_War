using System;
using System.Collections.Generic;
using UnityEngine;
using ArrowWar.Data;

namespace ArrowWar.Economy
{
    /// <summary>
    /// Singleton that owns the player's arrow collection for the current match.
    /// Reads the ArrowRegistry at start and grants every arrow marked ownedByDefault.
    /// ArrowSlotManager listens to OnInventoryChanged to sync the 5 firing slots.
    /// The upgrade shop calls AddArrow when a purchase is made.
    ///
    /// Persistence: _persistedOwned is static and survives scene reloads (Next Match),
    /// so purchased arrows carry over between matches. Call ClearPersistedOwned() on
    /// a full restart to wipe the slate clean.
    /// </summary>
    public class ArrowInventory : MonoBehaviour
    {
        public static ArrowInventory Instance { get; private set; }

        [Tooltip("The central arrow catalogue. Arrows with ownedByDefault = true are granted at match start.")]
        [SerializeField] private ArrowRegistry registry;

        // Survives LoadScene — accumulates every arrow the player has ever purchased.
        // Reset only when the player does a full restart (GameFlowManager.RestartBattle).
        private static readonly HashSet<ArrowData> _persistedOwned = new HashSet<ArrowData>();

        private readonly List<ArrowData> _owned = new List<ArrowData>();

        public IReadOnlyList<ArrowData> OwnedArrows => _owned;

        /// <summary>Fired whenever the owned list changes. ArrowSlotManager listens to sync slots.</summary>
        public event Action OnInventoryChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            if (registry == null)
            {
                Debug.LogError("[ArrowInventory] registry is not assigned. Assign ArrowRegistry.asset in the Inspector.");
                return;
            }

            // Grant default arrows.
            foreach (var arrow in registry.allArrows)
                if (arrow != null && arrow.ownedByDefault)
                    AddArrow(arrow);

            // Restore arrows purchased in previous matches.
            foreach (var arrow in _persistedOwned)
                if (arrow != null)
                    AddArrow(arrow); // AddArrow guards against duplicates
        }

        /// <summary>Adds an arrow to the owned list and notifies listeners. Ignores duplicates.</summary>
        public void AddArrow(ArrowData arrow)
        {
            if (arrow == null || _owned.Contains(arrow)) return;
            _owned.Add(arrow);
            _persistedOwned.Add(arrow);
            OnInventoryChanged?.Invoke();
        }

        public bool OwnsArrow(ArrowData arrow) => _owned.Contains(arrow);

        /// <summary>Returns the full registry so the upgrade panel can read all arrows.</summary>
        public ArrowRegistry Registry => registry;

        /// <summary>
        /// Wipes cross-match persistence. Call before a full restart so the next
        /// play-through begins with only the default arrows.
        /// </summary>
        public static void ClearPersistedOwned() => _persistedOwned.Clear();
    }
}
