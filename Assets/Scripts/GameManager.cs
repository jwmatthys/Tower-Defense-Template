using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float HP { get; private set; } = 100f;

    public void DealDamage(float damage)
    {
        HP -= damage;
    }


    public void EndLevel()
    {
        Debug.Log("EndLevel");
    }
    
    
}
