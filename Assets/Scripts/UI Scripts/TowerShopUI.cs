using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drives the tower shop Canvas UI.
///
/// Hierarchy expected
/// ------------------
/// Canvas
///   └── ShopPanel                  ← assign to shopPanel
///         ├── TowerButtonContainer  ← assign to towerButtonContainer  (HorizontalLayoutGroup)
///         ├── StatusPanel           ← assign to statusPanel
///         │     ├── StatusText      ← assign to statusText  (TextMeshProUGUI)
///         │     ├── CancelButton    ← assign to cancelButton
///         │     ├── SellButton      ← assign to sellButton
///         │     └── UpgradeButton   ← assign to upgradeButton
///         └── TowerInfoPanel        ← assign to towerInfoPanel
///               ├── TowerNameText   ← assign to towerNameText
///               └── TowerDescText   ← assign to towerDescText
///
/// Assign TowerPlacer and all TowerData assets in the Inspector.
/// </summary>
public class TowerShopUI : MonoBehaviour
{
    // -----------------------------------------------------------------------
    // Inspector — wiring
    // -----------------------------------------------------------------------

    private TowerPlacer towerPlacer;

    [Header("Tower Types")]
    [Tooltip("All tower ScriptableObjects to show in the shop.")]
    [SerializeField] private List<TowerData> towerTypes = new();

