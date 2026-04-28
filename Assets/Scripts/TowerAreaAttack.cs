using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerAreaAttack : MonoBehaviour
{
    public float damage = 0.1f;
    public float fireRate = 1f;
    public float attackRadius = 3f;

    private Coroutine shootingCoroutine;

    private void Update()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRadius, LayerMask.GetMask("Enemy"));

        if (hits.Length > 0 && shootingCoroutine == null)
            shootingCoroutine = StartCoroutine(ShootRoutine());
        else if (hits.Length == 0 && shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }
    }

    private IEnumerator ShootRoutine()
    {
        while (true)
        {
            Debug.Log("Firing!");
            DealDamageToAll();
            yield return new WaitForSeconds(fireRate);
        }
    }

    private void DealDamageToAll()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRadius, LayerMask.GetMask("Enemy"));

        foreach (Collider hit in hits)
        {
            Debug.Log(hit.transform.name);
            hit.GetComponent<EnemyHealth>()?.TakeDamage(damage);
        }
    }
}