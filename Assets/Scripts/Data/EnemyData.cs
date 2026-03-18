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
        [Tooltip("Damage dealt to the player castle on contact.")]
        public int attackDamage = 10;
        public int goldReward = 5;
    }
}
