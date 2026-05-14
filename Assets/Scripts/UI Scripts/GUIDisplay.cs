using System.Collections;
using UnityEngine;
using TMPro;

public class GUIDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private float levelDisplayTime = 4f;
    
    private EconomyManager _economyManager;

    private EconomyManager GetEconomyManager()
    {
        if (_economyManager == null)
            _economyManager = EconomyManager.Instance ?? FindAnyObjectByType<EconomyManager>();

        return _economyManager;
    }

    private void Start()
    {
        _economyManager = EconomyManager.Instance ?? FindAnyObjectByType<EconomyManager>();
        StartCoroutine(LevelDisplayCoroutine());
    }

    void Update()
    {
        EconomyManager economy = GetEconomyManager();
        if (economy == null) return;

        hpText.text = $"HP: {economy.HP:0}";
        moneyText.text = $"${economy.Money:0}";
    }
    
    IEnumerator LevelDisplayCoroutine()
    {
        EconomyManager economy = GetEconomyManager();
        if (economy == null) yield break;

        economy.level += 1;
        levelText.text = $"LEVEL {economy.level}";
        levelText.gameObject.SetActive(true);
        yield return new WaitForSeconds(levelDisplayTime);
        levelText.gameObject.SetActive(false);
    }
}