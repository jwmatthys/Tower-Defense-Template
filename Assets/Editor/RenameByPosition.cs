using UnityEngine;
using UnityEditor;

public class RenameByPosition : EditorWindow
{
    [MenuItem("Tools/Rename Selected by Position")]
    public static void RenameSelectedObjects()
    {
        GameObject[] selected = Selection.gameObjects;

        if (selected.Length == 0)
        {
            EditorUtility.DisplayDialog(
                "Rename by Position",
                "No GameObjects selected. Please select one or more GameObjects and try again.",
                "OK"
            );
            return;
        }

        Undo.RecordObjects(selected, "Rename GameObjects by Position");

        int renamed = 0;
        foreach (GameObject go in selected)
        {
            Vector3 pos = go.transform.position;
            int x = Mathf.RoundToInt(pos.x);
            int z = Mathf.RoundToInt(pos.z);
            go.name = $"Node ({x},{z})";
            renamed++;
        }

        Debug.Log($"[Rename by Position] Renamed {renamed} GameObject(s).");
    }
}