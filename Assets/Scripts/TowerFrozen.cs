using System.Collections;
using UnityEngine;

public class TowerFrozen : MonoBehaviour
{
    public void Freeze(float dur)
    {
        StartCoroutine(FreezeRoutine(dur));
    }

    IEnumerator FreezeRoutine(float dur)
    {
        if (TryGetComponent(out TowerAreaAttack areaAttack))
        {
            areaAttack.enabled = false;
            yield return new WaitForSeconds(dur);
            areaAttack.enabled = true;
        }
        else if (TryGetComponent(out TowerSingleAttack singleAttack))
        {
            Debug.Log("Frozen!");
            singleAttack.enabled = false;
            yield return new WaitForSeconds(dur);
            Debug.Log("Unfrozen!");
            singleAttack.enabled = true;
        }
    }
}
