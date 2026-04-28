using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class SpawnBehaviour : PlayableBehaviour
{
    public GameObject spawnerPrefab;
    public GameObject enemyPrefab;
    public float timeBetweenSpawns = 1f;

    private EnemySpawner _spawnedInstance;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (!Application.isPlaying || spawnerPrefab == null) return;

        GameObject go = Object.Instantiate(spawnerPrefab);
        _spawnedInstance = go.GetComponent<EnemySpawner>();
        _spawnedInstance?.StartSpawning(enemyPrefab, timeBetweenSpawns);
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (_spawnedInstance == null) return;

        Object.Destroy(_spawnedInstance.gameObject);
        _spawnedInstance = null;
    }
}