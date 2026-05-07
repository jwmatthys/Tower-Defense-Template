using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Shows a dropdown on the screen-space UI Canvas for selecting a tower's attack pattern.
/// Positions itself near the tower's screen position each frame.
///
/// Setup
/// -----
/// 1. Create a UI > Dropdown - TextMeshPro inside your existing screen-space Canvas,
///    style it, then save it as a prefab. Delete it from the scene.
/// 2. Add this component to your tower prefabs (or let TowerShopUI add it at runtime).
/// 3. Assign the dropdown prefab and the scene's RectTransform canvas root in the Inspector.
///    (Or let TowerShopUI assign the canvas root via SetCanvasRoot() before calling Show().)
/// </summary>
public class AttackPatternDropdown : MonoBehaviour
{
    [Tooltip("Prefab of a TMP_Dropdown to instantiate on the screen-space Canvas.")]
    [SerializeField] private GameObject dropdownPrefab;

    [Tooltip("Offset in screen pixels from the tower's screen position.")]
    [SerializeField] private Vector2 screenOffset = new Vector2(0f, -60f);

    // Set by TowerShopUI before Show() is called.
    private RectTransform _canvasRect;
    private Canvas        _canvas;

    private GameObject   _instance;
    private RectTransform _instanceRect;
    private TMP_Dropdown  _dropdown;
    private TowerAttack   _attack;
    private Camera        _cam;

    private TowerAttack.AttackPattern[] _patternOptions;

    // -----------------------------------------------------------------------
    // Lifecycle
    // -----------------------------------------------------------------------

    void Awake()
    {
        _cam    = Camera.main;
        _attack = GetComponent<TowerAttack>();
    }

    void LateUpdate()
    {
        // Track the tower's screen position every frame while visible.
        if (_instance != null && _instance.activeSelf)
            RepositionToTower();
    }

    void OnDestroy()
    {
        if (_instance != null)
            Destroy(_instance);
    }

    // -----------------------------------------------------------------------
    // Public API
    // -----------------------------------------------------------------------

    /// <summary>
    /// Called by TowerShopUI to provide the screen-space canvas before Show().
    /// </summary>
    public void SetCanvasRoot(Canvas canvas)
    {
        _canvas     = canvas;
        _canvasRect = canvas.GetComponent<RectTransform>();
    }

    /// <summary>Show the dropdown offering the given attack patterns.</summary>
    public void Show(TowerAttack.AttackPattern[] options)
    {
        if (_attack == null) return;

        if (dropdownPrefab == null)
        {
            Debug.LogWarning($"[AttackPatternDropdown] on '{gameObject.name}': No dropdownPrefab assigned.");
            return;
        }

        if (_canvas == null)
        {
            Debug.LogWarning($"[AttackPatternDropdown] on '{gameObject.name}': No canvas root set. Call SetCanvasRoot() first.");
            return;
        }

        if (_instance == null)
        {
            _instance     = Instantiate(dropdownPrefab, _canvasRect);
            _instanceRect = _instance.GetComponent<RectTransform>();

            _dropdown = _instance.GetComponentInChildren<TMP_Dropdown>();
            if (_dropdown == null)
            {
                Debug.LogError("[AttackPatternDropdown] dropdownPrefab has no TMP_Dropdown component.");
                return;
            }
        }

        _patternOptions = options;

        _dropdown.ClearOptions();
        var labels = new System.Collections.Generic.List<string>();
        int currentIndex = 0;

        for (int i = 0; i < options.Length; i++)
        {
            labels.Add(FormatPattern(options[i]));
            if (options[i] == _attack.attackPattern)
                currentIndex = i;
        }

        _dropdown.AddOptions(labels);
        _dropdown.onValueChanged.RemoveAllListeners();
        _dropdown.SetValueWithoutNotify(currentIndex);
        _dropdown.RefreshShownValue();
        _dropdown.onValueChanged.AddListener(OnDropdownChanged);

        _instance.SetActive(true);
        RepositionToTower();
    }

    /// <summary>Hide the dropdown.</summary>
    public void Hide()
    {
        if (_instance != null)
            _instance.SetActive(false);
    }

    // -----------------------------------------------------------------------
    // Positioning
    // -----------------------------------------------------------------------

    private void RepositionToTower()
    {
        if (_cam == null || _instanceRect == null || _canvasRect == null) return;

        Vector2 screenPos = _cam.WorldToScreenPoint(transform.position);
        screenPos += screenOffset;

        // Convert screen position to canvas local position.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect, screenPos, _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _cam,
            out Vector2 localPoint);

        _instanceRect.anchoredPosition = localPoint;
    }

    // -----------------------------------------------------------------------
    // Callback
    // -----------------------------------------------------------------------

    private void OnDropdownChanged(int index)
    {
        if (_attack != null && _patternOptions != null && index < _patternOptions.Length)
            _attack.attackPattern = _patternOptions[index];
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static string FormatPattern(TowerAttack.AttackPattern pattern) => pattern switch
    {
        TowerAttack.AttackPattern.AttackFirst   => "Attack First",
        TowerAttack.AttackPattern.AttackLast    => "Attack Last",
        TowerAttack.AttackPattern.AttackClosest => "Attack Closest",
        TowerAttack.AttackPattern.Area          => "Area Attack",
        TowerAttack.AttackPattern.Slow          => "Slow Enemies",
        _                                       => pattern.ToString()
    };
}