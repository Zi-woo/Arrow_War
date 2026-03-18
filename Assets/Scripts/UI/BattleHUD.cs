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
            resultPanel.SetActive(false);

            playerCastle.OnDamaged += HandleHPChanged;
            GoldManager.Instance.OnGoldChanged += HandleGoldChanged;

            restartButton.onClick.AddListener(() => GameFlowManager.Instance.RestartBattle());

            // Initialise display with current values.
            HandleHPChanged(playerCastle.CurrentHP, playerCastle.MaxHP);
            HandleGoldChanged(GoldManager.Instance.MatchGold);
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
            resultPanel.SetActive(true);
            resultText.text = victory ? "VICTORY!" : "DEFEAT";
        }
    }
}
