using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Attach this to every grid tile GameObject.
/// Draws a translucent filled quad over the tile's top face for hover/selection feedback.
/// Compatible with the Universal Render Pipeline (URP).
/// </summary>
public class GridTile : MonoBehaviour
{
    [Header("Overlay Colors")]
    [SerializeField] private Color hoverColor    = new Color(1f, 1f, 0f, 0.4f); // translucent yellow
    [SerializeField] private Color selectedColor = new Color(0f, 1f, 0f, 0.4f); // translucent green

    [Header("Overlay Settings")]
    [Tooltip("How far above the tile surface the quad is drawn, to prevent z-fighting.")]
    [SerializeField] private float verticalOffset = 0.01f;

    [Tooltip("Fraction of the tile size to inset the quad on each edge (0 = full size).")]
    [SerializeField] [Range(0f, 0.45f)] private float inset = 0.02f;

    [Header("GL Material")]
    [Tooltip("Assign an Unlit/Color material. If left empty, one is created automatically.")]
    [SerializeField] private Material overlayMaterial;

    private bool isHovered  = false;
    private bool isSelected = false;

    private Camera mainCamera;

    // -----------------------------------------------------------------------
    // Lifecycle
    // -----------------------------------------------------------------------

    void Awake()
    {
        if (overlayMaterial == null)
        {
            overlayMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
            overlayMaterial.hideFlags = HideFlags.HideAndDontSave;
        }

        // Enable alpha blending so the translucent overlay shows the tile beneath.
        overlayMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        overlayMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        overlayMaterial.SetInt("_ZWrite", 0);
        overlayMaterial.DisableKeyword("_ALPHATEST_ON");
        overlayMaterial.EnableKeyword("_ALPHABLEND_ON");
        overlayMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        overlayMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }

    void Start()
    {
        mainCamera = Camera.main;

        if (mainCamera == null)
            Debug.LogWarning($"GridTile on '{gameObject.name}': No camera tagged 'MainCamera' found.");
    }

    void OnEnable()
    {
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    void OnDisable()
    {
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }

    // -----------------------------------------------------------------------
    // Rendering
    // -----------------------------------------------------------------------

    private void OnEndCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        if (mainCamera == null || cam != mainCamera)
            return;

        if (!isHovered && !isSelected)
            return;

        // Selected takes visual priority over hovered.
        DrawOverlayQuad(isSelected ? selectedColor : hoverColor);
    }

    private void DrawOverlayQuad(Color color)
    {
        Bounds bounds = GetComponent<Renderer>().bounds;

        float top   = bounds.max.y + verticalOffset;
        float left  = bounds.min.x + bounds.size.x * inset;
        float right = bounds.max.x - bounds.size.x * inset;
        float front = bounds.min.z + bounds.size.z * inset;
        float back  = bounds.max.z - bounds.size.z * inset;

        // GL.QUADS expects vertices in order: bottom-left, bottom-right,
        // top-right, top-left (viewed from above).
        Vector3 v0 = new Vector3(left,  top, front);
        Vector3 v1 = new Vector3(right, top, front);
        Vector3 v2 = new Vector3(right, top, back);
        Vector3 v3 = new Vector3(left,  top, back);

        GL.PushMatrix();

        try
        {
            overlayMaterial.SetPass(0);
            GL.LoadProjectionMatrix(mainCamera.projectionMatrix);
            GL.modelview = mainCamera.worldToCameraMatrix;

            GL.Begin(GL.QUADS);
            GL.Color(color);

            GL.Vertex(v0);
            GL.Vertex(v1);
            GL.Vertex(v2);
            GL.Vertex(v3);

            GL.End();
        }
        finally
        {
            GL.PopMatrix();
        }
    }

    // -----------------------------------------------------------------------
    // State — called by GridSelector
    // -----------------------------------------------------------------------

    /// <summary>Called by GridSelector when the mouse enters this tile.</summary>
    public void OnHoverEnter() => isHovered = true;

    /// <summary>Called by GridSelector when the mouse leaves this tile.</summary>
    public void OnHoverExit() => isHovered = false;

    /// <summary>Toggles selection state. Returns the new state.</summary>
    public bool ToggleSelected()
    {
        isSelected = !isSelected;
        return isSelected;
    }

    /// <summary>Deselects this tile externally (e.g. when another tile is picked).</summary>
    public void Deselect()
    {
        isSelected = false;
        isHovered  = false;
    }

    public bool IsSelected => isSelected;
}