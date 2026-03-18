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
    /// Round flow (time-based):
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
    ///   - On Victory: upgrade panel opens; after Continue → Victory result shown.
    ///   - On Defeat: defeat result shown immediately.
    /// </summary>
    public class GameFlowManager : MonoBehaviour
    {
        public static GameFlowManager Instance { get; private set; }

        [Header("Scene References")]
        [SerializeField] private Castle.Castle playerCastle;
        [SerializeField] private EnemySpawner  enemySpawner;
        [SerializeField] private BattleHUD     battleHUD;

        [Header("Match Config")]
        [SerializeField] private MatchData matchData;

        [Header("Upgrade Shop")]
        [SerializeField] private UpgradePanel upgradePanel;
        [Tooltip("Seconds after all enemies are cleared before the upgrade panel opens.")]
        [SerializeField] private float upgradeOpenDelay = 0.8f;

        private GameState _currentState;
        private int _currentRoundIndex;

        public GameState CurrentState => _currentState;

        public event Action<GameState> OnStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
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
                    enemySpawner.StartRound(matchData.rounds[_currentRoundIndex]);
                    break;

                case GameState.Victory:
                    StartCoroutine(OpenUpgradePanelAfterDelay());
                    break;

                case GameState.Defeat:
                    battleHUD.ShowResult(false);
                    break;
            }
        }

        // ── Round chaining ────────────────────────────────────────────────────

        private void HandleRoundSpawnComplete()
        {
            if (_currentState != GameState.Battle) return;

            bool isLastRound = _currentRoundIndex >= matchData.rounds.Length - 1;

            if (isLastRound)
            {
                // All spawns scheduled — now wait for every enemy to die.
                enemySpawner.MarkLastRound();
            }
            else
            {
                _currentRoundIndex++;
                enemySpawner.StartRound(matchData.rounds[_currentRoundIndex]);
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
                upgradePanel.OnContinueClicked += HandleUpgradeContinue;
                upgradePanel.Show();
            }
            else
            {
                battleHUD.ShowResult(true);
            }
        }

        private void HandleUpgradeContinue()
        {
            upgradePanel.OnContinueClicked -= HandleUpgradeContinue;
            battleHUD.ShowResult(true);
        }

        public void RestartBattle()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
