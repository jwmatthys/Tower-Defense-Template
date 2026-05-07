using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Tooltip("How many seconds to wait after ending the level" +
             "before loading the next scene.")]
    [SerializeField] private float endLevelDelay = 3f;

    [Tooltip("The EconomyManager keeps track of HP, money, and current level.")]
    [SerializeField] private EconomyManager economyManager;

    [SerializeField] private TextMeshProUGUI sceneClearedMessage;
    [SerializeField] private PlayableDirector director;

    private readonly float _pollInterval = 1f;
    private int _enemyLayer;
    
    void Awake()
    {
        _enemyLayer = LayerMask.NameToLayer("Enemy");
    }

    void OnEnable()
    {
        director.stopped += OnTimelineFinished;
    }

    void OnDisable()
    {
        director.stopped -= OnTimelineFinished;
    }

    void OnTimelineFinished(PlayableDirector d)
    {
        InvokeRepeating(nameof(CheckLevelEnd), 0f, _pollInterval);
    }

    void CheckLevelEnd()
    {
        foreach (GameObject go in FindObjectsByType<GameObject>(FindObjectsInactive.Exclude))
            if (go.layer == _enemyLayer) return;

        CancelInvoke(nameof(CheckLevelEnd));
        EndLevel();
    }
    
    private void EndLevel()
    {
        sceneClearedMessage.gameObject.SetActive(true);
        StartCoroutine(LoadNextLevel());
    }

    IEnumerator LoadNextLevel()
    {
        yield return new WaitForSeconds(endLevelDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}