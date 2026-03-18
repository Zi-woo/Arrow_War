using UnityEngine;
using UnityEngine.UI;

namespace ArrowWar.Castle
{
    /// <summary>
    /// Manages the player castle's HP. Fires events on damage and death.
    /// The CPU castle has no runtime logic and does not use this component.
    /// </summary>
    public class Castle : MonoBehaviour
    {
        [SerializeField] private int maxHP = 100;

        [Header("Health Bar")]
        [Tooltip("How far above the castle centre the health bar appears (world units).")]
        [SerializeField] private float healthBarOffsetY = 1.5f;

        private int _currentHP;

        // World-space health bar (built at runtime — no prefab setup needed)
        private RectTransform _fillRect;
        private Image _fillImage;

        public int CurrentHP => _currentHP;
        public int MaxHP => maxHP;

        /// <summary>Fired whenever damage is taken. Passes (currentHP, maxHP).</summary>
        public event System.Action<int, int> OnDamaged;

        /// <summary>Fired once when HP reaches 0. GameFlowManager listens for defeat.</summary>
        public event System.Action OnDestroyed;

        private void Awake()
        {
            _currentHP = maxHP;
        }

        private void Start()
        {
            BuildHealthBar();
        }

        public void TakeDamage(int amount)
        {
            if (_currentHP <= 0) return;

            _currentHP = Mathf.Max(0, _currentHP - amount);
            OnDamaged?.Invoke(_currentHP, maxHP);
            UpdateHealthBar((float)_currentHP / maxHP);

            if (_currentHP == 0)
                OnDestroyed?.Invoke();
        }

        /// <summary>Called by UpgradeApplier at match start to apply castle HP upgrades.</summary>
        public void SetMaxHP(int value, bool refillToMax = false)
        {
            maxHP = Mathf.Max(1, value);
            if (refillToMax)
                _currentHP = maxHP;
            else
                _currentHP = Mathf.Min(_currentHP, maxHP);
        }

        // ------------------------------------------------------------------
        // Health bar (world-space Canvas child — follows the castle automatically)
        // ------------------------------------------------------------------

        private void BuildHealthBar()
        {
            var canvasGO = new GameObject("HPCanvas");
            canvasGO.transform.SetParent(transform, worldPositionStays: false);

            // Divide by lossyScale so the world-space offset and bar size are independent
            // of whatever scale the castle GameObject has (e.g. scale Y = 2).
            Vector3 ls = transform.lossyScale;
            float localOffsetY = (ls.y > 0f) ? healthBarOffsetY / ls.y : healthBarOffsetY;
            canvasGO.transform.localPosition = new Vector3(0f, localOffsetY, 0f);
            canvasGO.transform.localScale = new Vector3(0.01f / ls.x, 0.01f / ls.y, 1f);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 10;

            var canvasRt = canvasGO.GetComponent<RectTransform>();
            canvasRt.sizeDelta = new Vector2(100f, 12f);  // 1 unit wide, 0.12 units tall in world

            // Dark background
            var bgGO = new GameObject("BG");
            bgGO.transform.SetParent(canvasGO.transform, worldPositionStays: false);
            var bgRt = bgGO.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            bgGO.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

            // Green fill — shrinks right-to-left via anchorMax.x
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
