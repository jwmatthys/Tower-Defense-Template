using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float HP { get; private set; } = 100f;
    public int money { get; private set; } = 100;

    public void DealDamage(float damage)
    {
        HP -= damage;
    }

    public void SpendMoney(int amount)
    {
        money -= amount;
    }

    public void GainMoney(int amount)
    {
        money += amount;
    }

    public void EndLevel()
    {
        Debug.Log("EndLevel");
    }
    
    
}
