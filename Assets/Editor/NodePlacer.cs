using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class NodePlacer : EditorWindow
{
    private Transform pathParent;
    private Transform nodeParent;

    [MenuItem("Tools/Node Placer")]
    public static void ShowWindow()
    {
        GetWindow<NodePlacer>("Node Placer");
    }

    private static GameObject FindNodePrefab()
    {
        foreach (string guid in AssetDatabase.FindAssets("Node t:Prefab"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(path) == "Node")
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
        return null;
    }

    private void OnGUI()
    {
        GUILayout.Label("Node Placer", EditorStyles.boldLabel);
        GUILayout.Space(5);

        pathParent = (Transform)EditorGUILayout.ObjectField(
            "Path Parent", pathParent, typeof(Transform), true);

        nodeParent = (Transform)EditorGUILayout.ObjectField(
            "Node Parent (Optional)", nodeParent, typeof(Transform), true);

        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "Drag in the Path parent object, then click Place Nodes.",
            MessageType.Info);

        GUILayout.Space(5);

        int childCount = pathParent != null ? pathParent.childCount : 0;
        GUI.enabled = childCount > 0;

        if (GUILayout.Button("Place Nodes", GUILayout.Height(30)))
        {
            PlaceNodes();
        }

        GUI.enabled = true;

        GUILayout.Space(5);
        GUILayout.Label($"Path children: {childCount}", EditorStyles.miniLabel);
    }

    private void PlaceNodes()
    {
        if (pathParent == null || pathParent.childCount == 0)
        {
            EditorUtility.DisplayDialog("Node Placer", "Path parent has no children.", "OK");
            return;
        }

        GameObject nodePrefab = FindNodePrefab();
        if (nodePrefab == null)
        {
            EditorUtility.DisplayDialog("Node Placer",
                "Could not find a prefab named \"Node\" in the project.", "OK");
            return;
        }

        // Collect path positions and determine Y and bounding box
        HashSet<Vector2Int> pathPositions = new HashSet<Vector2Int>();
        float yPosition = pathParent.GetChild(0).position.y;
        int minX = int.MaxValue, maxX = int.MinValue;
        int minZ = int.MaxValue, maxZ = int.MinValue;

        foreach (Transform child in pathParent)
        {
            Vector3 pos = child.position;
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