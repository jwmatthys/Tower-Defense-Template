using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float HP = 5f;
    public int reward = 25;
    private float _maxHealth;
    
    private Component _triggerAnimation;

    private void Awake()
    {
        _triggerAnimation = GetComponent("TriggerAnimation");
        _maxHealth = HP;
    }

    public float GetMaxHealth()
    {
        return _maxHealth;
    }

    public void TakeDamage(float amount)
    {
        _triggerAnimation?.SendMessage("Play", SendMessageOptions.DontRequireReceiver);
        HP -= amount;
        if (HP <= 0f)
        {
            EconomyManager.Instance?.GainMoney(reward);
            Destroy(gameObject);
        }
    }
}
