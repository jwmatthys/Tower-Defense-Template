using UnityEngine;

public class Singleton : MonoBehaviour
{
    private static Singleton _instance;

    void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
}