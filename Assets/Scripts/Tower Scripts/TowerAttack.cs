// TowerAttack.cs
using System.Collections;
using UnityEngine;

public class TowerAttack : MonoBehaviour
{
    public enum AttackPattern { AttackFirst, AttackLast, AttackClosest, Area }

    [Header("Settings")]
    public AttackPattern attackPattern = AttackPattern.AttackFirst;
    public float damage = 1f;
    public float attackInterval = 1f;
    public float attackRadius = 2f;

    private Coroutine _shootingCoroutine;
    private readonly Collider[] _hits = new Collider[100];

    private void Update()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, attackRadius, _hits, LayerMask.GetMask("Enemy"));

        if (count > 0 && _shootingCoroutine == null)
            _shootingCoroutine = StartCoroutine(ShootRoutine());
        else if (count == 0 && _shootingCoroutine != null)
        {
            StopCoroutine(_shootingCoroutine);
            _shootingCoroutine = null;
        }
    }

    private IEnumerator ShootRoutine()
    {
        while (true)
        {
            if (attackPattern == AttackPattern.Area)
                DealDamageToAll();
            else
                GetTarget()?.TakeDamage(damage);

            yield return new WaitForSeconds(attackInterval);
        }
    }

    private void DealDamageToAll()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, attackRadius, _hits, LayerMask.GetMask("Enemy"));
        for (int i = 0; i < count; i++)
            _hits[i].GetComponent<EnemyHealth>()?.TakeDamage(damage);
    }

    private EnemyHealth GetTarget()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, attackRadius, _hits, LayerMask.GetMask("Enemy"));
        if (count == 0) return null;

        EnemyHealth bestHealth = null;
        float bestScore = float.MinValue;

        for (int i = 0; i < count; i++)
        {
            EnemyHealth health = _hits[i].GetComponent<EnemyHealth>();
            EnemyMover mover = _hits[i].GetComponent<EnemyMover>();
            if (health || mover) continue;

            float score = attackPattern switch
            {
                AttackPattern.AttackFirst   => mover.PathProgress,
                AttackPattern.AttackLast    => -mover.PathProgress,
                AttackPattern.AttackClosest => -Vector3.Distance(transform.position, _hits[i].transform.position),
                _                           => 0f
            };

            if (score > bestScore)
            {
                bestHealth = health;
                bestScore = score;
            }
        }

        return bestHealth;
    }
}