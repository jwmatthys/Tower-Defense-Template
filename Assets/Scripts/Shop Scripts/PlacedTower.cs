using UnityEngine;

/// <summary>
/// Placed on every instantiated tower. Tracks which TowerData it was built from
/// and which GridTile it occupies, so the shop UI can read costs and the placer
/// can free the tile on sell.
/// </summary>
public class PlacedTower : MonoBehaviour
{
    /// <summary>The ScriptableObject that describes this tower.</summary>
    public TowerData Data { get; private set; }

    /// <summary>The tile this tower is sitting on.</summary>
    public GridTile OccupiedTile { get; private set; }

    /// <summary>Called once immediately after the tower is instantiated.</summary>
    public void Initialize(TowerData data, GridTile tile)
    {
        Data         = data;
        OccupiedTile = tile;
    }
}
