using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Управляет игровым циклом и правилами режима Match-3.
/// </summary>
public class Match3Controller : MonoBehaviour
{
    #region Private State
    private GameManager _gameManager;
    private GameBoard _gameBoard;
    private CubeMover _cubeMover;
    private CubeSpawner _cubeSpawner;
    private Vector2Int _gravityDirection = Vector2Int.down;
    private bool _isProcessing = false;
    #endregion

    #region Public Properties & Events
    /// <summary>
    /// Текущее количество ходов игрока.
    /// </summary>
    public int TurnCount { get; private set; }
    
    /// <summary>
    /// Общее количество созданных кубов за сессию.
    /// </summary>
    public int CubesCreated { get; private set; }
    
    /// <summary>
    /// Общее количество уничтоженных кубов за сессию.
    /// </summary>
    public int CubesDestroyed { get; private set; }
    
    /// <summary>
    /// Событие, вызываемое при изменении любой статистики.
    /// </summary>
    public event System.Action OnStatsUpdated;

    /// <summary>
    /// Корутина текущего активного процесса обработки поля.
    /// GameManager ждет ее завершения.
    /// </summary>
    public Coroutine ActiveCoroutine { get; private set; }
    #endregion

    #region Public Methods
    /// <summary>
    /// Инициализирует модуль необходимыми ссылками на системы.
    /// </summary>
    public void Initialize(GameManager gameManager, GameBoard gameBoard, CubeMover cubeMover, CubeSpawner cubeSpawner)
    {
        _gameManager = gameManager;
        _gameBoard = gameBoard;
        _cubeMover = cubeMover;
        _cubeSpawner = cubeSpawner;
    }

    /// <summary>
    /// Запускает игровую сессию в режиме Match-3, обрабатывая начальное состояние поля.
    /// </summary>
    public void StartGame()
    {
        if (_isProcessing) return;
        ActiveCoroutine = StartCoroutine(StartGameCoroutine());
    }
    
    /// <summary>
    /// Запускается после любого действия игрока, увеличивает счетчик ходов и запускает обработку поля.
    /// </summary>
    public void OnPlayerAction()
    {
        if (_isProcessing || _gameManager.IsLoading) return;
        TurnCount++;
        OnStatsUpdated?.Invoke();
        ActiveCoroutine = StartCoroutine(ProcessBoardStateCoroutine(false));
    }
    
    /// <summary>
    /// Обновляет направление гравитации на основе состояния поворота поля.
    /// </summary>
    public void UpdateGravityFromState(int rotationState)
    {
        switch (rotationState)
        {
            case 0: _gravityDirection = Vector2Int.down; break;
            case 1: _gravityDirection = Vector2Int.right; break;
            case 2: _gravityDirection = Vector2Int.up; break;
            case 3: _gravityDirection = Vector2Int.left; break;
        }
    }

    /// <summary>
    /// Устанавливает значения счетчиков из загруженного состояния.
    /// </summary>
    public void LoadStats(WorldState savedState)
    {
        if (savedState == null) return;
        TurnCount = savedState.TurnCount;
        CubesCreated = savedState.CubesCreated;
        CubesDestroyed = savedState.CubesDestroyed;
        OnStatsUpdated?.Invoke();
    }
    
    /// <summary>
    /// Возвращает текущее направление гравитации (необходимо для сложных ходов в GameManager).
    /// </summary>
    public Vector2Int GetCurrentGravityDirection()
    {
        return _gravityDirection;
    }
    #endregion

    #region Coroutines
    private IEnumerator StartGameCoroutine()
    {
        TurnCount = 0;
        CubesCreated = 0;
        CubesDestroyed = 0;
        OnStatsUpdated?.Invoke();
        yield return StartCoroutine(ProcessBoardStateCoroutine(true));
    }
    
    private IEnumerator ProcessBoardStateCoroutine(bool isInitialState)
    {
        _isProcessing = true;
        bool changedInCycle;

        do
        {
            changedInCycle = false;
            
            // Фаза 1: Уничтожение
            List<CubeController> cubesToDestroy = FindAllMatches();
            if (cubesToDestroy.Count > 0)
            {
                changedInCycle = true;
                CubesDestroyed += cubesToDestroy.Count;
                OnStatsUpdated?.Invoke();
                foreach (var cube in cubesToDestroy)
                {
                    _gameManager.HideCube(cube);
                }
                yield return new WaitForSeconds(0.1f);
            }

            // Фаза 2: Гравитация
            var moveResults = _gameBoard.SettleBoard(_gravityDirection);
            if (moveResults.Count > 0)
            {
                changedInCycle = true;
                foreach (var result in moveResults)
                {
                    Vector3 worldPos = _gameBoard.GetWorldPosition(result.To);
                    _cubeMover.MoveCubeTo(result.Cube, worldPos);
                }
                while (_cubeMover.IsBusy())
                {
                    yield return null;
                }
            }
        } while (changedInCycle);

        // Фаза 3: Появление нового куба
        if (!isInitialState)
        {
            yield return StartCoroutine(SpawnNewCubesCoroutine(1));
        }

        _isProcessing = false;
    }

