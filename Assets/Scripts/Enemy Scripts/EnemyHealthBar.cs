using UnityEngine;

/// <summary>
/// Renders a world-space health bar above an enemy by procedurally building
/// two quads — a dark background track and a colored fill — using
/// MaterialPropertyBlock so no shared materials are modified.
///
/// Setup:
///   1. Add this component to your enemy GameObject.
///   2. Assign a material that uses an unlit color shader
///      (e.g. the built-in "Unlit/Color", or URP's "Universal Render Pipeline/Unlit").
///   3. Tweak the fields in the Inspector.
///
/// The bar reads EnemyHealth.HP and EnemyHealth.GetMaxHealth() every frame.
/// </summary>
[RequireComponent(typeof(EnemyHealth))]
public class EnemyHealthBar : MonoBehaviour
{
    [Header("Layout")]
    [Tooltip("Center of the bar relative to this transform (world units).")]
    public Vector3 offset = new Vector3(0f, 1.2f, 0f);

    [Tooltip("Total width of the bar in world units.")]
    public float width = 1f;

    [Tooltip("Height of the bar in world units.")]
    public float height = 0.1f;

    [Header("Visuals")]
    [Tooltip("Unlit material used for both the background and fill quads.")]
    public Material barMaterial;

    [Tooltip("Color of the fill that represents remaining health.")]
    public Color fillColor = Color.green;

    [Tooltip("Color of the empty background track.")]
    public Color backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);

    [Tooltip("How much smaller the background is on each side (world units). " +
             "Set to 0 for no border.")]
    public float borderThickness = 0.01f;

    [Tooltip("Hide the bar when the enemy is at full health.")]
    public bool hideWhenFull = true;

    // ---------------------------------------------------------------

    private EnemyHealth _health;

    private GameObject _bgObj;
    private GameObject _fillObj;
    private MeshRenderer _fillRenderer;
    private Transform _fillTransform;

    private static readonly int ColorPropID     = Shader.PropertyToID("_Color");
    private static readonly int BaseColorPropID = Shader.PropertyToID("_BaseColor");

    private float _maxHealth;
    private float _fillFullWidth;   // cached: width of fill quad at 100 % health
    private float _fillOriginX;     // cached: local x of fill quad's left edge

    // ---------------------------------------------------------------

    private void Awake()
    {
        _health = GetComponent<EnemyHealth>();
        _maxHealth = _health.GetMaxHealth();

        BuildQuads();
        CacheLayout();
    }

    private void LateUpdate()
    {
        float fraction = Mathf.Clamp01(_health.HP / _maxHealth);

        // Show/hide
        bool showBar = !(hideWhenFull && fraction >= 1f);
        _bgObj.SetActive(showBar);
        _fillObj.SetActive(showBar);

        if (!showBar) return;

        // Scale fill quad horizontally around its left edge
        // so it shrinks right-to-left as health drops.
        Vector3 s = _fillTransform.localScale;
        s.x = fraction;
        _fillTransform.localScale = s;

        // Shift the fill so its left edge stays fixed
        Vector3 p = _fillTransform.localPosition;
        p.x = _fillOriginX + (_fillFullWidth * fraction * 0.5f);
        _fillTransform.localPosition = p;
    }

    // ---------------------------------------------------------------
    // Construction

    private void BuildQuads()
    {
        // Background track (slightly inset by border)
        float bgWidth  = width  - borderThickness * 2f;
        float bgHeight = height - borderThickness * 2f;

        _bgObj   = CreateQuad("HealthBar_BG",   width,   height,  backgroundColor);
        var bgRenderer = _bgObj.GetComponent<MeshRenderer>();

        // Fill quad — same size as bg track, scaled in LateUpdate
        _fillObj      = CreateQuad("HealthBar_Fill", bgWidth, bgHeight, fillColor);
        _fillRenderer = _fillObj.GetComponent<MeshRenderer>();
        _fillTransform = _fillObj.transform;

        // Render fill on top of background
        bgRenderer.sortingOrder    = 1;
        _fillRenderer.sortingOrder = 2;
    }

    private void CacheLayout()
    {
        // The fill quad at full health sits centered on the offset position.
        // Its left edge is at:  offset.x - (bgWidth / 2)
        float bgWidth    = width - borderThickness * 2f;
        _fillFullWidth   = bgWidth;
        _fillOriginX     = -(bgWidth / 2f);   // local x of left edge (parent space)

        // Position fill's pivot to the left edge at start
        _fillTransform.localPosition = new Vector3(
            _fillOriginX + _fillFullWidth * 0.5f,
            offset.y,
            offset.z - 0.001f   // just in front of background
        );
    }

    private GameObject CreateQuad(string objName, float w, float h, Color color)
    {
        var obj = new GameObject(objName);
        obj.transform.SetParent(transform, false);
        obj.transform.localPosition = offset;

        // Mesh
        var mf   = obj.AddComponent<MeshFilter>();
        var mr   = obj.AddComponent<MeshRenderer>();
        mf.mesh  = BuildQuadMesh(w, h);
        mr.sharedMaterial = barMaterial;
        mr.shadowCastingMode  = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows     = false;

        var mpb = new MaterialPropertyBlock();
        ApplyColor(mr, mpb, color);

        return obj;
    }

    private static Mesh BuildQuadMesh(float w, float h)
    {
        float hw = w * 0.5f;
        float hh = h * 0.5f;

        var mesh = new Mesh();
        mesh.vertices = new[]
        {
            new Vector3(-hw, -hh, 0f),
            new Vector3( hw, -hh, 0f),
            new Vector3(-hw,  hh, 0f),
            new Vector3( hw,  hh, 0f),
        };
        mesh.triangles = new[] { 0, 2, 1,  2, 3, 1 };
        mesh.uv = new[]
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
        };
        mesh.RecalculateNormals();
        return mesh;
    }

    private static void ApplyColor(MeshRenderer mr, MaterialPropertyBlock mpb, Color color)
    {
        mr.GetPropertyBlock(mpb);
        mpb.SetColor(ColorPropID,     color);
        mpb.SetColor(BaseColorPropID, color);
        mr.SetPropertyBlock(mpb);
    }

    // ---------------------------------------------------------------

    private void OnDestroy()
    {
        // Clean up procedural meshes to avoid memory leaks
        if (_bgObj   != null) Destroy(_bgObj.GetComponent<MeshFilter>().mesh);
        if (_fillObj  != null) Destroy(_fillObj.GetComponent<MeshFilter>().mesh);
    }
}