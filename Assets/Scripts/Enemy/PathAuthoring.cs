using System.Collections.Generic;
using ArrowWar.Data;
using UnityEngine;

namespace ArrowWar.Enemy
{
    /// <summary>
    /// Scene-side authoring component for drawing and editing path waypoints visually.
    ///
    /// Workflow:
    ///   1. Add this component to any GameObject in the scene.
    ///   2. Select the GameObject — waypoint editing tools appear automatically.
    ///   3. Shift+Click in the Scene view to add waypoints.
    ///   4. Click Save to Asset / Create New Asset to persist to a PathData ScriptableObject.
    ///   5. Assign that PathData to the desired EnemyData.pathData field.
    ///
    /// This component is authoring-only. It carries no runtime behaviour and can be
    /// left in the scene or removed after baking data to the PathData asset.
    /// </summary>
    [AddComponentMenu("ArrowWar/Path Authoring")]
    public class PathAuthoring : MonoBehaviour
    {
        [Tooltip("The PathData asset to save to or load from.")]
        public PathData linkedPathData;

        [Tooltip("Working copy of waypoints. Edit in Scene view, then save to linked asset.")]
        public List<Vector2> waypoints = new List<Vector2>();

        public bool loop = false;
    }
}
