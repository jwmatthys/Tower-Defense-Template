using System.Collections;
using UnityEngine;

public class TowerSlowAttack : MonoBehaviour
{
    public float slowInterval = 15f;
    public float slowDuration = 5f;
    public float slowFactor = 2;
    public float attackRadius = 3f;

    private Coroutine _slowingCoroutine;

    private void Update()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRadius, LayerMask.GetMask("Enemy"));

        if (hits.Length > 0 && _slowingCoroutine == null)
            _slowingCoroutine = StartCoroutine(SlowRoutine());
        else if (hits.Length == 0 && _slowingCoroutine != null)
        {
            StopCoroutine(_slowingCoroutine);
            _slowingCoroutine = null;
        }
    }

    private IEnumerator SlowRoutine()
    {
        while (true)
        {
            Debug.Log("Firing!");
            ApplySlownessToAll();
            yield return new WaitForSeconds(slowInterval);
        }
    }

    private void ApplySlownessToAll()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRadius, LayerMask.GetMask("Enemy"));

        foreach (Collider hit in hits)
        {
            Debug.Log(hit.transform.name);
            hit.GetComponent<EnemyMover>()?.ApplySlowness(slowFactor, slowDuration);
        }
    }
}