    [Header("UI Panels")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform  towerButtonContainer;
    [SerializeField] private GameObject statusPanel;
    [SerializeField] private GameObject towerInfoPanel;

    [Header("Status Panel Children")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button          cancelButton;
    [SerializeField] private Button          sellButton;
    [SerializeField] private Button          upgradeButton;

    [Header("Tower Info Panel Children")]
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private TextMeshProUGUI towerDescText;
    [SerializeField] private TextMeshProUGUI towerCostText;

    [Header("Buy Button Template")]
    [Tooltip("Prefab with a Button component, an Image child for the icon, and a TextMeshProUGUI child for the label.")]
    [SerializeField] private GameObject towerButtonPrefab;

    [Header("Selection")]
    [Tooltip("How long to keep a selected tower visible before auto-deselecting.")]
    [SerializeField] private float selectionDisplayDuration = 3f;

    [Header("Status Messages")]
    [Tooltip("How long to show temporary status messages such as insufficient funds.")]
    [SerializeField] private float statusMessageDuration = 2f;

    private Coroutine _autoDeselectCoroutine;
    private Coroutine _statusMessageCoroutine;
    private string _statusTextBackup;

    // -----------------------------------------------------------------------
    // Lifecycle
    // -----------------------------------------------------------------------

    void Start()
    {
        towerPlacer = FindAnyObjectByType<GameManager>().GetComponent<TowerPlacer>();
        BuildTowerButtons();
        SetupStaticListeners();
        ShowIdle();
    }

    // -----------------------------------------------------------------------
    // Public state-display methods — called by TowerPlacer
    // -----------------------------------------------------------------------

    /// <summary>Default state: no tile selected, no pending action.</summary>
    // Tracks the indicator on whichever tower is currently selected.
    private RadiusIndicator      _activeIndicator;
    private AttackPatternDropdown _activeDropdown;

    // Patterns offered for targeting towers (not Area/Slow which have no target choice).
    private static readonly TowerAttack.AttackPattern[] TargetingPatterns =
    {
        TowerAttack.AttackPattern.AttackFirst,
        TowerAttack.AttackPattern.AttackLast,
        TowerAttack.AttackPattern.AttackClosest,
    };

    public void ShowIdle()
    {
        CancelAutoDeselect();
        CancelStatusMessage();
        HideActiveIndicator();
        HideActiveDropdown();
        SetSellUpgradeVisible(false);
        cancelButton.gameObject.SetActive(false);
        towerInfoPanel.SetActive(false);
        statusText.text = "Select a tile to place a tower.";
    }

    /// <summary>A tower type has been chosen; waiting for the player to click a tile.</summary>
    public void ShowPendingPlacement(TowerData data)
    {
        CancelAutoDeselect();
        CancelStatusMessage();
        HideActiveIndicator();
        HideActiveDropdown();
        SetSellUpgradeVisible(false);
        cancelButton.gameObject.SetActive(true);
        towerInfoPanel.SetActive(true);

        towerNameText.text = data.towerName;
        towerDescText.text = data.description;
        towerCostText.text = $"Cost: {data.buyCost} gold";
        statusText.text    = $"Click an available tile to place {data.towerName}.";
    }

    /// <summary>An occupied tile is selected; show sell / upgrade options for its tower.</summary>
    public void ShowOccupied(PlacedTower tower)
    {
        HideActiveIndicator();
        HideActiveDropdown();
        cancelButton.gameObject.SetActive(false);

        if (tower == null)
        {
            statusText.text = "Occupied tile (no tower data found).";
            SetSellUpgradeVisible(false);
            towerInfoPanel.SetActive(false);
            return;
        }

        TowerData data = tower.Data;

        towerInfoPanel.SetActive(true);
        towerNameText.text = $"{data.towerName} (Level {tower.Level})";
        towerDescText.text = data.description;

        // Show sell value
        int sellValue = tower.CurrentSellValue;
        string costText = $"Sell: {sellValue} gold";

        // Show upgrade cost if available
        if (tower.Level - 1 < data.upgrades.Count)
        {
            int upgradeCost = data.upgrades[tower.Level - 1].cost;
            costText += $"\nUpgrade: {upgradeCost} gold";
        }
        else
        {
            costText += "\nMax Level";
        }

        towerCostText.text = costText;
        statusText.text    = $"{data.towerName} Level {tower.Level} selected.";

        // Only show upgrade button if there are more upgrades available
        bool canUpgrade = tower.Level - 1 < data.upgrades.Count;
        sellButton.gameObject.SetActive(true);
        upgradeButton.gameObject.SetActive(canUpgrade);

        if (selectionDisplayDuration > 0f)
            StartAutoDeselectCountdown();

        CancelStatusMessage();

        TowerAttack attack = tower.GetComponent<TowerAttack>();
        if (attack != null)
        {
            // Radius indicator
            RadiusIndicator indicator = tower.GetComponent<RadiusIndicator>();
            if (indicator == null)
                indicator = tower.gameObject.AddComponent<RadiusIndicator>();

            indicator.Show(attack.GetCurrentAttackRadius());
            _activeIndicator = indicator;

            // Attack pattern dropdown — only for targeting patterns, not Area/Slow
            bool isTargetingTower = attack.attackPattern != TowerAttack.AttackPattern.Area
                                 && attack.attackPattern != TowerAttack.AttackPattern.Slow;
            if (isTargetingTower)
            {
                AttackPatternDropdown dropdown = tower.GetComponent<AttackPatternDropdown>();
                if (dropdown == null)
                    dropdown = tower.gameObject.AddComponent<AttackPatternDropdown>();

                dropdown.SetCanvasRoot(GetComponentInParent<Canvas>());
                dropdown.Show(TargetingPatterns);
                _activeDropdown = dropdown;
            }
        }
    }

    // -----------------------------------------------------------------------
    // Button builders
    // -----------------------------------------------------------------------

    private void BuildTowerButtons()
    {
        if (towerButtonPrefab == null)
        {
            Debug.LogError("[TowerShopUI] towerButtonPrefab is not assigned.");
            return;
        }

        foreach (TowerData data in towerTypes)
        {
            if (data == null) continue;

            GameObject go = Instantiate(towerButtonPrefab, towerButtonContainer);
            go.name = $"Btn_{data.towerName}";

            // Label — look for a TextMeshProUGUI anywhere in the button hierarchy
            TextMeshProUGUI label = go.GetComponentInChildren<TextMeshProUGUI>();
            if (label) label.text = $"{data.towerName}\n{data.buyCost}g";

            // Icon — look for an Image child (not the root button image)
            Image[] images = go.GetComponentsInChildren<Image>();
            if (data.icon != null && images.Length > 1)
                images[1].sprite = data.icon; // index 0 is usually the button background

            // Wire the click
            Button btn = go.GetComponent<Button>();
            if (btn)
            {
                TowerData captured = data; // capture for the lambda
                btn.onClick.AddListener(() => OnBuyButtonClicked(captured));
            }
        }
    }

    private void SetupStaticListeners()
    {
        cancelButton.onClick.AddListener(OnCancelClicked);
        sellButton.onClick.AddListener(OnSellClicked);
        upgradeButton.onClick.AddListener(OnUpgradeClicked);
    }

    // -----------------------------------------------------------------------
    // Button callbacks
    // -----------------------------------------------------------------------

    private void OnBuyButtonClicked(TowerData data)
    {
        towerPlacer.RequestPlace(data);
    }

    private void OnCancelClicked()
    {
        towerPlacer.CancelPending();
    }

    private void OnSellClicked()
    {
        towerPlacer.RequestSell();
    }

    private void OnUpgradeClicked()
    {
        towerPlacer.RequestUpgrade();
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private void SetSellUpgradeVisible(bool visible)
    {
        sellButton.gameObject.SetActive(visible);
        upgradeButton.gameObject.SetActive(visible);
    }

    private void StartAutoDeselectCountdown()
    {
        CancelAutoDeselect();
        _autoDeselectCoroutine = StartCoroutine(AutoDeselectRoutine());
    }

    private void CancelAutoDeselect()
    {
        if (_autoDeselectCoroutine != null)
        {
            StopCoroutine(_autoDeselectCoroutine);
            _autoDeselectCoroutine = null;
        }
    }

    private void CancelStatusMessage()
    {
        if (_statusMessageCoroutine != null)
        {
            StopCoroutine(_statusMessageCoroutine);
            _statusMessageCoroutine = null;
        }
    }

    public void ShowTemporaryStatus(string message)
    {
        CancelStatusMessage();
        _statusTextBackup = statusText.text;
        statusText.text = message;
        _statusMessageCoroutine = StartCoroutine(StatusMessageRoutine());
    }

    private IEnumerator StatusMessageRoutine()
    {
        yield return new WaitForSeconds(statusMessageDuration);
        statusText.text = _statusTextBackup;
        _statusMessageCoroutine = null;
    }

    private IEnumerator AutoDeselectRoutine()
    {
        yield return new WaitForSeconds(selectionDisplayDuration);
        towerPlacer?.DeselectTower();
    }

    private void HideActiveIndicator()
    {
        _activeIndicator?.Hide();
        _activeIndicator = null;
    }

    private void HideActiveDropdown()
    {
        _activeDropdown?.Hide();
        _activeDropdown = null;
    }
}