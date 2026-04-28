using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float HP = 5f;
    
    public void  TakeDamage(float amount)
    {
        HP -= amount;
        if (HP <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
