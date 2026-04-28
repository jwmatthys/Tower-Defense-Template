using UnityEngine;
using UnityEngine.Playables;

public class LevelManager : MonoBehaviour
{
    private PlayableDirector _director;
    public float pollInterval = 1f;

    private int _enemyLayer;

    void Awake()
    {
        _director = GetComponent<PlayableDirector>();
        _enemyLayer = LayerMask.NameToLayer("Enemy");
    }

    void OnEnable()
    {
        _director.stopped += OnTimelineFinished;
    }

    void OnDisable()
    {
        _director.stopped -= OnTimelineFinished;
    }

    void OnTimelineFinished(PlayableDirector d)
    {
        InvokeRepeating(nameof(CheckLevelEnd), 0f, pollInterval);
    }

    void CheckLevelEnd()
    {
        foreach (GameObject go in FindObjectsByType<GameObject>(FindObjectsInactive.Exclude))
            if (go.layer == _enemyLayer) return;

        CancelInvoke(nameof(CheckLevelEnd));
        GetComponent<GameManager>().EndLevel();
    }
}