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
    [SerializeField] private CubeMover cubeMover;
    [SerializeField] private CubeStateManager cubeStateManager;

    [Header("Настройки вращения поля")]
    [Tooltip("Объект, который будет вращаться (обычно родитель спавнера).")]
    [SerializeField] private Transform boardContainer;

    [Tooltip("Скорость вращения поля в градусах в секунду.")]
    [SerializeField] private float boardRotationSpeed = 90f;
    private Quaternion _targetBoardRotation;


    [Header("Игровые режимы")]
    [SerializeField] private GameOfLifeController gameOfLifeController;

    public GameBoard GameBoard { get; private set; }
    public CubeSelector CubeSelector => cubeSelector;
    public CubeRotator CubeRotator => cubeRotator;

    private List<CubeController> _allCubes = new List<CubeController>();
    private void InitializeGame()
    {
        _allCubes = cubeSpawner.SpawnCubes();

        GameBoard = new GameBoard();
        GameBoard.Initialize(_allCubes, cubeSpawner.CubeSpacing);

        cubeSelector.Initialize(_allCubes);
        cubeStateManager.Initialize(_allCubes, cubeRotator);

        //  ДЛЯ ТЕСТИРОВАНИЯ 
        // Создаем пустое пространство в центре поля для начала игры.
        Vector2Int center = new Vector2Int(GameBoard.Width / 2, GameBoard.Height / 2);
        DestroyCubeAt(center);
    }
    /// <summary>
    /// Запускает симуляцию игры "Жизнь", перед этим сбрасывая выделение кубов.
    /// </summary>
    public void ToggleGameOfLife()
    {
        if (gameOfLifeController != null)
        {

            CubeSelector.DeselectAll();
            gameOfLifeController.ToggleSimulation();
        }
    }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (boardContainer != null)
        {
            _targetBoardRotation = boardContainer.rotation;
        }

    }

    private void Start()
    {
        InitializeGame();
        if (gameOfLifeController != null)
        {
            // Обновляем вызов, передавая новый GameBoard
            gameOfLifeController.Initialize(GameBoard, cubeRotator, _allCubes);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            cubeSelector.HandleSelectionClick(Input.mousePosition, GameBoard);
        }
        HandleKeyboardInput();
    }


    private void HandleKeyboardInput()
    {
        // Снятие выделения
        if (Input.GetKeyDown(KeyCode.C))
        {
            cubeSelector.DeselectAll();
        }

        HandleMoveInput();
        HandleRotateInput();
        HandleSettleInput();
        HandleAbsoluteFaceInput(); 
        HandleStateSwapInput();
        UpdateBoardRotation();
        HandleRandomizeInput();
        HandleBoardRotationInput();


        if (Input.GetKeyDown(KeyCode.F5)) SaveState();
        if (Input.GetKeyDown(KeyCode.F9)) LoadState();
    }
    private void HandleRandomizeInput()
    {
        // Используем 'G' (от 'Generate') как клавишу действия.
        if (Input.GetKeyDown(KeyCode.G))
        {
            var targetCubes = Input.GetKey(KeyCode.LeftShift)
                ? cubeSelector.GetUnselectedCubes()
                : new List<CubeController>(cubeSelector.SelectedCubes);

            if (targetCubes.Count > 0)
            {
                cubeRotator.RotateCubesRandomly(targetCubes);
            }
        }
    }

    /// <summary>
    /// Обрабатывает ввод с клавиатуры для вращения кубов.
    /// </summary>
    private void HandleRotateInput()
    {
        // Используем 'R' (от 'Rotate') как клавишу-модификатор действия.
        if (Input.GetKey(KeyCode.R))
        {
            Vector3 axis = Vector3.zero;
            float angle = 90.0f;

            // W/S - вращение вокруг оси X (вперед/назад)
            if (Input.GetKeyDown(KeyCode.W)) axis = Vector3.right;
            if (Input.GetKeyDown(KeyCode.S)) axis = Vector3.left;

            // A/D - вращение вокруг оси Z (влево/вправо)
            if (Input.GetKeyDown(KeyCode.A)) axis = Vector3.forward;
            if (Input.GetKeyDown(KeyCode.D)) axis = Vector3.back;

            if (axis != Vector3.zero)
            {
                var targetCubes = Input.GetKey(KeyCode.LeftShift)
                    ? cubeSelector.GetUnselectedCubes()
                    : new List<CubeController>(cubeSelector.SelectedCubes);

                ProcessRotateCommand(targetCubes, axis, angle);
            }
        }
    }

    /// <summary>
    /// Выполняет команду вращения для указанных кубов.
    /// </summary>
    /// <param name="targetCubes">Список кубов для вращения.</param>
    /// <param name="axis">Ось вращения.</param>
    /// <param name="angle">Угол вращения.</param>
    private void ProcessRotateCommand(IReadOnlyList<CubeController> targetCubes, Vector3 axis, float angle)
    {
        if (targetCubes.Count == 0) return;

        cubeRotator.RotateCubesBy(targetCubes, axis, angle);
    }
    /// <summary>
    /// Обрабатывает ввод с клавиатуры для глобального сдвига ("2048-style").
    /// </summary>
    private void HandleSettleInput()
    {
        // Используем 'G' (от 'Gravity') как клавишу-модификатор действия.
        if (Input.GetKey(KeyCode.T)) 
        {
            Vector2Int direction = Vector2Int.zero;
            if (Input.GetKeyDown(KeyCode.W)) direction = Vector2Int.up;
            if (Input.GetKeyDown(KeyCode.S)) direction = Vector2Int.down;
            if (Input.GetKeyDown(KeyCode.A)) direction = Vector2Int.left;
            if (Input.GetKeyDown(KeyCode.D)) direction = Vector2Int.right;

            if (direction != Vector2Int.zero)
            {
                ProcessSettleCommand(direction);
            }
        }
    }
    /// <summary>
    /// Выполняет команду глобального сдвига всех кубов на доске.
    /// </summary>
    /// <param name="direction">Направление сдвига.</param>
    private void ProcessSettleCommand(Vector2Int direction)
    {
        var moveResults = GameBoard.SettleBoard(direction);

        if (moveResults.Count == 0) return;

        foreach (var result in moveResults)
        {
            Vector3 targetWorldPos = GameBoard.GridToWorldPosition(result.To);
            cubeMover.MoveCubeTo(result.Cube, targetWorldPos);
        }
    }

    /// <summary>
    /// Обрабатывает ввод с клавиатуры для перемещения кубов.
    /// </summary>
    private void HandleMoveInput()
    {
        // Используем 'V' (от 'moVe') как клавишу-модификатор действия.
        if (Input.GetKey(KeyCode.V))
        {
            Vector2Int direction = Vector2Int.zero;
            if (Input.GetKeyDown(KeyCode.W)) direction = Vector2Int.up;
            if (Input.GetKeyDown(KeyCode.S)) direction = Vector2Int.down;
            if (Input.GetKeyDown(KeyCode.A)) direction = Vector2Int.left;
            if (Input.GetKeyDown(KeyCode.D)) direction = Vector2Int.right;

            if (direction != Vector2Int.zero)
            {
                // LeftShift определяет цель: "невыбранные". По умолчанию - "выбранные".
                var targetCubes = Input.GetKey(KeyCode.LeftShift) 
                    ? cubeSelector.GetUnselectedCubes() 
                    : new List<CubeController>(cubeSelector.SelectedCubes); // Создаем копию на случай изменений

                ProcessMoveCommand(targetCubes, direction);
            }
        }
    }

    /// <summary>
    /// Выполняет команду перемещения для указанных кубов.
    /// </summary>
    /// <param name="targetCubes">Список кубов для перемещения.</param>
    /// <param name="direction">Направление перемещения.</param>
    private void ProcessMoveCommand(IReadOnlyList<CubeController> targetCubes, Vector2Int direction)
    {
        if (targetCubes.Count == 0) return;
        
        var moveResults = GameBoard.MoveCubes(targetCubes, direction);

        foreach (var result in moveResults)
        {
            Vector3 targetWorldPos = GameBoard.GridToWorldPosition(result.To);
            cubeMover.MoveCubeTo(result.Cube, targetWorldPos);
        }
    }
    /// <summary>
    /// Обрабатывает ввод для установки абсолютной грани кубов.
    /// </summary>
    private void HandleAbsoluteFaceInput()
    {
        // Используем 'F' (от 'Face') как клавишу-модификатор действия.
        if (Input.GetKey(KeyCode.F))
        {
            for (int i = 1; i <= 6; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
                {
                    var targetCubes = Input.GetKey(KeyCode.LeftShift)
                        ? cubeSelector.GetUnselectedCubes()
                        : new List<CubeController>(cubeSelector.SelectedCubes);

                    if (targetCubes.Count > 0)
                    {
                        cubeRotator.RotateCubesToFace(targetCubes, (CubeFace)i);
                    }
                }
            }
        }
    }
    /// <summary>
    /// Обрабатывает ввод для обмена состояниями (вращением) двух кубов.
    /// </summary>
    private void HandleStateSwapInput()
    {
        // Используем 'X' (от 'eXchange') как клавишу действия.
        if (Input.GetKeyDown(KeyCode.X))
        {
            ProcessStateSwapCommand();
        }
    }

    /// <summary>
    /// Выполняет команду обмена состояниями для двух выделенных кубов.
    /// </summary>
    private void ProcessStateSwapCommand()
    {
        var selectedCubes = cubeSelector.SelectedCubes;
        if (selectedCubes.Count != 2)
        {
            Debug.LogWarning("Для обмена состояниями нужно выделить ровно два куба.");
            return;
        }

        var cubeA = selectedCubes[0];
        var cubeB = selectedCubes[1];

        Quaternion rotationA = cubeRotator.GetTargetRotation(cubeA);
        Quaternion rotationB = cubeRotator.GetTargetRotation(cubeB);

        cubeRotator.RotateCubeTo(cubeA, rotationB);
        cubeRotator.RotateCubeTo(cubeB, rotationA);

        cubeSelector.DeselectAll();
    }


    private void HandleBoardRotationInput()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            _targetBoardRotation *= Quaternion.Euler(0, 90, 0);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            _targetBoardRotation *= Quaternion.Euler(0, -90, 0);
        }
    }

    private void UpdateBoardRotation()
    {
        if (boardContainer == null) return;

        boardContainer.rotation = Quaternion.RotateTowards(boardContainer.rotation, _targetBoardRotation, boardRotationSpeed * Time.deltaTime);
    }


    /// <summary>
    /// Полностью удаляет куб по указанным координатам сетки.
    /// </summary>
    /// <param name="gridPosition">Координаты куба в сетке.</param>
    private void DestroyCubeAt(Vector2Int gridPosition)
    {
        CubeController cubeToDestroy = GameBoard.GetCubeAt(gridPosition);
        if (cubeToDestroy != null)
        {
            // Удаляем из всех списков и систем
            _allCubes.Remove(cubeToDestroy);
            cubeSelector.Initialize(_allCubes); // Переинициализируем селектор с новым списком
            GameBoard.RemoveCubeAt(gridPosition);

            // Уничтожаем игровой объект
            Destroy(cubeToDestroy.gameObject);
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