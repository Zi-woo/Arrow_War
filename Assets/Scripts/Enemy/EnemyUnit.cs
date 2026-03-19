using System;
using UnityEngine;
using UnityEngine.UI;
using ArrowWar.Castle;
using ArrowWar.Data;
using ArrowWar.Effects;

namespace ArrowWar.Enemy
{
    /// <summary>
    /// Attached to each spawned enemy instance. Moves left toward the player castle,
    /// stops at the gate, attacks repeatedly, and dies when HP reaches 0.
    ///
    /// Animation contract — all enemy Animator Controllers must expose:
    ///   Bool    "IsMoving"  — true while walking, false when stopped at castle.
    ///   Trigger "Attack"    — set each time an attack is issued.
    ///   Trigger "Die"       — set on death; deathAnimDuration controls destroy delay.
    ///
    /// Health bar: created at runtime as a world-space Canvas child so the prefab
    /// requires no additional UI setup.
    /// </summary>
    public class EnemyUnit : MonoBehaviour
    {
        private EnemyData _data;
        private Castle.Castle _targetCastle;
        private int _currentHP;
        private bool _isDead;
        private bool _isAttacking;
        private float _attackTimer;

        // Optional — null-safe if the prefab has no Animator.
        private Animator _animator;
        private StatusEffectReceiver _statusFx;

        // Health bar runtime refs (built in Start)
        private RectTransform _fillRect;
        private Image _fillImage;

        /// <summary>
        /// Always fired on death regardless of cause.
        /// Passes goldReward = data.goldReward when killed by player, 0 on castle contact.
        /// EnemySpawner subscribes to decrement the alive count in all cases.
        /// </summary>
        public event Action<int> OnDied;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _statusFx = GetComponent<StatusEffectReceiver>();
        }

        /// <summary>Called by EnemySpawner immediately after instantiation.</summary>
        public void Initialize(EnemyData data, Castle.Castle targetCastle)
        {
            _data = data;
            _targetCastle = targetCastle;
            _currentHP = data.maxHP;

            _animator?.SetBool("IsMoving", true);
        }

        private void Start()
        {
            BuildHealthBar();
        }

        private void Update()
        {
            if (_isDead) return;

            if (_isAttacking)
                TickAttack();
            else
                MoveAndCheckContact();
        }

        // ── Movement & castle contact ──────────────────────────────────────────

        private void MoveAndCheckContact()
        {
            float speedMul = _statusFx != null ? _statusFx.SpeedMultiplier : 1f;
            transform.Translate(Vector2.left * (_data.moveSpeed * speedMul * Time.deltaTime));

            if (_targetCastle == null) return;

            float contactThreshold = _targetCastle.transform.position.x + 0.5f;
            if (transform.position.x <= contactThreshold)
                BeginAttacking();
        }

        private void BeginAttacking()
        {
            _isAttacking = true;
            _attackTimer = _data.attackInterval; // attack immediately on arrival

            _animator?.SetBool("IsMoving", false);
        }

        // ── Repeating attack loop ──────────────────────────────────────────────

        private void TickAttack()
        {
            if (_targetCastle == null) return;

            _attackTimer += Time.deltaTime;
            if (_attackTimer < _data.attackInterval) return;

            _attackTimer = 0f;
            _targetCastle.TakeDamage(_data.attackDamage);
            _animator?.SetTrigger("Attack");
        }

        // ── Damage & death ────────────────────────────────────────────────────

        public void TakeDamage(int amount)
        {
            if (_isDead) return;

            _currentHP = Mathf.Max(0, _currentHP - amount);
            UpdateHealthBar((float)_currentHP / _data.maxHP);

            if (_currentHP <= 0)
                Die(killedByPlayer: true);
        }

        private void Die(bool killedByPlayer)
        {
            if (_isDead) return;
            _isDead = true;

            // Disable collider so arrows pass through the corpse.
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            _animator?.SetBool("IsMoving", false);
            _animator?.SetTrigger("Die");

            // Fire immediately so EnemySpawner decrements its count and awards gold.
            OnDied?.Invoke(killedByPlayer ? _data.goldReward : 0);

            Destroy(gameObject, 4f);
        }

        // ── Health bar (built programmatically — no prefab changes needed) ────

        private void BuildHealthBar()
        {
            var canvasGO = new GameObject("HPCanvas");
            canvasGO.transform.SetParent(transform, worldPositionStays: false);
            canvasGO.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            canvasGO.transform.localScale = new Vector3(0.01f, 0.01f, 1f);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 10;

            var canvasRt = canvasGO.GetComponent<RectTransform>();
            canvasRt.sizeDelta = new Vector2(50f, 10f);

            var bgGO = new GameObject("BG");
            bgGO.transform.SetParent(canvasGO.transform, worldPositionStays: false);
            var bgRt = bgGO.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            bgGO.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

            var fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(canvasGO.transform, worldPositionStays: false);
            _fillRect = fillGO.AddComponent<RectTransform>();
            _fillRect.anchorMin = Vector2.zero;
            _fillRect.anchorMax = Vector2.one;
            _fillRect.sizeDelta = Vector2.zero;
            _fillImage = fillGO.AddComponent<Image>();
            _fillImage.color = Color.green;
        }

        private void UpdateHealthBar(float fraction)
        {
            if (_fillRect == null) return;

            fraction = Mathf.Clamp01(fraction);
            _fillRect.anchorMax = new Vector2(fraction, 1f);
            _fillImage.color = Color.Lerp(Color.red, Color.green, fraction);
        }
    }
}
