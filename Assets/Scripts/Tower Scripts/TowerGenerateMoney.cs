using System.Collections;
using UnityEngine;

public class TowerGenerateMoney : MonoBehaviour
{
    public int generatedMoney = 10;
    public float moneyInterval = 30f;
    private EconomyManager _economyManager;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _economyManager = FindAnyObjectByType<EconomyManager>();
        StartCoroutine(GenerateMoney());
    }

    IEnumerator GenerateMoney()
    {
        while (true)
        {
            yield return new WaitForSeconds(moneyInterval);
            _economyManager.GainMoney(generatedMoney);
        }
    }
}
