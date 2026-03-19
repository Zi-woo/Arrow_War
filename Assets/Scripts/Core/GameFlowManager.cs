using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using ArrowWar.Data;
using ArrowWar.Castle;
using ArrowWar.Enemy;
using ArrowWar.UI;

namespace ArrowWar.Core
{
    /// <summary>
    /// Owns the match state machine.
    ///
    /// Match flow:
    ///   - Matches progress sequentially through the 'matches' array.
    ///   - _currentMatchIndex is static so it survives the scene reload triggered by Next Match.
    ///   - RestartBattle resets it to 0 (full restart from match 1).
    ///
    /// Round flow (time-based, within a match):
    ///   - Each round's spawn schedule runs; when it finishes, the next round starts immediately.
    ///   - Enemies from earlier rounds persist — alive count is never reset between rounds.
    ///
    /// Victory condition:
    ///   - All rounds' spawn schedules have completed AND all enemies are dead.
    ///
    /// Defeat condition:
    ///   - Player castle HP reaches 0 at any time.
    ///
    /// Post-match:
    ///   - On Victory: upgrade panel opens; Continue → Victory result; Next Match → loads next match.
    ///   - On Defeat: defeat result shown immediately.
    /// </summary>
    public class GameFlowManager : MonoBehaviour
    {
        public static GameFlowManager Instance { get; private set; }

        [Header("Scene References")]
        [SerializeField] private Castle.Castle playerCastle;
        [SerializeField] private EnemySpawner  enemySpawner;
        [SerializeField] private ResultPanel   resultPanel;

        [Header("Match Config")]
        [Tooltip("Sequential list of matches. The game progresses through them in order.")]
        [SerializeField] private MatchData[] matches;

        [Header("Upgrade Shop")]
        [SerializeField] private UpgradePanel upgradePanel;
        [Tooltip("Seconds after all enemies are cleared before the upgrade panel opens.")]
        [SerializeField] private float upgradeOpenDelay = 0.8f;

        // Static: survives LoadScene so the match index carries over after "Next Match".
        // Resets to 0 on domain reload (editor stop/play, or fresh launch).
        private static int _currentMatchIndex = 0;

        private GameState _currentState;
        private int       _currentRoundIndex;

        public GameState CurrentState      => _currentState;
        public int       CurrentMatchIndex => _currentMatchIndex;
        public int       MatchCount        => matches != null ? matches.Length : 0;

        private MatchData CurrentMatch => matches[_currentMatchIndex];

        public event Action<GameState> OnStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            if (matches == null || matches.Length == 0)
            {
                Debug.LogError("[GameFlowManager] 'matches' array is empty. Assign at least one MatchData in the Inspector.");
                return;
            }

            // Clamp in case the static index is stale (e.g. a match was removed in the editor).
            _currentMatchIndex = Mathf.Clamp(_currentMatchIndex, 0, matches.Length - 1);

            playerCastle.OnDestroyed          += HandleCastleDestroyed;
            enemySpawner.OnSpawnComplete       += HandleRoundSpawnComplete;
            enemySpawner.OnAllEnemiesCleared   += HandleAllEnemiesCleared;

            _currentRoundIndex = 0;
            TransitionTo(GameState.Battle);
        }

        private void OnDestroy()
        {
            if (playerCastle != null) playerCastle.OnDestroyed          -= HandleCastleDestroyed;
            if (enemySpawner != null)
            {
                enemySpawner.OnSpawnComplete     -= HandleRoundSpawnComplete;
                enemySpawner.OnAllEnemiesCleared -= HandleAllEnemiesCleared;
            }
        }

        private void TransitionTo(GameState newState)
        {
            _currentState = newState;
            OnStateChanged?.Invoke(_currentState);

            switch (_currentState)
            {
                case GameState.Battle:
                    enemySpawner.StartRound(CurrentMatch.rounds[_currentRoundIndex]);
                    break;

                case GameState.Victory:
                    bool isFinalMatch = _currentMatchIndex >= matches.Length - 1;
                    if (isFinalMatch)
                        resultPanel?.Show(true);
                    else
                        StartCoroutine(OpenUpgradePanelAfterDelay());
                    break;

                case GameState.Defeat:
                    resultPanel?.Show(false);
                    break;
            }
        }

        // ── Round chaining ────────────────────────────────────────────────────

        private void HandleRoundSpawnComplete()
        {
            if (_currentState != GameState.Battle) return;

            bool isLastRound = _currentRoundIndex >= CurrentMatch.rounds.Length - 1;

            if (isLastRound)
            {
                // All spawns scheduled — now wait for every enemy to die.
                enemySpawner.MarkLastRound();
            }
            else
            {
                _currentRoundIndex++;
                enemySpawner.StartRound(CurrentMatch.rounds[_currentRoundIndex]);
            }
        }

        // ── Victory/Defeat ────────────────────────────────────────────────────

        private void HandleAllEnemiesCleared()
        {
            if (_currentState == GameState.Defeat) return;
            TransitionTo(GameState.Victory);
        }

        private void HandleCastleDestroyed()
        {
            if (_currentState == GameState.Victory || _currentState == GameState.Defeat) return;
            StopAllCoroutines();
            TransitionTo(GameState.Defeat);
        }

        // ── Post-match upgrade flow ───────────────────────────────────────────

        private IEnumerator OpenUpgradePanelAfterDelay()
        {
            yield return new WaitForSeconds(upgradeOpenDelay);

            if (upgradePanel != null)
            {
                upgradePanel.OnNextMatchClicked += HandleNextMatch;
                upgradePanel.Show();
            }
        }

        private void HandleNextMatch()
        {
            upgradePanel.OnNextMatchClicked -= HandleNextMatch;

            // Advance to next match, clamped to the last available match.
            _currentMatchIndex = Mathf.Min(_currentMatchIndex + 1, matches.Length - 1);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>Reloads the scene from the beginning of match 1, resetting all progression.</summary>
        public void RestartBattle()
        {
            _currentMatchIndex = 0;
            Economy.ArrowInventory.ClearPersistedOwned();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
