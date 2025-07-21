using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CityViewManager : MonoBehaviour
{
    public static CityViewManager Instance;

    [Header("Camera Settings")]
    public Camera mainMapCamera;
    public Camera cityCamera;

    [Header("Scene Names")]
    public string citySceneName = "CityScene";
    public string mainSceneName = "MainScene";

    private CityID _currentCity;
    public CityID CurrentCity => _currentCity;
    public static CityID SelectedCity { get; private set; }

    private bool _isInCityView = false;
    private bool _isLoading = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (_isInCityView && Input.GetKeyDown(KeyCode.Escape))
        {
            if (PersistentUIManager.Instance?.IsCityUIOpen == true)
                CloseCurrentCityUI();
            else
                ExitCity();
        }
    }

    public void EnterCity(CityID city)
    {
        if (_isLoading || city == null) return;

        StartCoroutine(LoadCitySceneAndUI(city));
    }

    private IEnumerator LoadCitySceneAndUI(CityID cityToLoad)
    {
        _isLoading = true;
        _currentCity = cityToLoad;
        SelectedCity = cityToLoad;

        // Load city scene
        yield return SceneManager.LoadSceneAsync(citySceneName);

        // Ensure city is properly initialized
        var citySetup = FindObjectOfType<CitySceneSetup>();
        if (citySetup == null)
        {
            Debug.LogError("CitySceneSetup not found in city scene!");
            yield break;
        }

        // Wait for city to be fully initialized
        float timeout = 3f;
        float elapsed = 0f;
        while ((_currentCity == null || _currentCity.resources == null) && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (_currentCity == null || _currentCity.resources == null)
        {
            Debug.LogError("City initialization timeout!");
            yield break;
        }

        // Now show UI
        if (PersistentUIManager.Instance != null)
        {
            PersistentUIManager.Instance.ShowCityUI(_currentCity);
        }

        // Camera setup
        if (mainMapCamera != null) mainMapCamera.gameObject.SetActive(false);
        if (cityCamera != null) cityCamera.gameObject.SetActive(true);

        _isInCityView = true;
        _isLoading = false;
    }

    public void ExitCity()
    {
        if (!_isInCityView || _isLoading) return;
        StartCoroutine(UnloadCityScene());
    }

    private IEnumerator UnloadCityScene()
    {
        _isLoading = true;

        // Load main scene additively first
        yield return SceneManager.LoadSceneAsync(mainSceneName, LoadSceneMode.Additive);

        // Find main camera
        mainMapCamera = FindCameraInScene("MainCamera") ?? mainMapCamera;

        // Unload city scene
        yield return SceneManager.UnloadSceneAsync(citySceneName);

        // Switch cameras
        if (mainMapCamera != null) mainMapCamera.gameObject.SetActive(true);
        if (cityCamera != null) cityCamera.gameObject.SetActive(false);

        // Reset state
        _isInCityView = false;
        _currentCity = null;
        SelectedCity = null;
        _isLoading = false;

        // Show main UI
        PersistentUIManager.Instance?.ShowMainUI();
    }

    public Camera GetActiveCamera() => _isInCityView ? cityCamera : mainMapCamera;
    public void SetCityCamera(Camera cam) => cityCamera = cam;
    public void CloseCurrentCityUI() => PersistentUIManager.Instance?.GetCurrentCityUI()?.gameObject.SetActive(false);
    public void OverrideCurrentCity(CityID city) => _currentCity = SelectedCity = city;

    private Camera FindCameraInScene(string nameContains)
    {
        foreach (Camera cam in Camera.allCameras)
        {
            if (cam.name.Contains(nameContains, System.StringComparison.OrdinalIgnoreCase))
                return cam;
        }
        return null;
    }
}