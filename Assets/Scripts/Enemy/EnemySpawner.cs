using System;
using System.Collections;
using UnityEngine;
using ArrowWar.Castle;
using ArrowWar.Data;
using ArrowWar.Economy;

namespace ArrowWar.Enemy
{
    /// <summary>
    /// Spawns enemies from RoundData assets and tracks living enemy count.
    ///
    /// Events:
    ///   OnSpawnComplete      — fired when the current round's spawn schedule finishes.
    ///                          GameFlowManager uses this to chain the next round.
    ///   OnAllEnemiesCleared  — fired when MarkLastRound() has been called AND alive
    ///                          count reaches 0. GameFlowManager uses this for Victory.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Tooltip("World position near the right castle where enemies appear.")]
        [SerializeField] private Transform spawnPoint;

        [Tooltip("The player castle that all spawned enemies will move toward.")]
        [SerializeField] private Castle.Castle playerCastle;

        private int  _aliveEnemyCount;
        private bool _lastRoundMarked;
        private bool _matchClearFired;

        /// <summary>Fired when a round's full spawn schedule has been issued.</summary>
        public event Action OnSpawnComplete;

        /// <summary>
        /// Fired once: when MarkLastRound() has been called AND all alive enemies are dead.
        /// </summary>
        public event Action OnAllEnemiesCleared;

        public void StartRound(RoundData roundData)
        {
            StopAllCoroutines();
            StartCoroutine(SpawnRoutine(roundData));
        }

        /// <summary>
        /// Called by GameFlowManager after starting the last round's spawn.
        /// From this point, reaching zero alive enemies triggers OnAllEnemiesCleared.
        /// </summary>
        public void MarkLastRound()
        {
            _lastRoundMarked = true;
            CheckMatchClear();
        }

        private IEnumerator SpawnRoutine(RoundData roundData)
        {
            yield return new WaitForSeconds(roundData.startDelay);

            foreach (SpawnEntry entry in roundData.spawnEntries)
            {
                for (int i = 0; i < entry.count; i++)
                {
                    SpawnEnemy(entry.enemyData);
                    yield return new WaitForSeconds(entry.spawnInterval);
                }
            }

            OnSpawnComplete?.Invoke();
        }

        private void SpawnEnemy(EnemyData data)
        {
            if (data == null || data.prefab == null)
            {
                Debug.LogWarning("[EnemySpawner] EnemyData or its prefab is null — skipping spawn.");
                return;
            }

            GameObject instance = Instantiate(data.prefab, spawnPoint.position, Quaternion.identity);
            EnemyUnit unit = instance.GetComponent<EnemyUnit>();

            if (unit == null)
            {
                Debug.LogError($"[EnemySpawner] Prefab '{data.prefab.name}' is missing EnemyUnit component.");
                Destroy(instance);
                return;
            }

            _aliveEnemyCount++;
            unit.Initialize(data, playerCastle);
            unit.OnDied += HandleEnemyDied;
        }

        private void HandleEnemyDied(int goldReward)
        {
            if (goldReward > 0)
                GoldManager.Instance?.EarnGold(goldReward);

            _aliveEnemyCount--;
            CheckMatchClear();
        }

        private void CheckMatchClear()
        {
            if (_matchClearFired) return;
            if (!_lastRoundMarked) return;
            if (_aliveEnemyCount > 0) return;

            _matchClearFired = true;
            OnAllEnemiesCleared?.Invoke();
        }
    }
}
