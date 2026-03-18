using UnityEngine;
using TMPro;

namespace ArrowWar.UI
{
    /// <summary>
    /// Round banners are no longer used (rounds advance automatically without banners).
    /// This component disables itself on Start to avoid any visual noise.
    /// </summary>
    public class RoundBannerUI : MonoBehaviour
    {
        [SerializeField] private GameObject bannerPanel;
        [SerializeField] private TextMeshProUGUI bannerText;

        private void Start()
        {
            if (bannerPanel != null)
                bannerPanel.SetActive(false);
        }
    }
}
