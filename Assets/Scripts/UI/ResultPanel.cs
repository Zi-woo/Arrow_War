using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ArrowWar.Core;

namespace ArrowWar.UI
{
    /// <summary>
    /// Attached to the ResultPanel scene GameObject (keep it inactive in the scene).
    /// GameFlowManager calls Show() on final-match Victory or Defeat,
    /// which activates this GameObject and sets the result text.
    /// </summary>
    public class ResultPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI resultText;

        private void Start()
        {
            // Runs once on first activation. Wire the restart button so no
            // Inspector onClick setup is needed.
            var btn = GetComponentInChildren<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => GameFlowManager.Instance?.RestartBattle());
        }

        /// <summary>
        /// Called by GameFlowManager. Activates the panel and displays the result.
        /// </summary>
        public void Show(bool victory)
        {
            gameObject.SetActive(true);
            if (resultText != null)
                resultText.text = victory ? "VICTORY!" : "DEFEAT";
        }
    }
}
