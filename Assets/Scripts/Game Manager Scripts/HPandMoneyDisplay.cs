using System;
using UnityEngine;
using TMPro;

public class HPandMoneyDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI moneyText;
    private GameManager _gameManager;

    private void Start()
    {
        _gameManager = FindAnyObjectByType<GameManager>();
    }

    void Update()
    {
        hpText.text = $"HP: {_gameManager.HP:0}";
        moneyText.text = $"${_gameManager.money:0}";
    }
}