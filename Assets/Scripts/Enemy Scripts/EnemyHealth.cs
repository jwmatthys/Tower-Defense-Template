using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float HP = 5f;
    public int reward = 25;
    private float _maxHealth;
    
    private ColorPulse _colorPulse;

    private void Awake()
    {
        _colorPulse = GetComponent<ColorPulse>();
        _maxHealth = HP;
    }

    public float GetMaxHealth()
    {
        return _maxHealth;
    }

    public void  TakeDamage(float amount)
    {
        _colorPulse.Pulse();
        HP -= amount;
        if (HP <= 0f)
        {
            FindAnyObjectByType<EconomyManager>().GainMoney(reward);
            Destroy(gameObject);
        }
    }
}
