using System.Collections;
using UnityEngine;

public class EnemyFreezer : MonoBehaviour
{
    public float fireRate = 15f;
    public float attackRadius = 3f;
    public float freezeDuration = 5f;

    private Coroutine _freezingCoroutine;

    private void Update()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRadius, LayerMask.GetMask("Tower"));

        if (hits.Length > 0 && _freezingCoroutine == null)
            _freezingCoroutine = StartCoroutine(FreezeRoutine());
        else if (hits.Length == 0 && _freezingCoroutine != null)
        {
            StopCoroutine(_freezingCoroutine);
            _freezingCoroutine = null;
        }
    }

    private IEnumerator FreezeRoutine()
    {
        while (true)
        {
            Debug.Log("Freezing!");
            FreezeAll();
            yield return new WaitForSeconds(fireRate);
        }
    }

    private void FreezeAll()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRadius, LayerMask.GetMask("Tower"));

        foreach (Collider hit in hits)
        {
            Debug.Log(hit.transform.name);
            hit.GetComponent<TowerFrozen>()?.Freeze(freezeDuration);
        }
    }
}