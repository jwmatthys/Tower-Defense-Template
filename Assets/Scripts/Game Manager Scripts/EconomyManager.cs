using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    public int HP { get; private set; } = 100;
    public int Money { get; private set; } = 100;
    public int Level { get; private set; } = 1;

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