using UnityEngine;

/// <summary>
/// Attach this to every grid tile GameObject.
/// Provides hover and selection feedback by tinting the tile's material.
/// Caches the original color on Awake so it can always be restored cleanly.
/// </summary>
public class GridTile : MonoBehaviour
{
    [Header("Tint Colors")]
    [SerializeField] private Color hoverColor    = new Color(1f, 1f, 0f, 1f); // yellow
    [SerializeField] private Color selectedColor = new Color(0f, 1f, 0f, 1f); // green

    private bool isHovered  = false;
    private bool isSelected = false;

    private Renderer _renderer;
    private Color    _originalColor;

    // -----------------------------------------------------------------------
    // Lifecycle
    // -----------------------------------------------------------------------

    void Awake()
    {
        _renderer = GetComponent<Renderer>();

        if (_renderer == null)
        {
            Debug.LogWarning($"GridTile on '{gameObject.name}': No Renderer found.");
            return;
        }

        // Cache the original color so we can restore it later.
        // Using sharedMaterial to read the baseline; we write to a per-instance
        // material copy via _renderer.material below.
        _originalColor = _renderer.sharedMaterial.color;
    }

    // -----------------------------------------------------------------------
    // State — called by GridSelector
    // -----------------------------------------------------------------------

    /// <summary>Called by GridSelector when the mouse enters this tile.</summary>
    public void OnHoverEnter()
    {
        isHovered = true;
        RefreshTint();
    }

    /// <summary>Called by GridSelector when the mouse leaves this tile.</summary>
    public void OnHoverExit()
    {
        isHovered = false;
        RefreshTint();
    }

    /// <summary>Toggles selection state. Returns the new state.</summary>
    public bool ToggleSelected()
    {
        isSelected = !isSelected;
        RefreshTint();
        return isSelected;
    }

    /// <summary>Deselects this tile externally (e.g. when another tile is picked).</summary>
    public void Deselect()
    {
        isSelected = false;
        isHovered  = false;
        RefreshTint();
    }

    public bool IsSelected => isSelected;

    // -----------------------------------------------------------------------
    // Tinting
    // -----------------------------------------------------------------------

    private void RefreshTint()
    {
        if (_renderer == null) return;

        Color target;

        if (isSelected)
            target = selectedColor;         // selected takes priority over hovered
        else if (isHovered)
            target = hoverColor;
        else
            target = _originalColor;

        // _renderer.material creates a per-instance copy automatically,
        // so other tiles sharing the same material are unaffected.
        _renderer.material.color = target;
    }
}