using System.Collections.Generic;
using ArrowWar.Data;
using ArrowWar.Enemy;
using UnityEditor;
using UnityEngine;

namespace ArrowWar.Editor
{
    /// <summary>
    /// Scene view editor for PathAuthoring components.
    ///
    /// Selecting a GameObject that has PathAuthoring activates full waypoint editing:
    ///   Shift + Left-click   Add waypoint at cursor
    ///   Click a sphere       Select that waypoint
    ///   Drag handle          Move the selected waypoint
    ///   Delete key           Remove the selected waypoint
    ///
    /// Unity calls OnSceneGUI reliably for selected MonoBehaviours, so input is
    /// captured cleanly via HandleUtility.AddDefaultControl without the conflicts
    /// that affect ScriptableObject-based editors.
    /// </summary>
    [CustomEditor(typeof(PathAuthoring))]
    public class PathAuthoringEditor : UnityEditor.Editor
    {
        private PathAuthoring _authoring;
        private int _selectedIndex = -1;

        private static readonly Color ColLine     = new Color(1f, 0.85f, 0f, 1f);
        private static readonly Color ColLoopLine = new Color(1f, 0.85f, 0f, 0.45f);
        private static readonly Color ColPoint    = new Color(0.9f, 0.9f, 0.9f, 1f);
        private static readonly Color ColSelected = new Color(0.2f, 0.9f, 1f, 1f);
        private static readonly Color ColArrow    = new Color(1f, 0.65f, 0f, 0.85f);
        private static readonly Color ColStart    = new Color(0.2f, 1f, 0.4f, 1f);
        private static readonly Color ColEnd      = new Color(1f, 0.3f, 0.3f, 1f);

        private void OnEnable()
        {
            _authoring = (PathAuthoring)target;
        }

        private void OnDisable()
        {
            // Always restore transform tools when this component is deselected.
            Tools.hidden = false;
        }

        // ── Inspector ────────────────────────────────────────────────────────────

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox(
                "Scene editing is active while this GameObject is selected.\n" +
                "Shift+Click → add  |  Click sphere → select  |  Drag → move  |  Delete → remove",
                MessageType.Info);

            EditorGUILayout.Space(4);

            // ── Loop toggle
            EditorGUILayout.PropertyField(serializedObject.FindProperty("loop"));

            EditorGUILayout.Space(6);

            // ── Asset link
            EditorGUILayout.LabelField("Asset Link", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("linkedPathData"),
                new GUIContent("Linked PathData"));

            EditorGUILayout.Space(4);

            bool hasAsset = _authoring.linkedPathData != null;
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = hasAsset;
            if (GUILayout.Button("Save to Asset"))   SaveToAsset();
            if (GUILayout.Button("Load from Asset")) LoadFromAsset();
            GUI.enabled = true;
            if (GUILayout.Button("Create New Asset")) CreateNewAsset();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);

            // ── Waypoints list
            SerializedProperty wpList = serializedObject.FindProperty("waypoints");
            EditorGUILayout.LabelField($"Waypoints  ({wpList.arraySize})", EditorStyles.boldLabel);

