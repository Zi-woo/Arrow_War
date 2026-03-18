using System;
using UnityEngine;

namespace ArrowWar.Economy
{
    /// <summary>
    /// Tracks gold earned during the current match.
    /// EnemySpawner calls EarnGold on each kill. The upgrade shop will call SpendGold.
    /// Fires OnGoldChanged so the HUD can update without a direct reference.
    /// </summary>
    public class GoldManager : MonoBehaviour
    {
        public static GoldManager Instance { get; private set; }

        private int _matchGold;

        public int MatchGold => _matchGold;

        /// <summary>Fired whenever the gold amount changes. Passes the new total.</summary>
        public event Action<int> OnGoldChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void EarnGold(int amount)
        {
            if (amount <= 0) return;
            _matchGold += amount;
            OnGoldChanged?.Invoke(_matchGold);
        }

        /// <summary>Returns true and deducts amount if there is enough gold.</summary>
        public bool SpendGold(int amount)
        {
            if (_matchGold < amount) return false;
            _matchGold -= amount;
            OnGoldChanged?.Invoke(_matchGold);
            return true;
        }

        public void ResetMatchGold()
        {
            _matchGold = 0;
            OnGoldChanged?.Invoke(_matchGold);
        }
    }
}
