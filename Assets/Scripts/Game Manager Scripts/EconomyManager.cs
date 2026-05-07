using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    public int HP { get; private set; } = 100;
    public int Money { get; private set; } = 100;
    public int Level { get; private set; } = 1;
    
    [Tooltip("How long the Game Over message should appear before game resets.")]
    [SerializeField] private float gameOverDelay = 5f;

    [SerializeField] private bool overrideStartingValues;
    [SerializeField] private int customStartingMoney = 1000;
    [SerializeField] private int customStartingHP = 10;
    
    private void Start()
    {
        if (overrideStartingValues)
        {
            Money = customStartingMoney;
            HP = customStartingHP;
        }
    }

    public void GainMoney(int amount)
    {
        Money += amount;
    }

    public void SpendMoney(int amount)
    {
        Money -= amount;
    }

    public void DealDamage(int damage)
    {
        HP -= damage;
        GameObject.Find("EndBlock").GetComponent<ColorPulse>().Pulse();
        if (HP <= 0)
        {
            HP = 0;
            StartCoroutine(GameOver());
        }
    }

    private IEnumerator GameOver()
    {
        Debug.Log("Game Over");
        GameObject.Find("Canvas/Titles/GameOverText").SetActive(true);

        foreach (TowerAttack attack in FindObjectsByType<TowerAttack>())
        {
            attack.StopShooting();
            attack.enabled = false;
        }
        
        yield return new WaitForSeconds(gameOverDelay);
        Instance = null;
        Destroy(gameObject);
        SceneManager.LoadScene(0);
    }
    
    public void NextLevel()
    {
        Level++;
    }
    
    
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Kill the scene duplicate, keep the original
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}