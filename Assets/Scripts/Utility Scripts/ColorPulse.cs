using System.Collections;
using UnityEngine;

/// <summary>
/// Pulses a mesh Renderer to a chosen color and back with smooth easing.
/// Uses MaterialPropertyBlock so the shared material is never modified.
/// Call Pulse() from any other script on this GameObject.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class ColorPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    [Tooltip("The color to flash toward.")]
    public Color pulseColor = Color.white;

    [Tooltip("Total duration of one full pulse (rise + fall), in seconds.")]
    [Min(0.01f)]
    public float duration = 0.4f;

    [Tooltip("How much of the duration is spent rising to the pulse color (0–1). " +
             "0.3 means 30% rise, 70% fall.")]
    [Range(0.01f, 0.99f)]
    public float attackRatio = 0.3f;

    // ---------------------------------------------------------------

    private Renderer _renderer;
    private MaterialPropertyBlock _mpb;
    private Color _baseColor;
    private Coroutine _activeCoroutine;

    private static readonly int ColorPropID     = Shader.PropertyToID("_Color");
    private static readonly int BaseColorPropID = Shader.PropertyToID("_BaseColor");

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _mpb = new MaterialPropertyBlock();

        // Read the base color from the material so we always return to it correctly.
        // _BaseColor is used by URP/HDRP Lit shaders; _Color by Standard/Built-in.
        _baseColor = _renderer.sharedMaterial.HasProperty(BaseColorPropID)
            ? _renderer.sharedMaterial.GetColor(BaseColorPropID)
            : _renderer.sharedMaterial.GetColor(ColorPropID);
    }

    // ---------------------------------------------------------------
    // Public API

    /// <summary>Trigger a pulse using the color set in the Inspector.</summary>
    public void Pulse()
    {
        if (_activeCoroutine != null)
            StopCoroutine(_activeCoroutine);

        _activeCoroutine = StartCoroutine(DoPulse(pulseColor));
    }

    /// <summary>Trigger a pulse with a one-off color at runtime.</summary>
    public void Pulse(Color color)
    {
        if (_activeCoroutine != null)
            StopCoroutine(_activeCoroutine);

        _activeCoroutine = StartCoroutine(DoPulse(color));
    }

    // ---------------------------------------------------------------
    // Internals

    private IEnumerator DoPulse(Color target)
    {
        float attackTime = duration * attackRatio;
        float decayTime  = duration * (1f - attackRatio);

        // --- Rise (ease-out: fast start, slows near peak) ---
        float elapsed = 0f;
        while (elapsed < attackTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / attackTime);
            SetColor(Color.Lerp(_baseColor, target, EaseOutQuad(t)));
            yield return null;
        }
        SetColor(target);

        // --- Fall (ease-in: slow start, accelerates back to base) ---
        elapsed = 0f;
        while (elapsed < decayTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / decayTime);
            SetColor(Color.Lerp(target, _baseColor, EaseInQuad(t)));
            yield return null;
        }

        // Clear the property block so we leave no residue on the renderer
        _mpb.Clear();
        _renderer.SetPropertyBlock(_mpb);
        _activeCoroutine = null;
    }

    private void SetColor(Color color)
    {
        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetColor(ColorPropID, color);
        _mpb.SetColor(BaseColorPropID, color);
        _renderer.SetPropertyBlock(_mpb);
    }

    // ---------------------------------------------------------------
    // Easing helpers

    /// Starts fast, eases to a stop.
    private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);

    /// Starts slow, accelerates to finish.
    private static float EaseInQuad(float t) => t * t;
}