            for (int i = 0; i < wpList.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();

                Color prev = GUI.backgroundColor;
                if (_selectedIndex == i) GUI.backgroundColor = ColSelected;
                if (GUILayout.Button(i.ToString(), GUILayout.Width(28)))
                {
                    _selectedIndex = i;
                    SceneView.RepaintAll();
                }
                GUI.backgroundColor = prev;

                EditorGUILayout.PropertyField(wpList.GetArrayElementAtIndex(i), GUIContent.none);

                GUI.backgroundColor = new Color(1f, 0.55f, 0.55f);
                if (GUILayout.Button("✕", GUILayout.Width(24)))
                {
                    Undo.RecordObject(_authoring, "Delete Waypoint");
                    _authoring.waypoints.RemoveAt(i);
                    if (_selectedIndex >= _authoring.waypoints.Count)
                        _selectedIndex = _authoring.waypoints.Count - 1;
                    EditorUtility.SetDirty(_authoring);
                    serializedObject.Update();
                    SceneView.RepaintAll();
                    EditorGUILayout.EndHorizontal();
                    break;
                }
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Add Waypoint"))
            {
                Undo.RecordObject(_authoring, "Add Waypoint");
                Vector2 next = _authoring.waypoints.Count > 0
                    ? _authoring.waypoints[_authoring.waypoints.Count - 1] + Vector2.left
                    : (Vector2)_authoring.transform.position;
                _authoring.waypoints.Add(next);
                _selectedIndex = _authoring.waypoints.Count - 1;
                EditorUtility.SetDirty(_authoring);
                SceneView.RepaintAll();
            }
            Color prevBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
            if (GUILayout.Button("Clear All", GUILayout.Width(80)))
            {
                if (EditorUtility.DisplayDialog("Clear Path", "Remove all waypoints?", "Yes", "Cancel"))
                {
                    Undo.RecordObject(_authoring, "Clear Waypoints");
                    _authoring.waypoints.Clear();
                    _selectedIndex = -1;
                    EditorUtility.SetDirty(_authoring);
                    SceneView.RepaintAll();
                }
            }
            GUI.backgroundColor = prevBg;
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
            SceneView.RepaintAll();
        }

        // ── Scene GUI ────────────────────────────────────────────────────────────

