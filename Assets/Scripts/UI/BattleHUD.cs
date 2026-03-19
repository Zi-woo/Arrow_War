using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ArrowWar.Castle;
using ArrowWar.Economy;
using ArrowWar.Core;

namespace ArrowWar.UI
{
    /// <summary>
    /// Reads from Castle and GoldManager (via events) and updates the HUD.
    /// Also owns the result panel (Victory/Defeat overlay) and the restart button.
    /// Never modifies game state directly.
    /// </summary>
    public class BattleHUD : MonoBehaviour
    {
        [Header("Castle HP")]
        [SerializeField] private Slider hpSlider;
        [SerializeField] private TextMeshProUGUI hpText;

        [Header("Gold")]
        [SerializeField] private TextMeshProUGUI goldText;

        [Header("Result Overlay")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button restartButton;

        [Header("References")]
        [SerializeField] private Castle.Castle playerCastle;

        private void Start()
        {
            if (resultPanel == null)   Debug.LogError("[BattleHUD] 'resultPanel' is not assigned in the Inspector.", this);
            if (resultText == null)    Debug.LogError("[BattleHUD] 'resultText' is not assigned in the Inspector.", this);
            if (restartButton == null) Debug.LogError("[BattleHUD] 'restartButton' is not assigned in the Inspector.", this);
            if (playerCastle == null)  Debug.LogError("[BattleHUD] 'playerCastle' is not assigned in the Inspector.", this);

            resultPanel?.SetActive(false);

            if (playerCastle != null) playerCastle.OnDamaged += HandleHPChanged;
            if (GoldManager.Instance != null) GoldManager.Instance.OnGoldChanged += HandleGoldChanged;
            if (restartButton != null) restartButton.onClick.AddListener(() => GameFlowManager.Instance.RestartBattle());

            // Initialise display with current values.
            if (playerCastle != null) HandleHPChanged(playerCastle.CurrentHP, playerCastle.MaxHP);
            if (GoldManager.Instance != null) HandleGoldChanged(GoldManager.Instance.MatchGold);
        }

        private void OnDestroy()
        {
            if (playerCastle != null) playerCastle.OnDamaged -= HandleHPChanged;
            if (GoldManager.Instance != null) GoldManager.Instance.OnGoldChanged -= HandleGoldChanged;
        }

        private void HandleHPChanged(int current, int max)
        {
            if (hpSlider != null)
                hpSlider.value = max > 0 ? (float)current / max : 0f;

            if (hpText != null)
                hpText.text = $"{current} / {max}";
        }

        private void HandleGoldChanged(int gold)
        {
            if (goldText != null)
                goldText.text = $"Gold: {gold}";
        }

        /// <summary>Called by GameFlowManager when the match ends.</summary>
        public void ShowResult(bool victory)
        {
            if (resultPanel == null || resultText == null)
            {
                Debug.LogError("[BattleHUD] Cannot show result — resultPanel or resultText is not assigned in the Inspector.", this);
                return;
            }
            resultPanel.SetActive(true);
            resultText.text = victory ? "VICTORY!" : "DEFEAT";
        }
    }
}
