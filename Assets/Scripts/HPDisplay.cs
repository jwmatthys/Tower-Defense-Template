using UnityEngine;
using TMPro;

public class HPDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private GameManager gameManager;

    void Update()
    {
        hpText.text = $"HP: {gameManager.HP:0}";
    }
}