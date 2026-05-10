using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class AutoAdvanceScene : MonoBehaviour
{
    [SerializeField] private float titleTimeInterval = 3f;
    void Start()
    {
        StartCoroutine(NextScene());
    }

    IEnumerator NextScene()
    {
        yield return new WaitForSeconds(titleTimeInterval);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
