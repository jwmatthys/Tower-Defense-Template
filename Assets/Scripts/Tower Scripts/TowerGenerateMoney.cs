using System.Collections;
using UnityEngine;

public class TowerGenerateMoney : MonoBehaviour
{
    public int generatedMoney = 10;
    public float moneyInterval = 30f;

    void Start()
    {
        StartCoroutine(GenerateMoney());
    }

    IEnumerator GenerateMoney()
    {
        while (true)
        {
            yield return new WaitForSeconds(moneyInterval);
            EconomyManager.Instance?.GainMoney(generatedMoney);
        }
    }
}
