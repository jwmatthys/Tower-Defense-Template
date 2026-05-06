using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class NodePlacer : EditorWindow
{
    private GameObject nodePrefab;
    private Transform nodeParent;

    [MenuItem("Tools/Node Placer")]
    public static void ShowWindow()
    {
        GetWindow<NodePlacer>("Node Placer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Node Placer", EditorStyles.boldLabel);
        GUILayout.Space(5);

        nodePrefab = (GameObject)EditorGUILayout.ObjectField(
            "Node Prefab", nodePrefab, typeof(GameObject), false);

        nodeParent = (Transform)EditorGUILayout.ObjectField(
            "Node Parent (Optional)", nodeParent, typeof(Transform), true);

        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "Select path cube GameObjects in the scene, then click Place Nodes.",
            MessageType.Info);

        GUILayout.Space(5);

        GUI.enabled = nodePrefab != null && Selection.gameObjects.Length > 0;

        if (GUILayout.Button("Place Nodes", GUILayout.Height(30)))
        {
            PlaceNodes();
        }

        GUI.enabled = true;

        GUILayout.Space(5);
        GUILayout.Label($"Selected objects: {Selection.gameObjects.Length}", EditorStyles.miniLabel);
    }

    private void PlaceNodes()
    {
        GameObject[] selected = Selection.gameObjects;

        if (selected.Length == 0)
        {
            EditorUtility.DisplayDialog("Node Placer", "No GameObjects selected.", "OK");
            return;
        }

        // Collect path positions and determine Y and bounding box
        HashSet<Vector2Int> pathPositions = new HashSet<Vector2Int>();
        float yPosition = selected[0].transform.position.y;
        int minX = int.MaxValue, maxX = int.MinValue;
        int minZ = int.MaxValue, maxZ = int.MinValue;

        foreach (GameObject go in selected)
        {
            Vector3 pos = go.transform.position;
            int x = Mathf.RoundToInt(pos.x);
            int z = Mathf.RoundToInt(pos.z);

            pathPositions.Add(new Vector2Int(x, z));

            if (x < minX) minX = x;
            if (x > maxX) maxX = x;
            if (z < minZ) minZ = z;
            if (z > maxZ) maxZ = z;
        }

        // Expand the shorter axis symmetrically so the grid is square,
        // with the path centered. When the difference is odd, the extra
        // cell goes on the high side (e.g. +0 low / +1 high).
        int spanX = maxX - minX + 1;
        int spanZ = maxZ - minZ + 1;
        int squareSize = Mathf.Max(spanX, spanZ);

        if (spanX < squareSize)
        {
            int deficit = squareSize - spanX;
            int padLow  = deficit / 2;
            int padHigh = deficit - padLow;
            minX -= padLow;
            maxX += padHigh;
        }

        if (spanZ < squareSize)
        {
            int deficit = squareSize - spanZ;
            int padLow  = deficit / 2;
            int padHigh = deficit - padLow;
            minZ -= padLow;
            maxZ += padHigh;
        }

        // Count how many nodes will be placed
        int nodeCount = 0;
        for (int x = minX; x <= maxX; x++)
            for (int z = minZ; z <= maxZ; z++)
                if (!pathPositions.Contains(new Vector2Int(x, z)))
                    nodeCount++;

        if (nodeCount == 0)
        {
            EditorUtility.DisplayDialog("Node Placer",
                "No empty squares found — the entire bounding area is occupied by path cubes.", "OK");
            return;
        }

        bool confirm = EditorUtility.DisplayDialog("Node Placer",
            $"This will place {nodeCount} Node(s) in a {squareSize}x{squareSize} square " +
            $"(X: {minX}–{maxX}, Z: {minZ}–{maxZ}) at Y={yPosition}.\n\nProceed?",
            "Place Nodes", "Cancel");

        if (!confirm) return;

        Undo.SetCurrentGroupName("Place Nodes");
        int undoGroup = Undo.GetCurrentGroup();

        int placed = 0;

        for (int x = minX; x <= maxX; x++)
        {
            for (int z = minZ; z <= maxZ; z++)
            {
                if (pathPositions.Contains(new Vector2Int(x, z)))
                    continue;

                Vector3 spawnPos = new Vector3(x, yPosition, z);

                GameObject node = (GameObject)PrefabUtility.InstantiatePrefab(nodePrefab);
                node.transform.position = spawnPos;
                node.name = $"Node ({x}, {z})";

                if (nodeParent != null)
                    node.transform.SetParent(nodeParent, true);

                Undo.RegisterCreatedObjectUndo(node, "Place Node");
                placed++;
            }
        }

        Undo.CollapseUndoOperations(undoGroup);

        Debug.Log($"[NodePlacer] Placed {placed} Node(s) in {squareSize}x{squareSize} square " +
                  $"X[{minX}..{maxX}] Z[{minZ}..{maxZ}] at Y={yPosition}.");

        EditorUtility.DisplayDialog("Node Placer", $"Done! Placed {placed} Node(s).", "OK");
    }
}