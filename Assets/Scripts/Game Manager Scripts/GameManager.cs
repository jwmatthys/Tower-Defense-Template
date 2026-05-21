using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Tooltip("How many seconds to wait after ending the level" +
             "before loading the next scene.")]
    [SerializeField] private float endLevelDelay = 5f;

    [Tooltip("How long the Game Over message should appear before game resets.")]
    [SerializeField] private float gameOverDelay = 5f;

    public bool IsGameOver { get; private set; }

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

    void Start()
    {
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnGameOver += HandleGameOver;
    }

    void OnEnable()
    {
        director.stopped += OnTimelineFinished;
    }

    void OnDisable()
    {
        director.stopped -= OnTimelineFinished;
    }

    void OnDestroy()
    {
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnGameOver -= HandleGameOver;
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
        if (IsGameOver) return;

        if (guiDisplay == null)
            guiDisplay = FindAnyObjectByType<GUIDisplay>();

        guiDisplay?.ShowLevelClear();

        if (levelClearSound != null)
            _audioSource.PlayOneShot(levelClearSound);

        StartCoroutine(LoadNextLevel());
    }

    IEnumerator LoadNextLevel()
    {
        EconomyManager.Instance?.LevelClearBonus();
        yield return new WaitForSeconds(endLevelDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void HandleGameOver()
    {
        IsGameOver = true;
        StartCoroutine(GameOverCoroutine());
    }

    private IEnumerator GameOverCoroutine()
    {
        Debug.Log("Game Over");
        if (guiDisplay == null)
            guiDisplay = FindAnyObjectByType<GUIDisplay>();
        guiDisplay?.ShowGameOver();

        foreach (TowerAttack attack in FindObjectsByType<TowerAttack>())
        {
            attack.StopShooting();
            attack.enabled = false;
        }

        yield return new WaitForSeconds(gameOverDelay);
        EconomyManager.Instance?.Shutdown();
        SceneManager.LoadScene(0);
    }
}