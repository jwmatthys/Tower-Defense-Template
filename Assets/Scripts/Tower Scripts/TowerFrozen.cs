// TowerFrozen.cs
using System.Collections;
using UnityEngine;

public class TowerFrozen : MonoBehaviour
{
    private Coroutine _freezeCoroutine;

    public void Freeze(float dur)
    {
        if (_freezeCoroutine != null)
            StopCoroutine(_freezeCoroutine);
        _freezeCoroutine = StartCoroutine(FreezeRoutine(dur));
    }

    private IEnumerator FreezeRoutine(float dur)
    {
        if (TryGetComponent(out TowerAttack towerAttack))
        {
            Debug.Log($"Tower {this.gameObject.name} is freezing.");
            towerAttack.StopShooting();
            towerAttack.enabled = false;
            yield return new WaitForSeconds(dur);
            towerAttack.enabled = true;
        }
        _freezeCoroutine = null;
    }
}