using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Управляет симуляцией игры "Жизнь" Конвея на поле из кубов.
/// Поддерживает тороидальный (зацикленный) мир.
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

    private List<CubeController> _allCubes;
    private Dictionary<Vector2Int, CubeController> _grid = new Dictionary<Vector2Int, CubeController>();
    private CubeRotator _cubeRotator;
    private Coroutine _simulationCoroutine;
    
    private float _spacing;
    private int _gridWidth, _gridHeight;
    
    /// <summary>
    /// Инициализирует контроллер необходимыми зависимостями и данными о поле.
    /// </summary>
    public void Initialize(List<CubeController> allCubes, CubeRotator cubeRotator, float cubeSpacing)
    {
        _allCubes = allCubes;
        _cubeRotator = cubeRotator;
        _spacing = cubeSpacing;
        BuildGrid();
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
            GenerationCount = 0;
            OnGenerationUpdated?.Invoke(GenerationCount);
            BinarizeCurrentState();
            _simulationCoroutine = StartCoroutine(SimulationLoop());
            Debug.Log("Симуляция 'Жизни' запущена.");
        }
    }
    
    private void BuildGrid()
    {
        if (_allCubes == null || _allCubes.Count == 0) return;

        var minX = _allCubes.Min(c => c.transform.position.x);
        var minZ = _allCubes.Min(c => c.transform.position.z);
        
        _grid.Clear();
        _gridWidth = 0;
        _gridHeight = 0;

        foreach (var cube in _allCubes)
        {
            int x = Mathf.RoundToInt((cube.transform.position.x - minX) / _spacing);
            int y = Mathf.RoundToInt((cube.transform.position.z - minZ) / _spacing);
            var gridPos = new Vector2Int(x, y);

            _grid[gridPos] = cube;

            if (x > _gridWidth) _gridWidth = x;
            if (y > _gridHeight) _gridHeight = y;
        }
        
        _gridWidth++;
        _gridHeight++;
    }

    private void BinarizeCurrentState()
    {
        var cubesToMakeAlive = new List<CubeController>();
        var cubesToMakeDead = new List<CubeController>();

        foreach (var cube in _allCubes)
        {
            // Логика определения состояния по принципу "темный/светлый"
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

        foreach (var cube in _allCubes)
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
        int count = 0;
        var gridPos = GetGridPosition(cube.transform.position);

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int neighborX = (gridPos.x + x + _gridWidth) % _gridWidth;
                int neighborY = (gridPos.y + y + _gridHeight) % _gridHeight;
                
                if (_grid.TryGetValue(new Vector2Int(neighborX, neighborY), out var neighbor) && IsCubeAlive(neighbor))
                {
                    count++;
                }
            }
        }
        return count;
    }

    // Этот метод я разделил, чтобы GetGridPosition не зависел от полей класса
    private Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        var minPos = new Vector3(_grid.Keys.Min(p => p.x), 0, _grid.Keys.Min(p => p.y));
        minPos.x = minPos.x * _spacing + _allCubes[0].transform.position.x;
        minPos.z = minPos.z * _spacing + _allCubes[0].transform.position.z;

        return GetGridPosition(worldPosition, minPos, _spacing);
    }

    // Статический метод, который можно тестировать независимо
    private static Vector2Int GetGridPosition(Vector3 worldPos, Vector3 originPos, float spacing)
    {
        int x = Mathf.RoundToInt((worldPos.x - originPos.x) / spacing);
        int y = Mathf.RoundToInt((worldPos.z - originPos.z) / spacing);
        return new Vector2Int(x, y);
    }


    private bool IsCubeAlive(CubeController cube)
    {
        var currentFace = cube.GetTopFaceCategory();
        if (currentFace == aliveFace) return true;
        if (currentFace == deadFace) return false;
        
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