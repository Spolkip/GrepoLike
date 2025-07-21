using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PersistentUIManager : MonoBehaviour
{
    public static PersistentUIManager Instance;

    [Header("UI References")]
    public GameObject mainUI; // Main map UI
    public Button enterCurrentCityButton; // Button on map to re-enter current city

    private CityUI _currentCityUI; // Reference to the active city UI (in city scene)
    private bool _isInCityView = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
        SetupButtons();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (CityViewManager.Instance == null) return;

        if (scene.name == CityViewManager.Instance.citySceneName)
        {
            _currentCityUI = FindObjectOfType<CityUI>();
            _isInCityView = true;
        }
        else
        {
            _isInCityView = false;
            _currentCityUI = null;
        }
    }

    private void SetupButtons()
    {
        if (enterCurrentCityButton != null)
        {
            enterCurrentCityButton.onClick.AddListener(() =>
            {
                if (CityViewManager.SelectedCity != null)
                {
                    CityViewManager.Instance?.EnterCity(CityViewManager.SelectedCity);
                }
            });
        }
    }

    public void ShowCityUI(CityID city)
    {
        if (_currentCityUI != null)
        {
            _currentCityUI.Initialize(city);
            _currentCityUI.gameObject.SetActive(true);
        }

        if (mainUI != null)
        {
            mainUI.SetActive(false);
        }
    }

    public void ShowMainUI()
    {
        if (_currentCityUI != null)
        {
            _currentCityUI.gameObject.SetActive(false);
        }

        if (mainUI != null)
        {
            mainUI.SetActive(true);
        }
    }

    public CityUI GetCurrentCityUI() => _currentCityUI;
    public bool IsCityUIOpen => _currentCityUI != null && _currentCityUI.gameObject.activeSelf;
}