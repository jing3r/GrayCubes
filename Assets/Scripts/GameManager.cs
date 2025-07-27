using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Центральный управляющий класс игры. Инициализирует все системы,
/// управляет основным игровым циклом и служит точкой доступа для UI.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Ключевые системы")]
    [SerializeField] private CubeSpawner cubeSpawner;
    [SerializeField] private CubeSelector cubeSelector;
    [SerializeField] private CubeRotator cubeRotator;
    [SerializeField] private CubeStateManager cubeStateManager;

    public CubeSelector CubeSelector => cubeSelector;
    public CubeRotator CubeRotator => cubeRotator;

    private List<CubeController> _allCubes = new List<CubeController>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        InitializeGame();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            cubeSelector.HandleSelectionClick(Input.mousePosition);
        }
        HandleKeyboardInput();
    }

    private void InitializeGame()
    {
        _allCubes = cubeSpawner.SpawnCubes();
        
        cubeSelector.Initialize(_allCubes);
        cubeStateManager.Initialize(_allCubes);
    }
    
    private void HandleKeyboardInput()
    {
        for (int i = 1; i <= 6; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                cubeRotator.RotateCubesToFace(cubeSelector.SelectedCubes, (CubeFace)i);
            }
            if (Input.GetKeyDown(KeyCode.Keypad0 + i))
            {
                cubeSelector.ToggleSelectionByCategory((CubeFace)i);
            }
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            cubeRotator.RotateCubesRandomly(cubeSelector.SelectedCubes);
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            var unselectedCubes = cubeSelector.GetUnselectedCubes();
            cubeRotator.RotateCubesRandomly(unselectedCubes);
        }
        if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.Keypad0))
        {
            cubeSelector.DeselectAll();
        }
    }
    
    #region Public Game Actions (API for UI)
    
    public void ResetAllCubes()
    {
        cubeSelector.DeselectAll();
        cubeRotator.RotateCubesToFace(_allCubes, CubeFace.Up); 
        Debug.Log("Все кубы сброшены в начальное состояние.");
    }
    
    public void SaveState() => cubeStateManager.SaveState();
    
    public void LoadState() => cubeStateManager.LoadState();
    
    #endregion
}