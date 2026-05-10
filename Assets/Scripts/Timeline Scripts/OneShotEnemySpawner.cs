using System.Collections;
using UnityEngine;

public class OneShotEnemySpawner : MonoBehaviour
{
    public void StartSpawning(GameObject enemyPrefab, float interval)
    {
        Instantiate(enemyPrefab, transform.position, transform.rotation);
    }
}