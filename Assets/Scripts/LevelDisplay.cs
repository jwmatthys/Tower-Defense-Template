using System.Collections;
using TMPro;
using UnityEngine;

public class LevelDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private float levelDisplayTime = 3f;
    void Start()
    {
        StartCoroutine(LevelDisplayCoroutine());
    }

    IEnumerator LevelDisplayCoroutine()
    {
        levelText.gameObject.SetActive(true);
        yield return new WaitForSeconds(levelDisplayTime);
        levelText.gameObject.SetActive(false);
    }
}
