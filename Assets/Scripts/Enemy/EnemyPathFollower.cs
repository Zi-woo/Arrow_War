using ArrowWar.Data;
using ArrowWar.Effects;
using UnityEngine;

namespace ArrowWar.Enemy
{
    /// <summary>
    /// Added at runtime by EnemyUnit when its EnemyData has a PathData assigned.
    /// Each call to Tick() advances the enemy toward the next waypoint using the
    /// enemy's configured moveSpeed, respecting any active slow effects.
    ///
    /// Once all waypoints are consumed (and loop is false) IsComplete becomes true
    /// and EnemyUnit falls back to the default left-only movement.
    /// </summary>
    public class EnemyPathFollower : MonoBehaviour
    {
        private PathData _pathData;
        private float _moveSpeed;
        private StatusEffectReceiver _statusFx;
        private int _currentIndex;

        public bool IsComplete { get; private set; }

        /// <summary>Called once by EnemyUnit.Initialize before the first Update.</summary>
        public void Setup(PathData pathData, float moveSpeed, StatusEffectReceiver statusFx)
        {
            _pathData = pathData;
            _moveSpeed = moveSpeed;
            _statusFx = statusFx;
            _currentIndex = 0;
            IsComplete = pathData == null || pathData.waypoints.Count == 0;
        }

        /// <summary>
        /// Advances the transform one frame toward the current target waypoint.
        /// Called explicitly by EnemyUnit so the update order stays clear.
        /// </summary>
        public void Tick()
        {
            if (IsComplete) return;

            Vector2 target = _pathData.waypoints[_currentIndex];
            Vector2 current = transform.position;

            float speedMul = _statusFx != null ? _statusFx.SpeedMultiplier : 1f;
            float step = _moveSpeed * speedMul * Time.deltaTime;

            Vector2 next = Vector2.MoveTowards(current, target, step);
            transform.position = new Vector3(next.x, next.y, transform.position.z);

            // Flip sprite to face the direction of travel
            float dx = target.x - current.x;
            if (Mathf.Abs(dx) > 0.01f)
            {
                Vector3 s = transform.localScale;
                s.x = Mathf.Abs(s.x) * (dx < 0 ? 1f : -1f);
                transform.localScale = s;
            }

            if (Vector2.Distance(next, target) < 0.05f)
                AdvanceWaypoint();
        }

        private void AdvanceWaypoint()
        {
            _currentIndex++;

            if (_currentIndex >= _pathData.waypoints.Count)
            {
                if (_pathData.loop)
                    _currentIndex = 0;
                else
                    IsComplete = true;
            }
        }
    }
}
