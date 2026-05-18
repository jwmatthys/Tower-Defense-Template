using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Tooltip("How many seconds to wait after ending the level" +
             "before loading the next scene.")]
    [SerializeField] private float endLevelDelay = 5f;

    [Tooltip("The EconomyManager keeps track of HP, money, and current level.")]
    [SerializeField] private EconomyManager economyManager;

    [SerializeField] private GUIDisplay guiDisplay;
    [SerializeField] private PlayableDirector director;

    [Tooltip("Sound played when the level is cleared.")]
    [SerializeField] private AudioClip levelClearSound;

    private AudioSource _audioSource;
    private readonly float _pollInterval = 1f;
    private int _enemyLayer;
    
    void Awake()
    {
        _enemyLayer = LayerMask.NameToLayer("Enemy");
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.spatialBlend = 0f;
        _audioSource.playOnAwake = false;
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
        if (EconomyManager.Instance != null && EconomyManager.Instance.IsGameOver) return;

        if (guiDisplay == null)
            guiDisplay = FindAnyObjectByType<GUIDisplay>();

        guiDisplay?.ShowLevelClear();

        if (levelClearSound != null)
            _audioSource.PlayOneShot(levelClearSound);

        StartCoroutine(LoadNextLevel());
    }

    IEnumerator LoadNextLevel()
    {
        yield return new WaitForSeconds(endLevelDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}