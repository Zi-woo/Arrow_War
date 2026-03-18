using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowWar.Data
{
    [Serializable]
    public class SpawnEntry
    {
        public EnemyData enemyData;
        [Tooltip("Total number of this enemy type to spawn.")]
        public int count = 5;
        [Tooltip("Seconds between each individual spawn of this entry.")]
        public float spawnInterval = 2f;
    }

    [CreateAssetMenu(fileName = "NewRoundData", menuName = "ArrowWar/Round Data")]
    public class RoundData : ScriptableObject
    {
        [Tooltip("Delay in seconds before the first enemy spawns.")]
        public float startDelay = 1f;

        [Tooltip("Spawned sequentially. Each entry's enemies finish spawning before the next entry begins.")]
        public List<SpawnEntry> spawnEntries = new List<SpawnEntry>();
    }
}
