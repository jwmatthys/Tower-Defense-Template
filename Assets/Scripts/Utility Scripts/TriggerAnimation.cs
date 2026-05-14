using System.Collections;
using UnityEngine;

/// <summary>
/// Plays a procedural squash-and-stretch and optional color flash.
/// Reads the object's own scale and material color at runtime, so the same
/// component works on every prefab variant — no Animator Controllers or
/// animation clips required, and no hardcoded per-variant values.
/// Call Play() from any gameplay script.
/// </summary>
public class TriggerAnimation : MonoBehaviour
{
    [Header("Scale Squash")]
    [Tooltip("Scale multipliers at the peak of the animation, relative to this object's starting scale. " +
             "e.g. (1.25, 0.75, 1.25) gives a classic squash-and-stretch.")]
    [SerializeField] private Vector3 peakScale = new Vector3(1.25f, 0.75f, 1.25f);

    [Header("Color Flash")]
    [Tooltip("Whether to also flash the renderer color during the animation.")]
    [SerializeField] private bool flashColor = false;

    [Tooltip("Color to flash toward. Only used when Flash Color is enabled.")]
    [SerializeField] private Color pulseColor = Color.white;

    [Header("Timing")]
    [Tooltip("Total duration of one full animation (rise + fall), in seconds.")]
    [Min(0.01f)]
    [SerializeField] private float duration = 0.3f;

    [Tooltip("Fraction of duration spent going toward the peak (0–1).")]
    [Range(0.01f, 0.99f)]
    [SerializeField] private float attackRatio = 0.3f;

    // -----------------------------------------------------------------------

    private Renderer _renderer;
    private MaterialPropertyBlock _mpb;
    private Vector3 _baseScale;
    private Color _baseColor;
    private Coroutine _activeCoroutine;

    private static readonly int ColorPropID     = Shader.PropertyToID("_Color");
    private static readonly int BaseColorPropID = Shader.PropertyToID("_BaseColor");

    private void Awake()
    {
        _baseScale = transform.localScale;
        _mpb = new MaterialPropertyBlock();

        if (flashColor)
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer == null)
                _renderer = GetComponentInChildren<Renderer>();

            if (_renderer != null)
            {
                _baseColor = _renderer.sharedMaterial.HasProperty(BaseColorPropID)
                    ? _renderer.sharedMaterial.GetColor(BaseColorPropID)
                    : _renderer.sharedMaterial.GetColor(ColorPropID);
            }
        }
    }

    /// <summary>Trigger the squash-and-stretch (and optional color flash).</summary>
    public void Play()
    {
        if (_activeCoroutine != null)
            StopCoroutine(_activeCoroutine);
        _activeCoroutine = StartCoroutine(Animate());
    }

    // -----------------------------------------------------------------------

    private IEnumerator Animate()
    {
        float attackTime  = duration * attackRatio;
        float decayTime   = duration * (1f - attackRatio);
        Vector3 targetScale = Vector3.Scale(_baseScale, peakScale);

        // --- Rise (ease-out) ---
        float elapsed = 0f;
        while (elapsed < attackTime)
        {
            elapsed += Time.deltaTime;
            float e = EaseOutQuad(Mathf.Clamp01(elapsed / attackTime));
            transform.localScale = Vector3.Lerp(_baseScale, targetScale, e);
            if (flashColor && _renderer != null)
                SetColor(Color.Lerp(_baseColor, pulseColor, e));
            yield return null;
        }
        transform.localScale = targetScale;
        if (flashColor && _renderer != null) SetColor(pulseColor);

        // --- Fall (ease-in) ---
        elapsed = 0f;
        while (elapsed < decayTime)
        {
            elapsed += Time.deltaTime;
            float e = EaseInQuad(Mathf.Clamp01(elapsed / decayTime));
            transform.localScale = Vector3.Lerp(targetScale, _baseScale, e);
            if (flashColor && _renderer != null)
                SetColor(Color.Lerp(pulseColor, _baseColor, e));
            yield return null;
        }

        transform.localScale = _baseScale;
        if (flashColor && _renderer != null)
        {
            _mpb.Clear();
            _renderer.SetPropertyBlock(_mpb);
        }
        _activeCoroutine = null;
    }

    private void SetColor(Color color)
    {
        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetColor(ColorPropID, color);
        _mpb.SetColor(BaseColorPropID, color);
        _renderer.SetPropertyBlock(_mpb);
    }

    private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
    private static float EaseInQuad(float t)  => t * t;
}
