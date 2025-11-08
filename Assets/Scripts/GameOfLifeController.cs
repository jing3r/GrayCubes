using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Управляет симуляцией игры "Жизнь" Конвея на поле из кубов.
/// Использует централизованный GameBoard для получения данных о сетке.
/// </summary>
public class GameOfLifeController : MonoBehaviour
{
    [Header("Настройки симуляции")]
    [Tooltip("Задержка между поколениями в секундах.")]
    [SerializeField] private float stepInterval = 2.0f;

    [Header("Настройки визуализации")]
    [Tooltip("Грань, представляющая 'живую' клетку.")]
    [SerializeField] private CubeFace aliveFace = CubeFace.Down;
    [Tooltip("Грань, представляющая 'мертвую' клетку.")]
    [SerializeField] private CubeFace deadFace = CubeFace.Up;

    public int GenerationCount { get; private set; }
    public bool IsRunning => _simulationCoroutine != null;
    public event System.Action<int> OnGenerationUpdated;

    private GameBoard _gameBoard;
    private CubeRotator _cubeRotator;
    private Coroutine _simulationCoroutine;
    private List<CubeController> _allCubesSnapshot; // Храним список для обхода

    /// <summary>
    /// Инициализирует контроллер необходимыми зависимостями.
    /// </summary>
    public void Initialize(GameBoard gameBoard, CubeRotator cubeRotator, IReadOnlyList<CubeController> allCubes)
    {
        _gameBoard = gameBoard;
        _cubeRotator = cubeRotator;
        _allCubesSnapshot = new List<CubeController>(allCubes);
    }

    /// <summary>
    /// Запускает или останавливает симуляцию.
    /// </summary>
    public void ToggleSimulation()
    {
        if (IsRunning)
        {
            StopCoroutine(_simulationCoroutine);
            _simulationCoroutine = null;
            Debug.Log("Симуляция 'Жизни' остановлена.");
        }
        else
        {
            if (_gameBoard == null)
            {
                Debug.LogError("GameOfLifeController не инициализирован!");
                return;
            }
            GenerationCount = 0;
            OnGenerationUpdated?.Invoke(GenerationCount);
            BinarizeCurrentState();
            _simulationCoroutine = StartCoroutine(SimulationLoop());
            Debug.Log("Симуляция 'Жизни' запущена.");
        }
    }

    private void BinarizeCurrentState()
    {
        var cubesToMakeAlive = new List<CubeController>();
        var cubesToMakeDead = new List<CubeController>();

        foreach (var cube in _allCubesSnapshot)
        {
            if ((int)cube.GetTopFaceCategory() >= 4)
                cubesToMakeAlive.Add(cube);
            else
                cubesToMakeDead.Add(cube);
        }
        _cubeRotator.RotateCubesToFace(cubesToMakeAlive, aliveFace);
        _cubeRotator.RotateCubesToFace(cubesToMakeDead, deadFace);
    }
    
    private IEnumerator SimulationLoop()
    {
        // Даем время на завершение анимации бинаризации
        yield return new WaitForSeconds(stepInterval);

        while (true)
        {
            PerformSimulationStep();
            yield return new WaitForSeconds(stepInterval);
        }
    }
    
    private void PerformSimulationStep()
    {
        var changes = new Dictionary<CubeController, bool>();

        foreach (var cube in _allCubesSnapshot)
        {
            int aliveNeighbors = CountAliveNeighbors(cube);
            bool isCurrentlyAlive = IsCubeAlive(cube);

            if (isCurrentlyAlive && (aliveNeighbors < 2 || aliveNeighbors > 3))
                changes[cube] = false; // Умирает
            else if (!isCurrentlyAlive && aliveNeighbors == 3)
                changes[cube] = true; // Рождается
        }
        
        ApplyChanges(changes);

        if (changes.Count > 0)
        {
            GenerationCount++;
            OnGenerationUpdated?.Invoke(GenerationCount);
        }
    }

    private int CountAliveNeighbors(CubeController cube)
    {
        Vector2Int gridPos = _gameBoard.WorldToGridPosition(cube.transform.position);
        List<CubeController> neighbors = _gameBoard.GetNeighbors(gridPos);
        
        return neighbors.Count(IsCubeAlive);
    }

    private bool IsCubeAlive(CubeController cube)
    {
        var currentFace = cube.GetTopFaceCategory();
        if (currentFace == aliveFace) return true;
        if (currentFace == deadFace) return false;
        
        // Резервная логика, если куб не находится в бинарном состоянии
        return (int)currentFace >= 4;
    }

    private void ApplyChanges(Dictionary<CubeController, bool> changes)
    {
        if (changes.Count == 0) return;

        var cubesToMakeAlive = changes.Where(c => c.Value).Select(c => c.Key).ToList();
        var cubesToMakeDead = changes.Where(c => !c.Value).Select(c => c.Key).ToList();

        _cubeRotator.RotateCubesToFace(cubesToMakeAlive, aliveFace);
        _cubeRotator.RotateCubesToFace(cubesToMakeDead, deadFace);
    }
}