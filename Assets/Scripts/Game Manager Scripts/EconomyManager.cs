using UnityEngine;
using System.Collections;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    public event System.Action OnGameOver;
    private bool _gameOver;

    [SerializeField] private int startingHP = 100;
    [SerializeField] private int startingMoney = 100;
    
    public int HP { get; private set; } = 100;
    public int Money { get; private set; } = 100;
    [HideInInspector] public int level;

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
        if (_gameOver) return;
        HP -= damage;
        GameObject.Find("EndBlock")
            .GetComponent("TriggerAnimation")
            ?.SendMessage("Play", SendMessageOptions.DontRequireReceiver);
        if (HP <= 0)
        {
            HP = 0;
            _gameOver = true;
            OnGameOver?.Invoke();
        }
    }

    public void Shutdown()
    {
        Instance = null;
        Destroy(gameObject);
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