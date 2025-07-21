// GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Configuration")]
    public StartingCityConfig startingCityConfig;
    public string worldMapSceneName = "WorldMap";
    public string playerCitySceneName = "PlayerCityScene";
    public float sceneTransitionDelay = 0.5f;

    [Header("State")]
    [SerializeField] private bool hasPlayerCity = false;
    [SerializeField] private bool isLoadingScene = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        InitializeGame();
    }

    void InitializeGame()
    {
        if (!hasPlayerCity)
        {
            StartCoroutine(ShowPlayerCity());
        }
        else
        {
            StartCoroutine(LoadWorldMap());
        }
    }

    IEnumerator ShowPlayerCity()
    {
        if (isLoadingScene) yield break;
        isLoadingScene = true;

        // Unload any existing scenes except persistent
        yield return UnloadAllNonPersistentScenes();

        // Load player city scene
        yield return SceneManager.LoadSceneAsync(playerCitySceneName, LoadSceneMode.Additive);

        // Initialize city with starting config
        if (startingCityConfig != null)
        {
            var citySetup = FindObjectOfType<PlayerCitySceneSetup>();
            if (citySetup != null)
            {
                citySetup.InitializeWithConfig(startingCityConfig);
            }
            else
            {
                Debug.LogError("PlayerCitySceneSetup not found in the scene!");
            }
        }
        else
        {
            Debug.LogError("StartingCityConfig is not assigned in GameManager!");
        }

        hasPlayerCity = true;
        isLoadingScene = false;
    }

    IEnumerator LoadWorldMap()
    {
        if (isLoadingScene) yield break;
        isLoadingScene = true;

        // Unload any existing scenes except persistent
        yield return UnloadAllNonPersistentScenes();

        // Load world map scene
        yield return SceneManager.LoadSceneAsync(worldMapSceneName, LoadSceneMode.Additive);
        isLoadingScene = false;
    }

    IEnumerator UnloadAllNonPersistentScenes()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name != gameObject.scene.name) // Don't unload persistent scene
            {
                yield return SceneManager.UnloadSceneAsync(scene);
            }
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == playerCitySceneName)
        {
            // Set as active scene
            SceneManager.SetActiveScene(scene);

            // Initialize camera
            Camera.main.transform.position = new Vector3(0, 0, Camera.main.transform.position.z);
        }
    }

    public void ReturnToWorldMap()
    {
        if (!isLoadingScene)
        {
            StartCoroutine(LoadWorldMap());
        }
    }

    public void ReturnToPlayerCity()
    {
        if (!isLoadingScene)
        {
            StartCoroutine(ShowPlayerCity());
        }
    }
}