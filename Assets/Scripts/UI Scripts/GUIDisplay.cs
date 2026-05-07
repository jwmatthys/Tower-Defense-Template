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

    private void Start()
    {
        _economyManager = EconomyManager.Instance ?? FindAnyObjectByType<EconomyManager>();
        StartCoroutine(LevelDisplayCoroutine());
    }

    void Update()
    {
        hpText.text = $"HP: {_economyManager.HP:0}";
        moneyText.text = $"${_economyManager.Money:0}";
    }
    
    IEnumerator LevelDisplayCoroutine()
    {
        _economyManager.level += 1;
        levelText.text = $"LEVEL {_economyManager.level}";
        levelText.gameObject.SetActive(true);
        yield return new WaitForSeconds(levelDisplayTime);
        levelText.gameObject.SetActive(false);
    }
}