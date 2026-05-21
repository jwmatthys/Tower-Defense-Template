using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }
    public bool IsGameOver { get; private set; }

    [SerializeField] private int startingHP = 100;
    [SerializeField] private int startingMoney = 100;
    
    public int HP { get; private set; } = 100;
    public int Money { get; private set; } = 100;
    [HideInInspector] public int level;
    
    [Tooltip("How long the Game Over message should appear before game resets.")]
    [SerializeField] private float gameOverDelay = 5f;

    [SerializeField] private bool overrideStartingValues;
    [SerializeField] private int customStartingMoney = 1000;
    [SerializeField] private int customStartingHP = 10;
    
    private void Start()
    {
        HP = startingHP;
        Money = startingMoney;

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

    /// <summary>Attempts to spend gold. Returns true if successful, false if insufficient funds.</summary>
    public bool TrySpendGold(int amount)
    {
        if (Money >= amount)
        {
            SpendMoney(amount);
            return true;
        }
        return false;
    }

    public void DealDamage(int damage)
    {
        if (IsGameOver) return;
        HP -= damage;
        GameObject.Find("EndBlock")
            .GetComponent("TriggerAnimation")
            ?.SendMessage("Play", SendMessageOptions.DontRequireReceiver);
        if (HP <= 0)
        {
            HP = 0;
            StartCoroutine(GameOver());
        }
    }

    private IEnumerator GameOver()
    {
        IsGameOver = true;
        Debug.Log("Game Over");
        FindAnyObjectByType<GUIDisplay>()?.ShowGameOver();

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
        level++;
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