using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public void StartSpawning(GameObject enemyPrefab, float interval)
    {
        StartCoroutine(SpawnLoop(enemyPrefab, interval));
    }

    IEnumerator SpawnLoop(GameObject enemyPrefab, float interval)
    {
        while (true)
        {
            Instantiate(enemyPrefab, transform.position, transform.rotation);
            yield return new WaitForSeconds(interval);
        }
    }
}