using System.Collections;
using UnityEngine;

public class RandomTimingEnemySpawner : EnemySpawner
{
    protected override IEnumerator SpawnLoop(GameObject enemyPrefab, float interval)
    {
        while (true)
        {
            Instantiate(enemyPrefab, transform.position, transform.rotation);
            yield return new WaitForSeconds(interval * Random.Range(0.5f, 2f));
        }
    }
}