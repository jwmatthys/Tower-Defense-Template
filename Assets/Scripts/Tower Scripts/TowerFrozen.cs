// TowerFrozen.cs
using System.Collections;
using UnityEngine;

public class TowerFrozen : MonoBehaviour
{
    public void Freeze(float dur)
    {
        StartCoroutine(FreezeRoutine(dur));
    }

    private IEnumerator FreezeRoutine(float dur)
    {
        if (TryGetComponent(out TowerAttack towerAttack))
        {
            towerAttack.enabled = false;
            yield return new WaitForSeconds(dur);
            towerAttack.enabled = true;
        }
    }
}