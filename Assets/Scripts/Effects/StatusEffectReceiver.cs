using UnityEngine;

namespace ArrowWar.Effects
{
    /// <summary>
    /// Add to every enemy prefab. Tracks active status effects and exposes
    /// multipliers that EnemyUnit reads each frame.
    ///
    /// Slow rule: strongest slow wins. Re-applying refreshes duration.
    /// </summary>
    public class StatusEffectReceiver : MonoBehaviour
    {
        private float _slowPercent; // 0–1
        private float _slowTimer;   // remaining seconds

        /// <summary>
        /// Movement speed multiplier. 1 = normal, 0.4 = 60% slow.
        /// EnemyUnit multiplies its base moveSpeed by this value.
        /// </summary>
        public float SpeedMultiplier => _slowTimer > 0f ? 1f - _slowPercent : 1f;

        /// <summary>True while any slow is active.</summary>
        public bool IsSlowed => _slowTimer > 0f;

        /// <summary>
        /// Apply or refresh a slow effect.
        /// If a stronger slow is already active, keeps the stronger one.
        /// Duration is always refreshed to whichever is longer.
        /// </summary>
        public void ApplySlow(float percent, float duration)
        {
            _slowPercent = Mathf.Max(_slowPercent, Mathf.Clamp01(percent));
            _slowTimer = Mathf.Max(_slowTimer, duration);
        }

        private void Update()
        {
            if (_slowTimer <= 0f) return;

            _slowTimer -= Time.deltaTime;

            if (_slowTimer <= 0f)
            {
                _slowPercent = 0f;
                _slowTimer = 0f;
            }
        }
    }
}
