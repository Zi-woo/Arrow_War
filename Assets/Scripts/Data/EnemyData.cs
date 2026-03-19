using UnityEngine;

namespace ArrowWar.Data
{
    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "ArrowWar/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        public string displayName = "Soldier";
        public GameObject prefab;

        [Header("Stats")]
        public int maxHP = 30;
        public float moveSpeed = 2f;
        [Tooltip("Damage dealt to the player castle per attack.")]
        public int attackDamage = 10;
        [Tooltip("Seconds between each castle attack while the enemy is stopped at the gate.")]
        public float attackInterval = 1.5f;
        public int goldReward = 5;
    }
}
