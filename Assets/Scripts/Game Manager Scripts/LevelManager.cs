using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    [Tooltip("How many seconds to wait after ending the level" +
             "before loading the next scene.")]
    [SerializeField] private float endLevelDelay = 3f;

    [SerializeField] private TextMeshProUGUI sceneClearedMessage;
    
    [SerializeField] private PlayableDirector director;
    public float pollInterval = 1f;

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
        InvokeRepeating(nameof(CheckLevelEnd), 0f, pollInterval);
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
        Debug.Log("EndLevel");
        sceneClearedMessage.gameObject.SetActive(true);
        StartCoroutine(LoadNextLevel());
    }

    IEnumerator LoadNextLevel()
    {
        yield return new WaitForSeconds(endLevelDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}