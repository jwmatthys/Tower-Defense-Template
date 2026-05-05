using System.Collections;
using UnityEngine;

/// <summary>
/// Pulses a SpriteRenderer to a chosen color and back with smooth easing.
/// Call Pulse() from any other script on this GameObject.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
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

    private SpriteRenderer _sr;
    private Color _baseColor;
    private Coroutine _activeCoroutine;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _baseColor = _sr.color;
    }

    // ---------------------------------------------------------------
    // Public API

    /// <summary>Trigger a pulse. Safe to call while one is already running.</summary>
    public void Pulse()
    {
        if (_activeCoroutine != null)
            StopCoroutine(_activeCoroutine);

        _activeCoroutine = StartCoroutine(DoPulse());
    }

    /// <summary>Trigger a pulse using a one-off color without changing the inspector value.</summary>
    public void Pulse(Color color)
    {
        if (_activeCoroutine != null)
            StopCoroutine(_activeCoroutine);

        _activeCoroutine = StartCoroutine(DoPulse(color));
    }

    // ---------------------------------------------------------------
    // Internals

    private IEnumerator DoPulse(Color? overrideColor = null)
    {
        Color target = overrideColor ?? pulseColor;
        float attackTime = duration * attackRatio;
        float decayTime  = duration * (1f - attackRatio);

        // --- Rise (ease-out: fast start, slows near peak) ---
        float elapsed = 0f;
        while (elapsed < attackTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / attackTime);
            _sr.color = Color.Lerp(_baseColor, target, EaseOutQuad(t));
            yield return null;
        }
        _sr.color = target;

        // --- Fall (ease-in: slow start, accelerates back to base) ---
        elapsed = 0f;
        while (elapsed < decayTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / decayTime);
            _sr.color = Color.Lerp(target, _baseColor, EaseInQuad(t));
            yield return null;
        }

        _sr.color = _baseColor;
        _activeCoroutine = null;
    }

    // ---------------------------------------------------------------
    // Easing helpers

    /// Starts fast, eases to a stop.
    private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);

    /// Starts slow, accelerates to finish.
    private static float EaseInQuad(float t) => t * t;
}