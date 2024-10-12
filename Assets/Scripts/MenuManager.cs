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

    private CubeRotator cubeRotator;
    private CubeSelector cubeSelector;
    private CubeStateManager cubeStateManager;

    void Start()
    {
        menuPanel.SetActive(true);

        startButton.onClick.AddListener(StartOrRestartGame);
        saveButton.onClick.AddListener(SaveGame);
        loadButton.onClick.AddListener(LoadGame);
        quitButton.onClick.AddListener(QuitGame);
        resumeButton.onClick.AddListener(ResumeGame); 
        cubeRotator = FindObjectOfType<CubeRotator>();
        cubeSelector = FindObjectOfType<CubeSelector>();
        cubeStateManager = FindObjectOfType<CubeStateManager>();

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
        bool isActive = menuPanel.activeSelf;
        menuPanel.SetActive(!isActive);
        Time.timeScale = isActive ? 1 : 0;
    }

    private void StartOrRestartGame()
    {
        cubeSelector.DeselectAllCubes();
        RotateAllCubesToFirstFace();
        resumeButton.gameObject.SetActive(true);
        ToggleMenu();
    }

    private void RotateAllCubesToFirstFace()
    {
        GameObject[] allCubes = GameObject.FindGameObjectsWithTag("Cube");
        foreach (var cube in allCubes)
        {
            cubeRotator.StartRotation(cube, 1);
        }
        Debug.Log("All cubes have the first face turned upwards");
    }

    private void SaveGame()
    {
        cubeStateManager.SaveCubeState();
    }

    private void LoadGame()
    {
        cubeStateManager.LoadCubeState();
    }

    private void QuitGame()
    {
        Debug.Log("Quitting the game...");
        Application.Quit();
    }

    private void ResumeGame()
    {
        ToggleMenu();
    }
}