using System.Collections.Generic;
using UnityEngine;

namespace ArrowWar.Data
{
    /// <summary>
    /// Stores an ordered list of world-space waypoints that an enemy will travel through.
    /// Assign this to EnemyData.pathData to override the default left-only movement.
    ///
    /// Edit waypoints directly in the Scene view using the PathData custom editor:
    ///   • Enable "Edit Path" in the Inspector
    ///   • Shift+Click in the Scene to add a waypoint
    ///   • Click a waypoint sphere to select it, then drag to move
    ///   • Press Delete to remove the selected waypoint
    /// </summary>
    [CreateAssetMenu(fileName = "NewPathData", menuName = "ArrowWar/Path Data")]
    public class PathData : ScriptableObject
    {
        [Tooltip("Ordered world-space positions the enemy will walk through.")]
        public List<Vector2> waypoints = new List<Vector2>();

        [Tooltip("When true the enemy loops back to waypoint 0 after the last one.")]
        public bool loop = false;
    }
}
