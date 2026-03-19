using UnityEngine;

namespace ArrowWar.Data
{
    [CreateAssetMenu(fileName = "NewArrowData", menuName = "ArrowWar/Arrow Data")]
    public class ArrowData : ScriptableObject
    {
        [Header("Identity")]
        public string displayName = "Basic Arrow";
        public Sprite icon;
        public GameObject prefab;

        [Header("Combat")]
        public int damage = 10;
        [Tooltip("Radius for splash damage. 0 = single target only.")]
        public float splashRadius = 0f;

        [Header("Hit Effect")]
        [Tooltip("Particle prefab spawned at the impact point. Null = no effect.")]
        public GameObject hitEffectPrefab;

        [Header("Status Effects")]
        [Tooltip("Slow percentage applied on hit. 0 = no slow, 0.5 = 50% slower.")]
        [Range(0f, 1f)]
        public float slowPercent = 0f;
        [Tooltip("Duration of the slow effect in seconds.")]
        public float slowDuration = 0f;

        [Header("On-Hit Area")]
        [Tooltip("Persistent area prefab spawned at impact (e.g. PoisonCloud). Null = none.")]
        public GameObject onHitAreaPrefab;

        [Header("Projectile Physics")]
        [Tooltip("Total seconds the arrow takes to reach the clicked target. Controls arc height.")]
        public float flightDuration = 1.5f;
        [Tooltip("Gravity multiplier applied to this arrow's Rigidbody2D.")]
        public float gravityScale = 1f;

        [Header("Cooldown")]
        [Tooltip("Seconds before this arrow slot is ready to fire again.")]
        public float cooldownTime = 1f;

        [Header("Shop")]
        [Tooltip("If true, the player owns this arrow from the start of every match. " +
                 "Set this only on the starter arrow (e.g. BasicArrow).")]
        public bool ownedByDefault = false;
        [Tooltip("Gold cost to purchase this arrow in the upgrade shop. Ignored if ownedByDefault.")]
        public int price = 20;
        [TextArea(1, 3)]
        [Tooltip("Short description shown in the upgrade shop tooltip.")]
        public string description = "";
    }
}