        private void OnSceneGUI()
        {
            if (_authoring == null) return;

            // ── Control ID ────────────────────────────────────────────────────
            // Called here (not in OnEnable) so the IMGUI sequential-ID system
            // assigns the same value during both the Layout and the MouseDown
            // event passes — that match is what makes AddDefaultControl work.
            int controlId = GUIUtility.GetControlID(FocusType.Passive);

            // Claim all unhandled mouse clicks. Any click that no waypoint handle
            // consumes routes to us rather than falling through to object selection.
            HandleUtility.AddDefaultControl(controlId);

            // Hide Unity's built-in transform gizmos so they don't compete for input.
            Tools.hidden = true;

            int count = _authoring.waypoints.Count;

            // ── Polyline ──────────────────────────────────────────────────────
            if (count >= 2)
            {
                Handles.color = ColLine;
                for (int i = 0; i < count - 1; i++)
                {
                    Handles.DrawLine(Wp(i), Wp(i + 1));
                    DrawArrow(Wp(i), Wp(i + 1));
                }

                if (_authoring.loop)
                {
                    Handles.color = ColLoopLine;
                    DrawDashedLine(Wp(count - 1), Wp(0));
                    DrawArrow(Wp(count - 1), Wp(0));
                }
            }

            // ── Waypoint spheres + move handles ───────────────────────────────
            for (int i = 0; i < count; i++)
            {
                bool isSelected = _selectedIndex == i;
                float size = HandleUtility.GetHandleSize(Wp(i)) * 0.12f;

                Handles.color = i == 0 ? ColStart
                    : (i == count - 1 && !_authoring.loop ? ColEnd : ColPoint);
                if (isSelected) Handles.color = ColSelected;

                if (Handles.Button(Wp(i), Quaternion.identity, size, size * 1.3f, Handles.SphereHandleCap))
                {
                    _selectedIndex = i;
                    Repaint();
                }

                string label = i == 0 ? $"[{i}] START"
                    : (i == count - 1 && !_authoring.loop ? $"[{i}] END" : $"[{i}]");
                Handles.Label(Wp(i) + Vector3.up * (size * 2f), label);

                if (isSelected)
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 moved = Handles.PositionHandle(Wp(i), Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_authoring, "Move Waypoint");
                        _authoring.waypoints[i] = new Vector2(moved.x, moved.y);
                        EditorUtility.SetDirty(_authoring);
                    }
                }
            }

            // ── Input ─────────────────────────────────────────────────────────
            Event e = Event.current;

            switch (e.type)
            {
                case EventType.MouseDown when e.button == 0:
                    if (e.shift)
                    {
                        Vector3 ray = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
                        Undo.RecordObject(_authoring, "Add Waypoint");
                        _authoring.waypoints.Add(new Vector2(ray.x, ray.y));
                        _selectedIndex = _authoring.waypoints.Count - 1;
                        EditorUtility.SetDirty(_authoring);
                        Repaint();
                    }
                    // Take hotControl regardless so drags and MouseUp stay with us.
                    GUIUtility.hotControl = controlId;
                    e.Use();
                    break;

                case EventType.MouseUp when e.button == 0 && GUIUtility.hotControl == controlId:
                    GUIUtility.hotControl = 0;
                    e.Use();
                    break;

                case EventType.MouseMove:
                    HandleUtility.Repaint();
                    break;

                case EventType.KeyDown when e.keyCode == KeyCode.Delete
                                         && _selectedIndex >= 0
                                         && _selectedIndex < _authoring.waypoints.Count:
                    Undo.RecordObject(_authoring, "Delete Waypoint");
                    _authoring.waypoints.RemoveAt(_selectedIndex);
                    _selectedIndex = Mathf.Clamp(_selectedIndex - 1, -1, _authoring.waypoints.Count - 1);
                    EditorUtility.SetDirty(_authoring);
                    e.Use();
                    Repaint();
                    break;
            }
        }

        // ── Asset operations ─────────────────────────────────────────────────────

        private void SaveToAsset()
        {
            PathData asset = _authoring.linkedPathData;
            Undo.RecordObject(asset, "Save Path to Asset");
            asset.waypoints = new List<Vector2>(_authoring.waypoints);
            asset.loop = _authoring.loop;
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            Debug.Log($"[PathAuthoring] Saved {asset.waypoints.Count} waypoints to '{asset.name}'.");
        }

        private void LoadFromAsset()
        {
            PathData asset = _authoring.linkedPathData;
            Undo.RecordObject(_authoring, "Load Path from Asset");
            _authoring.waypoints = new List<Vector2>(asset.waypoints);
            _authoring.loop = asset.loop;
            EditorUtility.SetDirty(_authoring);
            _selectedIndex = -1;
            SceneView.RepaintAll();
            Debug.Log($"[PathAuthoring] Loaded {_authoring.waypoints.Count} waypoints from '{asset.name}'.");
        }

        private void CreateNewAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create PathData Asset",
                "NewPathData",
                "asset",
                "Choose where to save the new PathData asset.",
                "Assets/Scripts/Data");

            if (string.IsNullOrEmpty(path)) return;

            PathData newAsset = CreateInstance<PathData>();
            newAsset.waypoints = new List<Vector2>(_authoring.waypoints);
            newAsset.loop = _authoring.loop;
            AssetDatabase.CreateAsset(newAsset, path);
            AssetDatabase.SaveAssets();

            Undo.RecordObject(_authoring, "Link PathData Asset");
            _authoring.linkedPathData = newAsset;
            EditorUtility.SetDirty(_authoring);

            Debug.Log($"[PathAuthoring] Created and linked '{path}'.");
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private Vector3 Wp(int i) =>
            new Vector3(_authoring.waypoints[i].x, _authoring.waypoints[i].y, 0f);

        private static void DrawDashedLine(Vector3 from, Vector3 to,
            float dashLen = 0.3f, float gapLen = 0.15f)
        {
            Vector3 dir = to - from;
            float total = dir.magnitude;
            if (total < 0.001f) return;
            Vector3 unit = dir / total;
            float traveled = 0f;
            bool drawing = true;
            while (traveled < total)
            {
                float segLen = drawing ? dashLen : gapLen;
                float end = Mathf.Min(traveled + segLen, total);
                if (drawing)
                    Handles.DrawLine(from + unit * traveled, from + unit * end);
                traveled = end;
                drawing = !drawing;
            }
        }

        private static void DrawArrow(Vector3 from, Vector3 to)
        {
            Vector3 mid = (from + to) * 0.5f;
            Vector3 dir = (to - from).normalized;
            if (dir == Vector3.zero) return;
            float size = HandleUtility.GetHandleSize(mid) * 0.18f;
            Handles.color = ColArrow;
            Handles.ConeHandleCap(0, mid, Quaternion.LookRotation(dir, Vector3.back), size, EventType.Repaint);
        }
    }
}
