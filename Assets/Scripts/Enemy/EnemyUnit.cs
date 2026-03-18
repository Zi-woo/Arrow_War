using System;
using UnityEngine;
using UnityEngine.UI;
using ArrowWar.Castle;
using ArrowWar.Data;

namespace ArrowWar.Enemy
{
    /// <summary>
    /// Attached to each spawned enemy instance. Moves left toward the player castle,
    /// damages it on contact, and dies when HP reaches 0.
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

        // Health bar runtime refs (built in Start)
        private RectTransform _fillRect;
        private Image _fillImage;

        /// <summary>
        /// Always fired on death regardless of cause.
        /// Passes goldReward = data.goldReward when killed by player, 0 on castle contact.
        /// EnemySpawner subscribes to decrement the alive count in all cases.
        /// </summary>
        public event Action<int> OnDied;

        /// <summary>Called by EnemySpawner immediately after instantiation.</summary>
        public void Initialize(EnemyData data, Castle.Castle targetCastle)
        {
            _data = data;
            _targetCastle = targetCastle;
            _currentHP = data.maxHP;
        }

        private void Start()
        {
            BuildHealthBar();
        }

        private void Update()
        {
            if (_isDead) return;

            MoveTowardCastle();
            CheckCastleContact();
        }

        private void MoveTowardCastle()
        {
            transform.Translate(Vector2.left * (_data.moveSpeed * Time.deltaTime));
        }

        private void CheckCastleContact()
        {
            if (_targetCastle == null) return;

            float contactThreshold = _targetCastle.transform.position.x + 0.5f;
            if (transform.position.x <= contactThreshold)
            {
                _targetCastle.TakeDamage(_data.attackDamage);
                Die(killedByPlayer: false);
            }
        }

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

            // Always fire so EnemySpawner can decrement its alive count.
            // Gold is only awarded for player kills.
            OnDied?.Invoke(killedByPlayer ? _data.goldReward : 0);

            Destroy(gameObject);
        }

        // ------------------------------------------------------------------
        // Health bar (built programmatically — no prefab changes needed)
        // ------------------------------------------------------------------

        private void BuildHealthBar()
        {
            // World-space Canvas positioned above the enemy.
            var canvasGO = new GameObject("HPCanvas");
            canvasGO.transform.SetParent(transform, worldPositionStays: false);
            canvasGO.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            // 0.01 scale: 100-pixel canvas = 1 Unity unit wide.
            canvasGO.transform.localScale = new Vector3(0.01f, 0.01f, 1f);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 10;

            var canvasRt = canvasGO.GetComponent<RectTransform>();
            canvasRt.sizeDelta = new Vector2(50f, 10f);

            // Dark background panel.
            var bgGO = new GameObject("BG");
            bgGO.transform.SetParent(canvasGO.transform, worldPositionStays: false);
            var bgRt = bgGO.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            bgGO.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

            // Green fill — shrinks right-to-left via anchorMax.x.
            var fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(canvasGO.transform, worldPositionStays: false);
            _fillRect = fillGO.AddComponent<RectTransform>();
            _fillRect.anchorMin = Vector2.zero;
            _fillRect.anchorMax = Vector2.one;  // starts full
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
