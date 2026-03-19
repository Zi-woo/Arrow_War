using UnityEngine;
using TMPro;

namespace ArrowWar.UI
{
    /// <summary>
    /// Self-contained floating text spawned in world space.
    /// Floats upward and fades out, then destroys itself.
    /// Uses TextMeshPro (world-space) — no Canvas required.
    /// </summary>
    public class FloatingText : MonoBehaviour
    {
        [SerializeField] private float floatSpeed  = 1.2f;
        [SerializeField] private float lifetime    = 1.0f;

        private TextMeshPro _tmp;
        private float       _elapsed;

        /// <summary>Spawns a gold reward text above the given world position.</summary>
        public static void Spawn(Vector3 worldPosition, int gold)
        {
            if (gold <= 0) return;

            var go  = new GameObject("FloatingGold");
            go.transform.position = worldPosition + Vector3.up * 0.3f;

            var tmp             = go.AddComponent<TextMeshPro>();
            tmp.text            = $"+{gold}G";
            tmp.fontSize        = 3f;
            tmp.fontStyle       = FontStyles.Bold;
            tmp.color           = Color.yellow;
            tmp.alignment       = TextAlignmentOptions.Center;
            tmp.sortingOrder    = 20;

            go.AddComponent<FloatingText>();
        }

        private void Awake()
        {
            _tmp = GetComponent<TextMeshPro>();
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;

            transform.position += Vector3.up * (floatSpeed * Time.deltaTime);

            float alpha = Mathf.Lerp(1f, 0f, _elapsed / lifetime);
            var c = _tmp.color;
            c.a = alpha;
            _tmp.color = c;

            if (_elapsed >= lifetime)
                Destroy(gameObject);
        }
    }
}
