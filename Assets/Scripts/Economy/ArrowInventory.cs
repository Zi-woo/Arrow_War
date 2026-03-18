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
    /// </summary>
    public class ArrowInventory : MonoBehaviour
    {
        public static ArrowInventory Instance { get; private set; }

        [Tooltip("The central arrow catalogue. Arrows with ownedByDefault = true are granted at match start.")]
        [SerializeField] private ArrowRegistry registry;

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

            foreach (var arrow in registry.allArrows)
            {
                if (arrow != null && arrow.ownedByDefault)
                    AddArrow(arrow);
            }
        }

        /// <summary>Adds an arrow to the owned list and notifies listeners.</summary>
        public void AddArrow(ArrowData arrow)
        {
            if (arrow == null) return;
            _owned.Add(arrow);
            OnInventoryChanged?.Invoke();
        }

        public bool OwnsArrow(ArrowData arrow) => _owned.Contains(arrow);

        /// <summary>Returns the full registry so the upgrade panel can read all arrows.</summary>
        public ArrowRegistry Registry => registry;
    }
}
