using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Defines the target tower stats for a single upgrade level.
/// </summary>
[System.Serializable]
public class TowerUpgrade
{
    [Tooltip("Gold cost for this upgrade.")]
    public int cost = 100;

    [Header("Target Stats")]
    [Tooltip("Damage after this upgrade.")]
    public float damage = 1f;

    [Tooltip("Attack interval after this upgrade.")]
    public float attackInterval = 1f;

    [Tooltip("Attack radius after this upgrade.")]
    public float attackRadius = 2f;

    [Header("Slow Settings")]
    [Tooltip("Slow factor after this upgrade.")]
    public float slowFactor = 2f;

    [Tooltip("Slow duration after this upgrade.")]
    public float slowDuration = 5f;

    [Tooltip("Sell value after this upgrade.")]
    public int sellValue = 50;
}

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

    [Tooltip("Gold returned when this tower is sold.")]
    public int sellValue = 50;

    [Header("Base Stats")]
    [Tooltip("Base damage for this tower.")]
    public float damage = 1f;

    [Tooltip("Base attack interval for this tower.")]
    public float attackInterval = 1f;

    [Tooltip("Base attack radius for this tower.")]
    public float attackRadius = 2f;

    [Tooltip("Base slow factor for this tower.")]
    public float slowFactor = 2f;

    [Tooltip("Base slow duration for this tower.")]
    public float slowDuration = 5f;

    [Header("Upgrades")]
    [Tooltip("List of available upgrades for this tower. Each upgrade defines the target stats after that upgrade.")]
    public List<TowerUpgrade> upgrades = new();

    public int GetSellValueForLevel(int level)
    {
        if (level <= 1 || upgrades == null || upgrades.Count == 0)
            return sellValue;

        int index = level - 2;
        if (index >= 0 && index < upgrades.Count)
        {
            int value = upgrades[index].sellValue;
            return value > 0 ? value : sellValue;
        }

        return sellValue;
    }

}

