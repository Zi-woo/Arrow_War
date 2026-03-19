using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ArrowWar.Data;

namespace ArrowWar.UI
{
    /// <summary>
    /// Shown after each match ends. Uses pre-existing scene children — no runtime
    /// UI construction. Assign gridContainer and nextMatchButton in the Inspector.
    ///
    /// Hierarchy expected under this GameObject:
    ///   Panel
    ///     Title        (TextMeshProUGUI)
    ///     GoldHint     (TextMeshProUGUI)
    ///     Grid         (GridLayoutGroup)   ← assign as gridContainer
    ///     NextMatchBtn (Button)            ← assign as nextMatchButton
    ///       Label      (TextMeshProUGUI)
    /// </summary>
    public class UpgradePanel : MonoBehaviour
    {
        [SerializeField] private ArrowRegistry registry;
        [SerializeField] private Transform      gridContainer;
        [SerializeField] private Button         nextMatchButton;

        public event Action OnNextMatchClicked;

        private readonly List<UpgradeItemUI> _items = new List<UpgradeItemUI>();

        private void Start()
        {
            if (nextMatchButton != null)
                nextMatchButton.onClick.AddListener(HandleNextMatch);
        }

        /// <summary>Called by GameFlowManager at the start of each round transition.</summary>
        public void Show()
        {
            gameObject.SetActive(true);
            RefreshItems();
        }

        /// <summary>Called by UpgradeItemUI after a purchase so all items recheck affordability.</summary>
        public void NotifyPurchase()
        {
            foreach (var item in _items)
                item?.RefreshState();
        }

        private void RefreshItems()
        {
            foreach (var item in _items)
                if (item != null) Destroy(item.gameObject);
            _items.Clear();

            if (registry == null || registry.allArrows == null || gridContainer == null) return;

            foreach (var arrow in registry.allArrows)
            {
                if (arrow == null) continue;
                var go   = new GameObject(arrow.displayName);
                go.transform.SetParent(gridContainer, false);
                go.AddComponent<RectTransform>();
                var item = go.AddComponent<UpgradeItemUI>();
                item.Initialize(arrow, this);
                _items.Add(item);
            }
        }

        private void HandleNextMatch()
        {
            gameObject.SetActive(false);
            OnNextMatchClicked?.Invoke();
        }
    }
}
