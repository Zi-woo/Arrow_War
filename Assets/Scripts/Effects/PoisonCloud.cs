using System.Collections.Generic;
using UnityEngine;

namespace ArrowWar.Effects
{
    /// <summary>
    /// Persistent area spawned at PoisonArrow impact. Damages enemies inside
    /// at a fixed interval, then self-destructs after its lifetime expires.
    ///
    /// Requires a trigger CircleCollider2D and a kinematic Rigidbody2D
    /// (kinematic RB is needed so Unity fires trigger events even though
    /// enemy prefabs don't have a Rigidbody2D).
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class PoisonCloud : MonoBehaviour
    {
        [Header("Damage")]
        [SerializeField] private int damagePerTick = 5;
        [SerializeField] private float tickInterval = 0.5f;

        [Header("Lifetime")]
        [SerializeField] private float lifetime = 3f;

        private readonly HashSet<Enemy.EnemyUnit> _enemiesInside = new();
        private float _tickTimer;
        private float _lifetimeTimer;

        private void Start()
        {
            _lifetimeTimer = lifetime;

            // Ensure collider is a trigger.
            var col = GetComponent<CircleCollider2D>();
            col.isTrigger = true;

            // Ensure RB is kinematic (no physics movement, just trigger support).
            var rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        private void Update()
        {
            // Lifetime countdown.
            _lifetimeTimer -= Time.deltaTime;
            if (_lifetimeTimer <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            // Damage tick.
            _tickTimer += Time.deltaTime;
            if (_tickTimer >= tickInterval)
            {
                _tickTimer = 0f;
                DamageAllInside();
            }
        }

        private void DamageAllInside()
        {
            // Remove destroyed enemies (null check covers Destroy'd GameObjects).
            _enemiesInside.RemoveWhere(e => e == null);

            // Iterate over a snapshot so TakeDamage-triggered death/exit callbacks
            // cannot modify the set mid-loop.
            foreach (var enemy in new List<Enemy.EnemyUnit>(_enemiesInside))
            {
                if (enemy != null)
                    enemy.TakeDamage(damagePerTick);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Enemy")) return;

            var enemy = other.GetComponent<Enemy.EnemyUnit>();
            if (enemy != null)
                _enemiesInside.Add(enemy);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Enemy")) return;

            var enemy = other.GetComponent<Enemy.EnemyUnit>();
            if (enemy != null)
                _enemiesInside.Remove(enemy);
        }
    }
}
