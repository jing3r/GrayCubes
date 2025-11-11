using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Управляет главным меню, меню паузы и запуском игровых режимов.
/// </summary>
public class MenuManager : MonoBehaviour
{
    #region Serialized Fields
    [Header("Панели")]
    [SerializeField] private GameObject menuPanel;
    
    [Header("Кнопки Меню")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button startMatch3Button;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button resumeButton;

    [Header("Кнопки Игровых Режимов")]
    [SerializeField] private Button gameOfLifeButton;
    [SerializeField] private GameOfLifeController gameOfLifeController;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        menuPanel.SetActive(true);
        Time.timeScale = 0; 
        BindButtons();
        UpdateInGameButtonsVisibility();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Открывает или закрывает меню паузы.
    /// </summary>
    public void ToggleMenu()
    {
        bool isMenuActive = !menuPanel.activeSelf;
        menuPanel.SetActive(isMenuActive);
        Time.timeScale = isMenuActive ? 0 : 1;
        
        resumeButton.gameObject.SetActive(isMenuActive && GameManager.Instance != null);
    }
    #endregion
    
    #region Private Methods
    private void BindButtons()
    {
        startButton.onClick.AddListener(OnStartSandboxClicked);
        startMatch3Button.onClick.AddListener(OnStartMatch3Clicked);
        saveButton.onClick.AddListener(SaveGame);
        loadButton.onClick.AddListener(LoadGame);
        quitButton.onClick.AddListener(QuitGame);
        resumeButton.onClick.AddListener(ResumeGame);
        
        if (gameOfLifeButton != null && gameOfLifeController != null)
        {
            gameOfLifeButton.onClick.AddListener(gameOfLifeController.ToggleSimulation);
        }
    }

    private void OnStartSandboxClicked() 
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.StartSandboxGame();
        UpdateInGameButtonsVisibility();
        ToggleMenu();
    }

    private void OnStartMatch3Clicked()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.StartMatch3Game();
        UpdateInGameButtonsVisibility();
        ToggleMenu();
    }

    /// <summary>
    /// Обновляет видимость кнопок, зависящих от игрового режима.
    /// </summary>
    private void UpdateInGameButtonsVisibility()
    {
        if (GameManager.Instance == null) return;
        
        bool isSandbox = (GameManager.Instance.CurrentMode == GameMode.Sandbox);
        gameOfLifeButton.gameObject.SetActive(isSandbox);
    }

    private void SaveGame()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.SaveState();
    }
    
    private void LoadGame()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.LoadState();
    }
    
    private void ResumeGame() => ToggleMenu();

    private void QuitGame()
    {
        Debug.Log("Выход из приложения...");
        Application.Quit();
    }
    #endregion
}