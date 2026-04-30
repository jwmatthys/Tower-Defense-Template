using UnityEngine;

/// <summary>
/// ScriptableObject that defines a tower type.
/// Create via: Assets > Create > Tower Defense > Tower Data
/// </summary>
[CreateAssetMenu(fileName = "NewTowerData", menuName = "Tower Defense/Tower Data")]
public class TowerData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Display name shown in the shop UI.")]
    public string towerName = "Tower";

    [Tooltip("Short description shown in the shop UI.")]
    [TextArea(2, 4)]
    public string description = "A basic tower.";

    [Tooltip("Icon shown on the buy button.")]
    public Sprite icon;

    [Header("Prefab")]
    [Tooltip("The GameObject instantiated when this tower is placed.")]
    public GameObject prefab;

    [Header("Economy")]
    [Tooltip("Gold cost to place this tower.")]
    public int buyCost = 100;

    [Tooltip("Gold cost to upgrade this tower (placeholder).")]
    public int upgradeCost = 150;

    [Tooltip("Gold returned when this tower is sold.")]
    public int sellValue = 50;

    [Header("Placement")]
    [Tooltip("Y-axis offset from the grid tile's position when spawned.")]
    public float yOffset = 0.5f;
}