    private IEnumerator SpawnNewCubesCoroutine(int amount)
    {
        var emptySpawnCells = new List<Vector2Int>();
        switch (_gravityDirection)
        {
            case var _ when _gravityDirection == Vector2Int.down:
                for (int x = 0; x < _gameBoard.Width; x++)
                {
                    var pos = new Vector2Int(x, _gameBoard.Height - 1);
                    if (_gameBoard.GetCubeAt(pos) == null) emptySpawnCells.Add(pos);
                }
                break;
            case var _ when _gravityDirection == Vector2Int.up:
                for (int x = 0; x < _gameBoard.Width; x++)
                {
                    var pos = new Vector2Int(x, 0);
                    if (_gameBoard.GetCubeAt(pos) == null) emptySpawnCells.Add(pos);
                }
                break;
            case var _ when _gravityDirection == Vector2Int.left:
                for (int y = 0; y < _gameBoard.Height; y++)
                {
                    var pos = new Vector2Int(_gameBoard.Width - 1, y);
                    if (_gameBoard.GetCubeAt(pos) == null) emptySpawnCells.Add(pos);
                }
                break;
            case var _ when _gravityDirection == Vector2Int.right:
                for (int y = 0; y < _gameBoard.Height; y++)
                {
                    var pos = new Vector2Int(0, y);
                    if (_gameBoard.GetCubeAt(pos) == null) emptySpawnCells.Add(pos);
                }
                break;
        }
        
        if (emptySpawnCells.Count == 0)
        {
            HandleGameOver();
            yield break;
        }
        
        emptySpawnCells = emptySpawnCells.OrderBy(p => Random.value).ToList();
        int spawnCount = Mathf.Min(amount, emptySpawnCells.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            var spawnPos = emptySpawnCells[i];
            _gameManager.SpawnCubeAt(spawnPos);
            CubesCreated++;
            OnStatsUpdated?.Invoke();
            yield return new WaitForSeconds(0.1f);
        }
        
        if (spawnCount > 0)
        {
            yield return StartCoroutine(ProcessBoardStateCoroutine(true));
        }
    }
    #endregion

    #region Private Methods
    private void HandleGameOver()
    {
        Debug.LogWarning("GAME OVER: Нет места для появления новых кубов!");
        _isProcessing = false;
    }
  
    private List<CubeController> FindAllMatches()
    {
        var matchedCubes = new HashSet<CubeController>();
        
        // Горизонтальный проход
        for (int y = 0; y < _gameBoard.Height; y++)
        {
            for (int x = 0; x <= _gameBoard.Width - 3; )
            {
                var startCube = _gameBoard.GetCubeAt(new Vector2Int(x, y));
                if (startCube == null) { x++; continue; }

                var faceToMatch = startCube.GetTopFaceCategory();
                var line = new List<CubeController>();
                line.Add(startCube);

                for (int i = x + 1; i < _gameBoard.Width; i++)
                {
                    var nextCube = _gameBoard.GetCubeAt(new Vector2Int(i, y));
                    if (nextCube != null && nextCube.GetTopFaceCategory() == faceToMatch)
                    {
                        line.Add(nextCube);
                    }
                    else break;
                }
                
                if(line.Count >= 3)
                {
                    foreach (var cube in line) matchedCubes.Add(cube);
                }
                x += line.Count;
            }
        }
        
        // Вертикальный проход
        for (int x = 0; x < _gameBoard.Width; x++)
        {
            for (int y = 0; y <= _gameBoard.Height - 3; )
            {
                var startCube = _gameBoard.GetCubeAt(new Vector2Int(x, y));
                if (startCube == null) { y++; continue; }
                
                var faceToMatch = startCube.GetTopFaceCategory();
                var line = new List<CubeController>();
                line.Add(startCube);

                for (int i = y + 1; i < _gameBoard.Height; i++)
                {
                    var nextCube = _gameBoard.GetCubeAt(new Vector2Int(x, i));
                    if (nextCube != null && nextCube.GetTopFaceCategory() == faceToMatch)
                    {
                        line.Add(nextCube);
                    }
                    else break;
                }
                
                if (line.Count >= 3)
                {
                    foreach (var cube in line) matchedCubes.Add(cube);
                }
                y += line.Count;
            }
        }
        
        matchedCubes.RemoveWhere(cube => cube == null);
        return matchedCubes.ToList();
    }
    #endregion
}