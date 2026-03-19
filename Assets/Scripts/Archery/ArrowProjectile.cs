using UnityEngine;
using ArrowWar.Castle;
using ArrowWar.Data;
using ArrowWar.Enemy;

namespace ArrowWar.Archery
{
    /// <summary>
    /// Attached to each spawned arrow instance. Reads behaviour from its ArrowData.
    /// Rotates to follow the velocity vector, detects hits via trigger, then destroys itself.
    /// Requires a Rigidbody2D and a trigger Collider2D on the same GameObject.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class ArrowProjectile : MonoBehaviour
    {
        private ArrowData _data;
        private Rigidbody2D _rb;
        private bool _hasHit;

        // Angle (degrees) from root +X to the arrowhead child's rest-pose direction.
        // Computed once in Awake so RotateToVelocity aligns the arrowhead with velocity.
        private float _arrowheadAngleOffset;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();

            // The arrow is instantiated with identity rotation, so localPosition of the
            // arrowhead child directly gives its rest-pose direction from the root.
            Transform arrowhead = transform.Find("ArrowHead");
            if (arrowhead == null) arrowhead = transform.Find("arrowhead"); // case fallback
            if (arrowhead != null)
            {
                Vector2 localDir = arrowhead.localPosition;
                if (localDir.sqrMagnitude > 0.001f)
                    _arrowheadAngleOffset = Mathf.Atan2(localDir.y, localDir.x) * Mathf.Rad2Deg;
            }
        }

        /// <summary>Called by ArrowSpawner immediately after instantiation.</summary>
        public void Initialize(ArrowData data, Vector2 launchVelocity)
        {
            _data = data;
            _rb.gravityScale = data.gravityScale;
            _rb.velocity = launchVelocity;

            // Safety: auto-destroy if somehow the arrow never hits anything (e.g. fired off-screen).
            Destroy(gameObject, 10f);
        }

        private void Update()
        {
            if (!_hasHit)
                RotateToVelocity();
        }

        private void RotateToVelocity()
        {
            if (_rb.velocity.sqrMagnitude > 0.01f)
            {
                float velAngle = Mathf.Atan2(_rb.velocity.y, _rb.velocity.x) * Mathf.Rad2Deg;
                // Subtract the arrowhead's rest-pose angle so the arrowhead tip (not +X)
                // points toward the velocity direction.
                transform.rotation = Quaternion.AngleAxis(velAngle - _arrowheadAngleOffset, Vector3.forward);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_hasHit) return;

            if (other.CompareTag("Enemy"))
            {
                HitEnemy(other);
            }
            else if (other.CompareTag("Castle"))
            {
                HitCastle(other);
            }
            else if (other.CompareTag("Ground"))
            {
                DestroyProjectile();
            }
        }

        private void HitCastle(Collider2D castleCollider)
        {
            castleCollider.GetComponent<Castle.Castle>()?.TakeDamage(_data.damage);
            DestroyProjectile();
        }

        private void HitEnemy(Collider2D enemyCollider)
        {
            if (_data.splashRadius > 0f)
                ApplySplashDamage(transform.position, _data.splashRadius, _data.damage);
            else
                enemyCollider.GetComponent<EnemyUnit>()?.TakeDamage(_data.damage);

            DestroyProjectile();
        }

        private void ApplySplashDamage(Vector2 center, float radius, int damage)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                    hit.GetComponent<EnemyUnit>()?.TakeDamage(damage);
            }
        }

        private void DestroyProjectile()
        {
            _hasHit = true;
            Destroy(gameObject);
        }
    }
}
