using System.Collections;
using UnityEngine;

public class TowerSingleAttack : MonoBehaviour
{
    public enum AttackPattern { AttackFirst, AttackLast, AttackClosest }
    public float damage = 1f;
    public float fireRate = 1f;
    public float attackRadius = 10f;
    public AttackPattern attackPattern = AttackPattern.AttackFirst;

    private Coroutine _shootingCoroutine;
    private readonly Collider[] _hits = new Collider[100]; // pre-allocated buffer

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
            EnemyHealth target = GetTarget();
            if (target != null)
                target.TakeDamage(damage);

            yield return new WaitForSeconds(1f / fireRate);
        }
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
            if (health == null || mover == null) continue;

            float score = GetScore(mover, _hits[i].transform.position);

            if (score > bestScore)
            {
                bestHealth = health;
                bestScore = score;
            }
        }

        return bestHealth;
    }

    private float GetScore(EnemyMover mover, Vector3 enemyPosition)
    {
        switch (attackPattern)
        {
            case AttackPattern.AttackFirst:  return mover.PathProgress;
            case AttackPattern.AttackLast:   return -mover.PathProgress;
            case AttackPattern.AttackClosest: return -Vector3.Distance(transform.position, enemyPosition);
            default: return 0f;
        }
    }
}