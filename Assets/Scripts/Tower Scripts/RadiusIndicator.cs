using UnityEngine;

/// <summary>
/// Draws a filled translucent circle on the XZ plane showing the tower's attack radius.
/// Uses a procedural mesh — assign a transparent URP Unlit material in the Inspector.
/// Added and controlled by TowerShopUI when a tower is selected.
/// </summary>
public class RadiusIndicator : MonoBehaviour
{
    [Tooltip("Number of triangle slices forming the circle. Higher = smoother.")]
    [SerializeField] private int segments = 64;

    [Tooltip("Additional world-space Y offset above the grid plane.")]
    [SerializeField] private float yOffset = 0.05f;

    [Tooltip("Fixed world-space Y position for the indicator disc on the grid plane.")]
    [SerializeField] private float gridPlaneY = 0.5f;

    [Tooltip("Transparent URP Unlit material to use for the disc.")]
    [SerializeField] private Material discMaterial;

    private GameObject   _discObject;
    private MeshFilter   _meshFilter;
    private MeshRenderer _meshRenderer;

    // -----------------------------------------------------------------------
    // Lifecycle
    // -----------------------------------------------------------------------

    void Awake()
    {
        if (discMaterial == null)
            Debug.LogWarning($"[RadiusIndicator] on '{gameObject.name}': No material assigned.");

        // Create a dedicated world-space object so the disc does not move with
        // the tower's model height or scale.
        _discObject = new GameObject("RadiusDisc");
        _discObject.transform.SetParent(null);
        _discObject.transform.position = new Vector3(transform.position.x, gridPlaneY + yOffset, transform.position.z);

        _meshFilter   = _discObject.AddComponent<MeshFilter>();
        _meshRenderer = _discObject.AddComponent<MeshRenderer>();

        _meshRenderer.material           = discMaterial;
        _meshRenderer.shadowCastingMode  = UnityEngine.Rendering.ShadowCastingMode.Off;
        _meshRenderer.receiveShadows     = false;
        _meshRenderer.enabled            = false;
    }

    void OnDestroy()
    {
        if (_discObject != null)
            Destroy(_discObject);
    }

    // -----------------------------------------------------------------------
    // Public API
    // -----------------------------------------------------------------------

    /// <summary>Show the filled disc with the given world-space radius.</summary>
    public void Show(float radius)
    {
        _meshFilter.mesh = BuildDiscMesh(radius);
        _meshRenderer.enabled = true;
    }

    /// <summary>Hide the disc.</summary>
    public void Hide()
    {
        _meshRenderer.enabled = false;
    }

    // -----------------------------------------------------------------------
    // Mesh generation
    // -----------------------------------------------------------------------

    private Mesh BuildDiscMesh(float radius)
    {
        Mesh mesh = new Mesh { name = "RadiusDisc" };

        // Build vertices in world space so the tower's scale doesn't affect the radius.
        // We then convert to local space of the disc child object.
        Vector3 worldCenter = _discObject.transform.position;
        Matrix4x4 worldToLocal = _discObject.transform.worldToLocalMatrix;

        int vertCount = segments + 1;
        Vector3[] verts = new Vector3[vertCount];
        verts[0] = worldToLocal.MultiplyPoint3x4(worldCenter); // centre in local space

        float angleStep = 2f * Mathf.PI / segments;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * angleStep;
            Vector3 worldPoint = worldCenter + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            verts[i + 1] = worldToLocal.MultiplyPoint3x4(worldPoint);
        }

        // Triangles: each slice is (centre, edgePoint[i], edgePoint[i+1]).
        int[] tris = new int[segments * 3];
        for (int i = 0; i < segments; i++)
        {
            tris[i * 3 + 0] = 0;
            tris[i * 3 + 1] = i + 1;
            tris[i * 3 + 2] = (i + 1) % segments + 1; // wraps last segment back to index 1
        }

        mesh.vertices  = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        return mesh;
    }
}