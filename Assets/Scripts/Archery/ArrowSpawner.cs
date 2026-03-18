using UnityEngine;
using ArrowWar.Data;

namespace ArrowWar.Archery
{
    /// <summary>
    /// Instantiates arrow prefabs and gives them the correct launch velocity
    /// to follow a parabolic arc to the target position.
    /// </summary>
    public class ArrowSpawner : MonoBehaviour
    {
        [Tooltip("The point above the player castle where arrows are spawned.")]
        [SerializeField] private Transform firePoint;

        /// <summary>
        /// Spawns an arrow from the fire point aimed at targetWorldPos.
        /// Called by ArrowSlotManager after a successful fire check.
        /// </summary>
        public void Fire(ArrowData arrowData, Vector2 targetWorldPos)
        {
            if (arrowData.prefab == null)
            {
                Debug.LogWarning($"[ArrowSpawner] Prefab is null on ArrowData '{arrowData.displayName}'.");
                return;
            }

            GameObject instance = Instantiate(arrowData.prefab, firePoint.position, Quaternion.identity);

            ArrowProjectile projectile = instance.GetComponent<ArrowProjectile>();
            if (projectile == null)
            {
                Debug.LogError($"[ArrowSpawner] Prefab '{arrowData.prefab.name}' is missing ArrowProjectile component.");
                Destroy(instance);
                return;
            }

            Vector2 launchVelocity = CalculateLaunchVelocity(
                firePoint.position,
                targetWorldPos,
                arrowData.flightDuration,
                arrowData.gravityScale
            );

            projectile.Initialize(arrowData, launchVelocity);
        }

        /// <summary>
        /// Solves for the initial velocity needed to reach <paramref name="target"/> in exactly
        /// <paramref name="flightDuration"/> seconds under the given gravity scale.
        ///
        /// Derivation:
        ///   x(t) = start.x + vx * t           → vx = dx / T
        ///   y(t) = start.y + vy*t + ½*g*t²   → vy = (dy - ½*g*T²) / T
        /// where g = Physics2D.gravity.y * gravityScale (negative value).
        /// </summary>
        private Vector2 CalculateLaunchVelocity(Vector2 start, Vector2 target,
            float flightDuration, float gravityScale)
        {
            float g = Physics2D.gravity.y * gravityScale;
            float dx = target.x - start.x;
            float dy = target.y - start.y;
            float T = Mathf.Max(flightDuration, 0.1f); // guard against zero

            float vx = dx / T;
            float vy = (dy - 0.5f * g * T * T) / T;

            return new Vector2(vx, vy);
        }
    }
}
