using UnityEngine;

/// <summary>
/// Subclasses GridSelector to respond to tile selection.
/// Attach to the same GameManager object as GridSelector (replace GridSelector with this).
///
/// Workflow
/// --------
/// 1. Player clicks a tile → OnTileSelected fires.
/// 2. If the tile is Available and a tower type is pending → place the tower.
/// 3. If the tile is Occupied → show the sell / upgrade panel for that tower.
/// 4. TowerShopUI calls RequestPlace / RequestSell / RequestUpgrade on this component.
/// </summary>
public class TowerPlacer : GridSelector
{
    // -----------------------------------------------------------------------
    // Constants
    // -----------------------------------------------------------------------

    private const string TAG_AVAILABLE = "Available";
    private const string TAG_OCCUPIED  = "Occupied";

    // -----------------------------------------------------------------------
    // Inspector
    // -----------------------------------------------------------------------

    [Header("Shop UI")]
    [Tooltip("Assign the TowerShopUI component in the scene.")]
    [SerializeField] private TowerShopUI shopUI;

    // -----------------------------------------------------------------------
    // Runtime state
    // -----------------------------------------------------------------------

    /// <summary>Tower type queued for placement; set by TowerShopUI.</summary>
    private TowerData _pendingTower;

    /// <summary>The tower sitting on the currently selected occupied tile.</summary>
    private PlacedTower _selectedPlacedTower;

    // -----------------------------------------------------------------------
    // GridSelector override
    // -----------------------------------------------------------------------

    protected override void OnTileSelected(GridTile tile)
    {
        _selectedPlacedTower = null;

        if (tile == null)
        {
            shopUI?.ShowIdle();
            return;
        }

        if (tile.CompareTag(TAG_AVAILABLE) && _pendingTower != null)
        {
            PlaceTower(tile, _pendingTower);
            _pendingTower = null;
            shopUI?.ShowIdle();
        }
        else if (tile.CompareTag(TAG_OCCUPIED))
        {
            // Find the tower on this tile
            _selectedPlacedTower = FindTowerOnTile(tile);
            shopUI?.ShowOccupied(_selectedPlacedTower);
        }
        else
        {
            // Available tile but no tower selected, or untagged tile
            shopUI?.ShowIdle();
        }
    }

    // -----------------------------------------------------------------------
    // Public API — called by TowerShopUI
    // -----------------------------------------------------------------------

    /// <summary>
    /// Queue a tower type for placement. The next click on an Available tile
    /// will instantiate it.
    /// </summary>
    public void RequestPlace(TowerData data)
    {
        _pendingTower = data;
        shopUI?.ShowPendingPlacement(data);
        Debug.Log($"[TowerPlacer] Queued placement: {data.towerName}");
    }

    /// <summary>
    /// Sell the tower on the currently selected occupied tile. Placeholder:
    /// logs the action and destroys the tower object.
    /// </summary>
    public void RequestSell()
    {
        if (_selectedPlacedTower == null)
        {
            Debug.LogWarning("[TowerPlacer] RequestSell called but no tower is selected.");
            return;
        }

        GridTile tile = _selectedPlacedTower.OccupiedTile;
        int refund    = _selectedPlacedTower.Data.sellValue;

        Debug.Log($"[TowerPlacer] Sold {_selectedPlacedTower.Data.towerName} for {refund} gold. " +
                  "(Hook up your economy system here.)");

        Destroy(_selectedPlacedTower.gameObject);

        if (tile != null)
            tile.gameObject.tag = TAG_AVAILABLE;

        _selectedPlacedTower = null;
        shopUI?.ShowIdle();
    }

    /// <summary>
    /// Upgrade the tower on the currently selected occupied tile.
    /// Placeholder — logs the action.
    /// </summary>
    public void RequestUpgrade()
    {
        if (_selectedPlacedTower == null)
        {
            Debug.LogWarning("[TowerPlacer] RequestUpgrade called but no tower is selected.");
            return;
        }

        int cost = _selectedPlacedTower.Data.upgradeCost;
        Debug.Log($"[TowerPlacer] Upgrade requested for {_selectedPlacedTower.Data.towerName} " +
                  $"(cost: {cost} gold). Upgrade logic not yet implemented.");

        // TODO: implement upgrade logic (swap prefab, boost stats, etc.)
    }

    /// <summary>Cancel a pending placement (e.g. player pressed Escape).</summary>
    public void CancelPending()
    {
        _pendingTower = null;
        shopUI?.ShowIdle();
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    private void PlaceTower(GridTile tile, TowerData data)
    {
        if (data.prefab == null)
        {
            Debug.LogError($"[TowerPlacer] TowerData '{data.towerName}' has no prefab assigned.");
            return;
        }

        Vector3 spawnPos = tile.transform.position + Vector3.up * data.yOffset;
        GameObject go    = Instantiate(data.prefab, spawnPos, Quaternion.identity);

        PlacedTower placed = go.GetComponent<PlacedTower>();
        if (placed == null)
            placed = go.AddComponent<PlacedTower>();

        placed.Initialize(data, tile);

        tile.gameObject.tag = TAG_OCCUPIED;

        Debug.Log($"[TowerPlacer] Placed {data.towerName} at {spawnPos}.");
    }

    private static PlacedTower FindTowerOnTile(GridTile tile)
    {
        // Search all placed towers for one that references this tile.
        foreach (PlacedTower t in FindObjectsByType<PlacedTower>(FindObjectsSortMode.None))
        {
            if (t.OccupiedTile == tile)
                return t;
        }

        Debug.LogWarning($"[TowerPlacer] Tile '{tile.gameObject.name}' is tagged Occupied " +
                         "but no PlacedTower references it.");
        return null;
    }
}
