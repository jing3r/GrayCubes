using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject menuPanel;
    public Button startButton;
    public Button saveButton;
    public Button loadButton;
    public Button quitButton;
    public Button resumeButton;

    void Start()
    {
        menuPanel.SetActive(true);
        Time.timeScale = 0; // Пауза при старте

        startButton.onClick.AddListener(StartOrRestartGame);
        saveButton.onClick.AddListener(SaveGame);
        loadButton.onClick.AddListener(LoadGame);
        quitButton.onClick.AddListener(QuitGame);
        resumeButton.onClick.AddListener(ResumeGame);

        resumeButton.gameObject.SetActive(false); 
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        bool isMenuActive = !menuPanel.activeSelf;
        menuPanel.SetActive(isMenuActive);
        Time.timeScale = isMenuActive ? 0 : 1;
        
        // Кнопка "Продолжить" видна только если игра уже была начата
        resumeButton.gameObject.SetActive(Time.timeScale == 0 && GameManager.Instance != null);
    }

    private void StartOrRestartGame()
    {
        GameManager.Instance.ResetAllCubes();
        ToggleMenu();
    }

    private void SaveGame() => GameManager.Instance.SaveState();
    private void LoadGame() => GameManager.Instance.LoadState();
    private void ResumeGame() => ToggleMenu();

    private void QuitGame()
    {
        Debug.Log("Quitting...");
        Application.Quit();
    }
}