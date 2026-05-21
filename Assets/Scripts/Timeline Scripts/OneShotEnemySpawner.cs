using System.Collections;
using UnityEngine;

public class OneShotEnemySpawner : EnemySpawner
{
    protected override IEnumerator SpawnLoop(GameObject enemyPrefab, float interval)
    {
        Instantiate(enemyPrefab, transform.position, transform.rotation);
        yield break;
    }
}