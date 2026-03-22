using ArrowWar.Data;
using UnityEditor;
using UnityEngine;

namespace ArrowWar.Editor
{
    /// <summary>
    /// Read-only inspector for PathData assets.
    /// Displays the waypoint list and draws the path as a yellow polyline in the
    /// Scene view when the asset is selected — no interactive editing.
    ///
    /// To edit a path visually, add a PathAuthoring component to a scene GameObject.
    /// </summary>
    [CustomEditor(typeof(PathData))]
    public class PathDataEditor : UnityEditor.Editor
    {
        private PathData _path;

        private static readonly Color ColLine   = new Color(1f, 0.85f, 0f, 1f);
        private static readonly Color ColStart  = new Color(0.2f, 1f, 0.4f, 1f);
        private static readonly Color ColEnd    = new Color(1f, 0.3f, 0.3f, 1f);
        private static readonly Color ColPoint  = new Color(0.9f, 0.9f, 0.9f, 1f);

        private void OnEnable() => _path = (PathData)target;

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "To edit this path visually, add a PathAuthoring component to a " +
                "scene GameObject, load this asset, edit, then save back.",
                MessageType.Info);

            EditorGUILayout.Space(4);
            DrawDefaultInspector();
        }

        private void OnSceneGUI()
        {
            if (_path == null || _path.waypoints == null || _path.waypoints.Count < 2) return;

            int count = _path.waypoints.Count;

            Handles.color = ColLine;
            for (int i = 0; i < count - 1; i++)
                Handles.DrawLine(Wp(i), Wp(i + 1));

            if (_path.loop)
                Handles.DrawLine(Wp(count - 1), Wp(0));

            for (int i = 0; i < count; i++)
            {
                Handles.color = i == 0 ? ColStart : (i == count - 1 && !_path.loop ? ColEnd : ColPoint);
                float size = HandleUtility.GetHandleSize(Wp(i)) * 0.08f;
                Handles.SphereHandleCap(0, Wp(i), Quaternion.identity, size, EventType.Repaint);
                Handles.Label(Wp(i) + Vector3.up * (size * 2.5f), i.ToString());
            }
        }

        private Vector3 Wp(int i) =>
            new Vector3(_path.waypoints[i].x, _path.waypoints[i].y, 0f);
    }
}
