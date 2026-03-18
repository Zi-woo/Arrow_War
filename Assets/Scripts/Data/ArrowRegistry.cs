using UnityEngine;

namespace ArrowWar.Data
{
    /// <summary>
    /// The single source of truth for every arrow type in the game.
    ///
    /// Workflow for adding a new arrow:
    ///   1. Create an ArrowData asset (Assets > Create > ArrowWar > Arrow Data).
    ///   2. Fill in the fields (prefab, icon, damage, cooldown, price, description).
    ///   3. Add it to allArrows in this asset.
    ///   4. Done — it will automatically appear in the upgrade panel and be purchasable.
    ///
    /// To make an arrow free / given at match start, set ownedByDefault = true on its ArrowData.
    /// </summary>
    [CreateAssetMenu(fileName = "ArrowRegistry", menuName = "ArrowWar/Arrow Registry")]
    public class ArrowRegistry : ScriptableObject
    {
        [Tooltip("Every arrow type in the game, in display order. " +
                 "Arrows with ownedByDefault = true are given free at match start. " +
                 "All others are available to purchase in the upgrade panel.")]
        public ArrowData[] allArrows;
    }
}
