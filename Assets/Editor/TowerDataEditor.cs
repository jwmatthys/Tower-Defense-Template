using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(TowerData))]
public class TowerDataEditor : Editor
{
    private ReorderableList _upgradeList;
    private SerializedProperty _upgradesProp;

    private void OnEnable()
    {
        _upgradesProp = serializedObject.FindProperty("upgrades");

        _upgradeList = new ReorderableList(serializedObject, _upgradesProp,
            draggable: true, displayHeader: true, displayAddButton: true, displayRemoveButton: true);

        _upgradeList.drawHeaderCallback = rect =>
            EditorGUI.LabelField(rect, "Upgrades");

        _upgradeList.elementHeightCallback = index =>
        {
            SerializedProperty element = _upgradesProp.GetArrayElementAtIndex(index);
            float lh = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (!element.isExpanded)
                return lh + EditorGUIUtility.standardVerticalSpacing;

            TowerData data = (TowerData)target;
            // foldout + cost + sellValue = 3 lines always
            int lines = 3;
            if (data.towerType == TowerType.Money)
                lines += 3; // header + generatedMoney + moneyInterval
            else
                lines += 4; // header + 3 stat fields

            return lh * lines + EditorGUIUtility.singleLineHeight;
        };

        _upgradeList.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            TowerData data = (TowerData)target;
            SerializedProperty element = _upgradesProp.GetArrayElementAtIndex(index);

            float y   = rect.y + EditorGUIUtility.standardVerticalSpacing;
            float lh  = EditorGUIUtility.singleLineHeight;
            float step = lh + EditorGUIUtility.standardVerticalSpacing;

            // Foldout
            Rect foldoutRect = new Rect(rect.x + 10f, y, rect.width - 10f, lh);
            bool wasExpanded = element.isExpanded;
            element.isExpanded = EditorGUI.Foldout(foldoutRect, element.isExpanded, $"Upgrade {index + 1}", true);
            y += step;
            if (wasExpanded != element.isExpanded) Repaint();

            if (!element.isExpanded) return;

            // Always-visible fields
            DrawField(element, "cost",      "Cost",       rect, ref y);
            DrawField(element, "sellValue", "Sell Value", rect, ref y);

            switch (data.towerType)
            {
                case TowerType.Damage:
                    DrawSectionHeader("Target Stats", rect, ref y);
                    DrawField(element, "damage",         "Damage",          rect, ref y);
                    DrawField(element, "attackInterval", "Attack Interval", rect, ref y);
                    DrawField(element, "attackRadius",   "Attack Radius",   rect, ref y);
                    break;
                case TowerType.Slow:
                    DrawSectionHeader("Attack Settings", rect, ref y);
                    DrawField(element, "attackInterval", "Attack Interval", rect, ref y);
                    DrawField(element, "attackRadius",   "Attack Radius",   rect, ref y);
                    DrawField(element, "slowFactor",     "Slow Factor",     rect, ref y);
                    break;
                case TowerType.Money:
                    DrawSectionHeader("Money Generation", rect, ref y);
                    DrawField(element, "generatedMoney", "Generated Money", rect, ref y);
                    DrawField(element, "moneyInterval",  "Money Interval",  rect, ref y);
                    break;
            }
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        TowerData data = (TowerData)target;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("towerType"), new GUIContent("Tower Type"));

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("towerName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("prefab"));

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("buyCost"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("sellValue"));

        EditorGUILayout.Space();

        switch (data.towerType)
        {
            case TowerType.Damage:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("damage"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("attackInterval"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("attackRadius"));
                break;
            case TowerType.Slow:
                SectionLabel("Attack Settings");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("attackInterval"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("attackRadius"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("slowFactor"));
                break;
            case TowerType.Money:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("generatedMoney"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("moneyInterval"));
                break;
        }

        EditorGUILayout.Space();
        _upgradeList.DoLayoutList();

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("yOffset"));

        serializedObject.ApplyModifiedProperties();
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static void SectionLabel(string text) =>
        EditorGUILayout.LabelField(text, EditorStyles.boldLabel);

    private static void DrawSectionHeader(string text, Rect rect, ref float y)
    {
        float lh   = EditorGUIUtility.singleLineHeight;
        float step = lh + EditorGUIUtility.standardVerticalSpacing;
        EditorGUI.LabelField(new Rect(rect.x + 14f, y, rect.width - 14f, lh), text, EditorStyles.boldLabel);
        y += step;
    }

    private static void DrawField(SerializedProperty parent, string name, string label, Rect rect, ref float y)
    {
        float lh   = EditorGUIUtility.singleLineHeight;
        float step = lh + EditorGUIUtility.standardVerticalSpacing;
        EditorGUI.PropertyField(
            new Rect(rect.x + 14f, y, rect.width - 14f, lh),
            parent.FindPropertyRelative(name),
            new GUIContent(label));
        y += step;
    }
}
