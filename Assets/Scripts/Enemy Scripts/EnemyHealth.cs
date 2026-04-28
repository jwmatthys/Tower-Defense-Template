using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float HP = 5f;
    public int reward = 25;

    public void  TakeDamage(float amount)
    {
        HP -= amount;
        if (HP <= 0f)
        {
            FindAnyObjectByType<GameManager>().GainMoney(reward);
            Destroy(gameObject);
        }
    }
}
