using System;
using UnityEngine;
using TMPro;

public class HPandMoneyDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI moneyText;
    private EconomyManager _economyManager;

    private void Start()
    {
        _economyManager = FindAnyObjectByType<EconomyManager>();
    }

    void Update()
    {
        hpText.text = $"HP: {_economyManager.HP:0}";
        moneyText.text = $"${_economyManager.money:0}";
    }
}