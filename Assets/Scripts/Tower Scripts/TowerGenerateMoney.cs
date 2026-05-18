using System.Collections;
using UnityEngine;

public class TowerGenerateMoney : MonoBehaviour
{
    private int generatedMoney = 10;
    private float moneyInterval = 30f;

    private TriggerAnimation _triggerAnimation;
    private PlacedTower _placedTower;

    void Start()
    {
        _triggerAnimation = GetComponent<TriggerAnimation>();
        _placedTower = GetComponent<PlacedTower>();
        ApplyUpgrades();
    }

    public void ApplyUpgrades()
    {
        if (_placedTower == null)
            _placedTower = GetComponent<PlacedTower>();

        if (_placedTower != null && _placedTower.Data != null)
        {
            TowerData data = _placedTower.Data;

            if (data.generatedMoney > 0) generatedMoney = data.generatedMoney;
            if (data.moneyInterval > 0)  moneyInterval  = data.moneyInterval;

            int level = _placedTower.Level;
            if (level > 1)
            {
                int upgradeIndex = level - 2;
                if (upgradeIndex >= 0 && upgradeIndex < data.upgrades.Count)
                {
                    TowerUpgrade upgrade = data.upgrades[upgradeIndex];
                    if (upgrade.generatedMoney > 0) generatedMoney = upgrade.generatedMoney;
                    if (upgrade.moneyInterval  > 0) moneyInterval  = upgrade.moneyInterval;
                }
            }
        }

        StopAllCoroutines();
        StartCoroutine(GenerateMoney());
    }

    IEnumerator GenerateMoney()
    {
        while (true)
        {
            yield return new WaitForSeconds(moneyInterval);
            EconomyManager.Instance?.GainMoney(generatedMoney);
            _triggerAnimation?.Play();
        }
    }
}
