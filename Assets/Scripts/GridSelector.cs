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

    void Update()
    {
        HandleHover();

        if (Input.GetMouseButtonDown(0)) // left-click
            HandleSelection();
    }

    // -----------------------------------------------------------------------
    // Hover — runs every frame
    // -----------------------------------------------------------------------

    private void HandleHover()
    {
        GridTile tile = RaycastToTile();

        if (tile == _hoveredTile)
            return; // nothing changed

        // Leave the old tile
        _hoveredTile?.OnHoverExit();

        // Enter the new tile (may be null if mouse is off the grid)
        _hoveredTile = tile;

        _hoveredTile?.OnHoverEnter();
    }

    // -----------------------------------------------------------------------
    // Selection — runs on click
    // -----------------------------------------------------------------------

    private void HandleSelection()
    {
        GridTile tile = RaycastToTile();

        if (!tile)
        {
            // Clicked empty space — deselect whatever is selected
            _selectedTile?.Deselect();
            _selectedTile = null;
            return;
        }

        if (tile == _selectedTile)
        {
            // Clicking the already-selected tile toggles it off
            tile.ToggleSelected();
            _selectedTile = null;
        }
        else
        {
            // Deselect the previous tile, select the new one
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

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, tileLayerMask))
            return hit.collider.GetComponent<GridTile>();

        return null;
    }

    // -----------------------------------------------------------------------
    // Extension point
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
    /// Public accessor in case other systems (e.g. a tower placement manager)
    /// need to query which tile is currently selected.
    /// </summary>
    public GridTile SelectedTile => _selectedTile;
}