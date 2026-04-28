using System.Collections;
using UnityEngine;

public class EnemyFreezer : MonoBehaviour
{
    public float freezeInterval = 15f;
    public float freezeRadius = 3f;
    public float freezeDuration = 5f;

    private Coroutine _freezingCoroutine;
    private readonly Collider[] _hits = new Collider[50];

    private void Update()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, freezeRadius, _hits, LayerMask.GetMask("Tower"));

        if (count > 0 && _freezingCoroutine == null)
            _freezingCoroutine = StartCoroutine(FreezeRoutine());
        else if (count == 0 && _freezingCoroutine != null)
        {
            StopCoroutine(_freezingCoroutine);
            _freezingCoroutine = null;
        }
    }

    private IEnumerator FreezeRoutine()
    {
        while (true)
        {
            FreezeAll();
            yield return new WaitForSeconds(freezeInterval);
        }
    }

    private void FreezeAll()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, freezeRadius, _hits, LayerMask.GetMask("Tower"));
        for (int i = 0; i < count; i++)
            _hits[i].GetComponent<TowerFrozen>()?.Freeze(freezeDuration);
    }
}