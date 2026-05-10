using System;
using UnityEngine;

/// <summary>
/// Subclasses GridSelector to respond to tile selection.
/// Attach to the same GameManager object as GridSelector (replace GridSelector with this).
///
/// Workflow
/// --------
/// 1. Player clicks a tile → OnTileSelected fires.
/// 2. If the tile is Available and a tower type is pending → place the tower.
/// 3. If the tile is Occupied, OR the player clicks directly on a tower mesh
///    → show the sell / upgrade panel for that tower.
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

    [Tooltip("Only these layers are considered when clicking on a placed tower. " +
             "Exclude the Enemy layer so enemies don't block tower selection.")]
    [SerializeField] private LayerMask towerLayerMask = ~0;

    // -----------------------------------------------------------------------
    // Runtime state
    // -----------------------------------------------------------------------

    /// <summary>Tower type queued for placement; set by TowerShopUI.</summary>
    private TowerData _pendingTower;

    /// <summary>The tower sitting on the currently selected occupied tile.</summary>
    private PlacedTower _selectedPlacedTower;

    /// <summary> The gameManager is in charge of the economy. </summary>
    private EconomyManager _economyManager;
    

    // -----------------------------------------------------------------------
    // HandleClick override — intercept clicks on tower meshes
    // -----------------------------------------------------------------------

    private void Awake()
    {
        _economyManager = EconomyManager.Instance ?? FindAnyObjectByType<EconomyManager>();
    }

    protected override void HandleClick()
    {
        // Don't process world clicks when the pointer is over a UI element
        // (e.g. the Sell / Upgrade / Cancel buttons).
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;

        PlacedTower tower = RaycastToPlacedTower();
        if (tower != null)
        {
            // Clicked directly on a tower mesh — select it without going through tile logic
            SelectPlacedTower(tower);
            return;
        }

        // No tower hit — let GridSelector handle tile selection normally
        base.HandleClick();
    }

    private PlacedTower RaycastToPlacedTower()
    {
        Camera cam = Camera.main;
        if (!cam) return null;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, towerLayerMask))
            return hit.collider.GetComponentInParent<PlacedTower>()
                ?? hit.collider.GetComponent<PlacedTower>();

        return null;
    }

    private void SelectPlacedTower(PlacedTower tower)
    {
        _selectedPlacedTower = tower;
        shopUI?.ShowOccupied(tower);
    }

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
            ClearSelection(); // deselect the tile so it doesn't stay highlighted
            shopUI?.ShowIdle();
        }
        else if (tile.CompareTag(TAG_OCCUPIED))
        {
            _selectedPlacedTower = FindTowerOnTile(tile);
            shopUI?.ShowOccupied(_selectedPlacedTower);
        }
        else
        {
            // Available tile but no tower pending, or untagged tile
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
        // If an available tile is already selected, place immediately.
        if (SelectedTile != null && SelectedTile.CompareTag(TAG_AVAILABLE))
        {
            PlaceTower(SelectedTile, data);
            ClearSelection();
            shopUI?.ShowIdle();
            return;
        }

        // Otherwise queue the tower and wait for the player to click a tile.
        _pendingTower = data;
        shopUI?.ShowPendingPlacement(data);
        Debug.Log($"[TowerPlacer] Queued placement: {data.towerName}");
    }

    /// <summary>
    /// Sell the tower on the currently selected occupied tile.
    /// </summary>
    public void RequestSell()
    {
        if (_selectedPlacedTower == null)
        {
            Debug.LogWarning("[TowerPlacer] RequestSell called but no tower is selected.");
            return;
        }

        GridTile tile = _selectedPlacedTower.OccupiedTile;
        int refund    = _selectedPlacedTower.CurrentSellValue;

        Debug.Log($"[TowerPlacer] Sold {_selectedPlacedTower.Data.towerName} for {refund} gold.");

        _economyManager.GainMoney(refund);
        
        Destroy(_selectedPlacedTower.gameObject);

        if (tile != null)
            tile.gameObject.tag = TAG_AVAILABLE;

        _selectedPlacedTower = null;
        shopUI?.ShowIdle();
    }

    /// <summary>
    /// Upgrade the tower on the currently selected occupied tile.
    /// </summary>
    public void RequestUpgrade()
    {
        if (_selectedPlacedTower == null)
        {
            Debug.LogWarning("[TowerPlacer] RequestUpgrade called but no tower is selected.");
            return;
        }

        TowerData data = _selectedPlacedTower.Data;
        int currentLevel = _selectedPlacedTower.Level;

        // Check if there are more upgrades available
        if (currentLevel - 1 >= data.upgrades.Count)
        {
            Debug.Log("[TowerPlacer] Tower is already at maximum upgrade level.");
            return;
        }

        // Get the cost for the next upgrade
        int cost = data.upgrades[currentLevel - 1].cost;

        if (!_economyManager.TrySpendGold(cost))
        {
            Debug.Log("[TowerPlacer] Not enough gold for upgrade.");
            shopUI?.ShowTemporaryStatus("Not enough money for upgrade");
            return;
        }

        // Upgrade the tower
        _selectedPlacedTower.Upgrade();

        // Apply stat upgrades to the tower components
        TowerAttack attack = _selectedPlacedTower.GetComponent<TowerAttack>();
        if (attack != null)
        {
            attack.ApplyUpgrades();
        }

        // Update the radius indicator if it exists
        RadiusIndicator indicator = _selectedPlacedTower.GetComponent<RadiusIndicator>();
        if (indicator != null)
        {
            indicator.Show(attack != null ? attack.GetCurrentAttackRadius() : 2f);
        }

        Debug.Log($"[TowerPlacer] Upgraded {_selectedPlacedTower.Data.towerName} to level {_selectedPlacedTower.Level} for {cost} gold.");

        // Refresh the UI to show updated stats
        shopUI?.ShowOccupied(_selectedPlacedTower);
    }

    /// <summary>Deselects the currently selected tower and returns the UI to idle state.</summary>
    public void DeselectTower()
    {
        _selectedPlacedTower = null;
        ClearSelection();
        shopUI?.ShowIdle();
    }

    /// <summary>Cancel a pending placement.</summary>
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

        if (data.buyCost > _economyManager.Money)
        {
            Debug.LogWarning($"[TowerPlacer] TowerData '{data.towerName}' cost exceeds available money.");
            shopUI?.ShowTemporaryStatus("Not enough money for tower");
            return;
        }
        
        _economyManager.SpendMoney(data.buyCost);
        Vector3 spawnPos = new Vector3(tile.transform.position.x, 1f, tile.transform.position.z);
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
        foreach (PlacedTower t in FindObjectsByType<PlacedTower>())
        {
            if (t.OccupiedTile == tile)
                return t;
        }

        Debug.LogWarning($"[TowerPlacer] Tile '{tile.gameObject.name}' is tagged Occupied " +
                         "but no PlacedTower references it.");
        return null;
    }
}