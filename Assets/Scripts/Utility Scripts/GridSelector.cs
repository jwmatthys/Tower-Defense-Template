using UnityEngine;

/// <summary>
/// Manages mouse-over and click selection for grid tiles.
/// Attach to any persistent manager GameObject (e.g. "GameManager").
/// Requires a Camera in the scene tagged "MainCamera".
/// </summary>
public class GridSelector : MonoBehaviour
{
    [Header("Settings")] [Tooltip("Only GameObjects on these layers can be selected.")] [SerializeField]
    private LayerMask tileLayerMask = ~0; // default: all layers

    [Tooltip("How far the ray travels before giving up.")] [SerializeField]
    private float rayDistance = 100f;

    // Currently hovered and selected tiles
    private GridTile _hoveredTile;
    private GridTile _selectedTile;

    private Camera _mainCamera;

    // -----------------------------------------------------------------------
    // Lifecycle
    // -----------------------------------------------------------------------

    void Start()
    {
        _mainCamera = Camera.main;

        if (!_mainCamera)
            Debug.LogError("GridSelector: No camera tagged 'MainCamera' found in scene.");
    }

    // Update stays private so Unity's reflection finds exactly one copy.
    // Subclasses that need to intercept clicks should override HandleClick().
    void Update()
    {
        HandleHover();

        if (Input.GetMouseButtonDown(0))
            HandleClick();
    }

    // -----------------------------------------------------------------------
    // Hover — runs every frame
    // -----------------------------------------------------------------------

    private void HandleHover()
    {
        GridTile tile = RaycastToTile();

        if (tile == _hoveredTile)
            return; // nothing changed

        _hoveredTile?.OnHoverExit();
        _hoveredTile = tile;
        _hoveredTile?.OnHoverEnter();
    }

    // -----------------------------------------------------------------------
    // Click — override in subclasses to intercept before tile selection
    // -----------------------------------------------------------------------

    /// <summary>
    /// Called on left-click. Override in a subclass to intercept clicks before
    /// tile selection runs. Call base.HandleClick() to keep tile selection working.
    /// </summary>
    protected virtual void HandleClick()
    {
        HandleSelection();
    }

    // -----------------------------------------------------------------------
    // Selection
    // -----------------------------------------------------------------------

    private void HandleSelection()
    {
        GridTile tile = RaycastToTile();

        if (!tile)
        {
            ClearSelection();
            OnTileSelected(null);
            return;
        }

        if (tile == _selectedTile)
        {
            tile.ToggleSelected();
            _selectedTile = null;
        }
        else
        {
            _selectedTile?.Deselect();
            tile.ToggleSelected();
            _selectedTile = tile;
        }

        OnTileSelected(_selectedTile);
    }

    // -----------------------------------------------------------------------
    // Raycast helper
    // -----------------------------------------------------------------------

    private GridTile RaycastToTile()
    {
        if (!_mainCamera)
            return null;

        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        // Use RaycastAll so non-tile colliders (e.g. towers/enemies) don't block
        // selecting the grid tile underneath.
        RaycastHit[] hits = Physics.RaycastAll(ray, rayDistance, tileLayerMask);
        if (hits == null || hits.Length == 0)
            return null;

        GridTile closestTile = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            GridTile tile = hits[i].collider.GetComponent<GridTile>();
            if (tile == null) continue;

            if (hits[i].distance < closestDistance)
            {
                closestDistance = hits[i].distance;
                closestTile = tile;
            }
        }

        return closestTile;
    }

    // -----------------------------------------------------------------------
    // Extension points
    // -----------------------------------------------------------------------

    /// <summary>
    /// Called after every selection change. Override in a subclass to react to
    /// tile selection — e.g. placing or removing a tower — without modifying
    /// this base class (Open/Closed Principle).
    ///
    /// <paramref name="tile"/> is null when the selection is cleared.
    /// </summary>
    protected virtual void OnTileSelected(GridTile tile)
    {
        Debug.Log(tile
            ? $"Selected tile: {tile.gameObject.name} at {tile.transform.position}"
            : "Selection cleared.");
    }

    /// <summary>
    /// Programmatically deselects the currently selected tile and clears internal
    /// state. Call from subclasses when a selection should be cleared without a
    /// click (e.g. immediately after placing a tower).
    /// </summary>
    protected void ClearSelection()
    {
        _selectedTile?.Deselect();
        _selectedTile = null;
    }

    /// <summary>
    /// Public accessor in case other systems (e.g. a tower placement manager)
    /// need to query which tile is currently selected.
    /// </summary>
    public GridTile SelectedTile => _selectedTile;
}