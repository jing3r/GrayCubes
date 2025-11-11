using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Центральный управляющий класс игры. Управляет игровыми режимами, состоянием
/// и предоставляет публичный API для взаимодействия с игровым миром.
/// Не обрабатывает ввод напрямую.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    #region Serialized Fields
    [Header("Ключевые системы")]
    [SerializeField] private CubeSpawner cubeSpawner;
    [SerializeField] private CubeSelector cubeSelector;
    [SerializeField] private CubeRotator cubeRotator;
    [SerializeField] private CubeMover cubeMover;
    [SerializeField] private CubeStateManager cubeStateManager;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private ActionController actionController;
    
    [Header("Игровые модули")]
    [SerializeField] private GameOfLifeController gameOfLifeController;
    [SerializeField] private Match3Controller match3Controller;

    [Header("UI Компоненты")]
    [SerializeField] private Match3UI match3UI;

    [Header("Настройки вращения поля")]
    [SerializeField] private Transform boardContainer;
    [SerializeField] private float boardRotationSpeed = 90f;
    #endregion

    #region Private State
    private List<CubeController> _allCubes = new List<CubeController>();
    private GameMode _currentMode;
    private Quaternion _targetBoardRotation;
    private int _boardRotationState = 0;
    private Transform _hiddenCubesContainer;
    #endregion

    #region Public Properties
    /// <summary>
    /// Логическое представление игровой доски.
    /// </summary>
    public GameBoard GameBoard { get; private set; }
    
    /// <summary>
    /// Указывает, находится ли игра в процессе действия (ввод заблокирован).
    /// </summary>
    public bool IsActionInProgress => actionController != null && actionController.IsActionInProgress;
    
    /// <summary>
    /// Указывает, находится ли игра в процессе загрузки.
    /// </summary>
    public bool IsLoading { get; set; } = false;
    
    /// <summary>
    /// Список всех активных кубов на поле.
    /// </summary>
    public IReadOnlyList<CubeController> AllCubes => _allCubes;

    /// <summary>
    /// Текущий активный игровой режим.
    /// </summary>
    public GameMode CurrentMode => _currentMode;

    /// <summary>
    /// Логическое состояние поворота поля (0=0°, 1=90°, 2=180°, 3=270°).
    /// </summary>
    public int BoardRotationState => _boardRotationState;
    
    /// <summary>
    /// Возвращает целевое вращение игрового поля.
    /// </summary>
    public Quaternion GetTargetBoardRotation() => _targetBoardRotation;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        
        if (boardContainer != null) _targetBoardRotation = boardContainer.rotation;
        _hiddenCubesContainer = new GameObject("HiddenCubesContainer").transform;
        _hiddenCubesContainer.gameObject.SetActive(false);
        
        actionController.Initialize(this, null, cubeSelector, cubeRotator, cubeMover, match3Controller, boardContainer);
    }

    private void Update()
    {
        UpdateBoardRotation();
    }
    #endregion
    
    #region Game State Management
    /// <summary>
    /// Запускает игру в режиме "Песочница".
    /// </summary>
    public void StartSandboxGame()
    {
        _currentMode = GameMode.Sandbox;
        ClearBoard();
        InitializeSandboxBoard();
        if(match3UI != null) match3UI.Hide();
    }

    /// <summary>
    /// Запускает игру в режиме "Match-3".
    /// </summary>
    public void StartMatch3Game()
    {
        _currentMode = GameMode.Match3;
        InitializeMatch3Board();
        if(match3UI != null) match3UI.Show();
    }
    
    /// <summary>
    /// Сохраняет текущее состояние игрового поля.
    /// </summary>
    public void SaveState() => cubeStateManager.SaveState();
    
    /// <summary>
    /// Загружает ранее сохраненное состояние игрового поля.
    /// </summary>
    public void LoadState() => cubeStateManager.LoadState();
    #endregion

    #region Board Initialization
    private void InitializeSandboxBoard()
    {
        int boardSize = cubeSpawner.GridSize;
        _allCubes = cubeSpawner.SpawnCubes(boardSize, boardSize);
        InitializeCommonSystems();
        if (cameraController != null) cameraController.FitCameraToBoard(boardSize, boardSize, cubeSpawner.CubeSpacing);
        if (gameOfLifeController != null) gameOfLifeController.Initialize(GameBoard, cubeRotator, _allCubes);
    }
    
    private void InitializeMatch3Board()
    {
        ClearBoard(); 
        int boardSize = 8;
        _allCubes = cubeSpawner.SpawnCubes(boardSize, boardSize); 
        foreach (var cube in _allCubes)
        {
            CubeFace randomFace = (CubeFace)Random.Range(1, 7);
            cube.transform.rotation = cubeRotator.GetRotationForFace(randomFace);
        }
        InitializeCommonSystems();
        if (cameraController != null) cameraController.FitCameraToBoard(boardSize, boardSize, cubeSpawner.CubeSpacing);
        match3Controller.Initialize(this, GameBoard, cubeMover, cubeSpawner);
        match3Controller.StartGame();
    }
    
    private void InitializeCommonSystems()
    {
        GameBoard = new GameBoard();
        GameBoard.Initialize(_allCubes, cubeSpawner.CubeSpacing, boardContainer);
        actionController.Initialize(this, GameBoard, cubeSelector, cubeRotator, cubeMover, match3Controller, boardContainer);
        cubeSelector.Initialize(_allCubes);
        cubeStateManager.Initialize(this, GameBoard, cubeRotator);
    }
    
    /// <summary>
    /// Полностью очищает игровое поле, уничтожая все кубы и сбрасывая системы.
    /// </summary>
    public void ClearBoard()
    {
        if (cubeRotator != null) cubeRotator.StopAllRotations();
        if (cubeMover != null) cubeMover.StopAllMovements();
        foreach (var cube in _allCubes)
        {
            if (cube != null) Destroy(cube.gameObject);
        }
        _allCubes.Clear();
        if (cubeSelector != null) cubeSelector.DeselectAll();
        if (GameBoard != null) GameBoard.ClearGrid();
    }
    #endregion
    
    #region Cube & Board Management
    /// <summary>
    /// Скрывает куб, перемещая его в специальный контейнер.
    /// </summary>
    public void HideCube(CubeController cubeToHide)
    {
        if (cubeToHide == null) return;
        RemoveCubeFromLogic(cubeToHide);
        cubeToHide.transform.SetParent(_hiddenCubesContainer);
    }
    
    /// <summary>
    /// Удаляет куб из всех логических систем (но не уничтожает GameObject).
    /// </summary>
    public void RemoveCubeFromLogic(CubeController cubeToRemove)
    {
        if (cubeToRemove == null) return;
        var gridPosition = GameBoard.WorldToGridPosition(cubeToRemove.transform.position);
        _allCubes.Remove(cubeToRemove);
        GameBoard.RemoveCubeAt(gridPosition);
        cubeSelector.Deselect(cubeToRemove);
    }

    /// <summary>
    /// Создает один куб со СЛУЧАЙНОЙ гранью в указанной ячейке.
    /// Используется для появления новых кубов во время игры.
    /// </summary>
    public CubeController SpawnCubeAt(Vector2Int gridPosition)
    {
        var newCube = CreateCubeAt(gridPosition);
        CubeFace randomFace = (CubeFace)Random.Range(1, 7);
        cubeRotator.RotateCubeTo(newCube, cubeRotator.GetRotationForFace(randomFace));

        return newCube;
    }
    
    /// <summary>
    /// Создает один куб в указанной ячейке БЕЗ установки вращения.
    /// Используется для воссоздания поля при загрузке.
    /// </summary>
    /// <param name="gridPosition">Координаты ячейки для создания куба.</param>
    /// <returns>Контроллер созданного куба.</returns>
    public CubeController CreateCubeAt(Vector2Int gridPosition)
    {
        Vector3 worldPos = GameBoard.GetWorldPosition(gridPosition);
        var newCube = cubeSpawner.SpawnSingleCube(worldPos);
        
        _allCubes.Add(newCube);
        GameBoard.PlaceCube(newCube, gridPosition);
        
        return newCube;
    }
    /// <summary>
    /// Переинициализирует CubeSelector новым списком всех кубов.
    /// </summary>
    public void ReinitializeSelector()
    {
        if (cubeSelector != null) cubeSelector.Initialize(_allCubes);
    }

    /// <summary>
    /// Обновляет целевое вращение и логическое состояние поворота поля.
    /// </summary>
    public void UpdateBoardRotationState(float angle)
    {
        _targetBoardRotation *= Quaternion.Euler(0, angle, 0);
        _boardRotationState = (angle > 0) ? (_boardRotationState + 1) % 4 : (_boardRotationState + 3) % 4;
    }

    private void UpdateBoardRotation()
    {
        if (boardContainer == null) return;
        boardContainer.rotation = Quaternion.RotateTowards(boardContainer.rotation, _targetBoardRotation, boardRotationSpeed * Time.deltaTime);
    }
    #endregion

    #region Public API for Input & UI

    /// <summary>
    /// Мгновенно устанавливает вращение игрового поля, минуя плавную анимацию.
    /// Используется для немедленной синхронизации состояния, например, при загрузке.
    /// </summary>
    public void SetBoardRotationInstantly(Quaternion newRotation)
    {
        if (boardContainer != null)
        {
            _targetBoardRotation = newRotation;
            boardContainer.rotation = newRotation;
        }
    }

    /// <summary>
    /// Принудительно синхронизирует состояние Match3Controller (в частности, гравитацию)
    /// с текущим состоянием поворота поля. Используется после загрузки.
    /// </summary>
    public void SyncMatch3State()
    {
        if (_currentMode == GameMode.Match3 && match3Controller != null)
        {
            float yAngle = Mathf.Round(boardContainer.rotation.eulerAngles.y / 90.0f) * 90.0f;
            _boardRotationState = (int)(yAngle / 90.0f);
            match3Controller.UpdateGravityFromState(_boardRotationState);
        }
    }
    /// <summary>
    /// Обрабатывает клик мыши для выделения. Делегируется из InputController.
    /// </summary>
    public void HandleSelectionClick(Vector3 screenPosition) => cubeSelector.HandleSelectionClick(screenPosition, GameBoard);
    
    /// <summary>
    /// Запускает действие простого перемещения. Делегируется в ActionController.
    /// </summary>
    public void ProcessMove(Vector2Int inputDir) => actionController.ProcessMove(inputDir);
    
    /// <summary>
    /// Запускает действие L-образного прыжка. Делегируется в ActionController.
    /// </summary>
    public void ProcessJumpStrafe() => actionController.ProcessJumpStrafe();
    
    /// <summary>
    /// Проверяет, возможно ли выполнить L-образный прыжок.
    /// </summary>
    public bool CanProcessJumpStrafe() => cubeSelector.SelectedCubes.Count == 1;
    
    /// <summary>
    /// Запускает действие относительного вращения. Делегируется в ActionController.
    /// </summary>
    public void ProcessRelativeRotation(Vector3 axis, float angle) => actionController.ProcessRelativeRotation(axis, angle);
    
    /// <summary>
    /// Запускает действие абсолютного вращения. Делегируется в ActionController.
    /// </summary>
    public void ProcessAbsoluteRotation(CubeFace face) => actionController.ProcessAbsoluteRotation(face);
    
    /// <summary>
    /// Запускает действие обмена состояний. Делегируется в ActionController.
    /// </summary>
    public void ProcessStateSwap() => actionController.ProcessStateSwap();
    
    /// <summary>
    /// Запускает действие случайного вращения. Делегируется в ActionController.
    /// </summary>
    public void ProcessRandomization() => actionController.ProcessRandomization();
    
    /// <summary>
    /// Запускает действие сдвига всего поля. Делегируется в ActionController.
    /// </summary>
    public void ProcessSettle(Vector2Int inputDir) => actionController.ProcessSettle(inputDir);
    
    /// <summary>
    /// Запускает действие вращения всего поля. Делегируется в ActionController.
    /// </summary>
    public void ProcessBoardRotation(float angle) => actionController.ProcessBoardRotation(angle);
    
    // UI команды
    public void RandomizeSelectedCubes() => cubeRotator.RotateCubesRandomly(cubeSelector.SelectedCubes);
    public void RandomizeUnselectedCubes() => cubeRotator.RotateCubesRandomly(cubeSelector.GetUnselectedCubes());
    public void DeselectAllCubes() => cubeSelector.DeselectAll();
    public void SelectCubesByCategory(CubeFace category) => cubeSelector.SelectByCategory(category);
    public void RotateSelectedCubesToFace(CubeFace face) => cubeRotator.RotateCubesToFace(cubeSelector.SelectedCubes, face);
    #endregion
}