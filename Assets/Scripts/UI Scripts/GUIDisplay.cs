using System.Collections;
using UnityEngine;
using TMPro;

public class GUIDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private float levelDisplayTime = 4f;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip nextLevelSound;

    private EconomyManager _economyManager;
    private AudioSource _audioSource;

    private EconomyManager GetEconomyManager()
    {
        if (_economyManager == null)
            _economyManager = EconomyManager.Instance ?? FindAnyObjectByType<EconomyManager>();

        return _economyManager;
    }

    private void Start()
    {
        _economyManager = EconomyManager.Instance ?? FindAnyObjectByType<EconomyManager>();

        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.spatialBlend = 0f;
        _audioSource.playOnAwake = false;

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
        if (nextLevelSound != null) _audioSource.PlayOneShot(nextLevelSound);
        yield return new WaitForSeconds(levelDisplayTime);
        levelText.gameObject.SetActive(false);
    }

    public void ShowLevelClear()
    {
        StopAllCoroutines();
        levelText.text = "LEVEL CLEAR";
        levelText.gameObject.SetActive(true);
    }

    public void ShowGameOver()
    {
        StopAllCoroutines();
        levelText.text = "GAME OVER";
        levelText.gameObject.SetActive(true);
        if (gameOverSound != null) _audioSource.PlayOneShot(gameOverSound);
    }
}