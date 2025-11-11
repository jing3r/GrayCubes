using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Управляет логическим представлением игровой доски.
/// Хранит данные о кубах в двумерной сетке и предоставляет методы для работы с ней,
/// включая преобразование координат для повернутого поля.
/// </summary>
public class GameBoard
{
    #region Private State
    private CubeController[,] _grid;
    private float _spacing;
    private Transform _boardTransform;
    #endregion

    #region Public Properties
    /// <summary>
    /// Ссылка на Transform игрового поля для преобразования координат.
    /// </summary>
    public Transform BoardTransform => _boardTransform;
    
    /// <summary>
    /// Ширина доски в ячейках.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Высота доски в ячейках.
    /// </summary>
    public int Height { get; private set; }
    #endregion

    #region Initialization
    /// <summary>
    /// Инициализирует доску на основе списка кубов и ссылки на ее Transform.
    /// </summary>
    public void Initialize(IReadOnlyList<CubeController> allCubes, float spacing, Transform boardTransform)
    {
        if (allCubes == null || boardTransform == null)
        {
            Debug.LogError("Невозможно инициализировать доску: не предоставлены все необходимые данные.");
            return;
        }

        _spacing = spacing;
        _boardTransform = boardTransform;

        if (allCubes.Count == 0)
        {
            Width = 0;
            Height = 0;
            _grid = new CubeController[0,0];
            return;
        }

        Width = Mathf.RoundToInt(Mathf.Sqrt(allCubes.Count));
        Height = Width; 
        _grid = new CubeController[Width, Height];
        
        foreach (var cube in allCubes)
        {
            Vector2Int gridPos = WorldToGridPosition(cube.transform.position);
            if (IsInBounds(gridPos))
            {
                _grid[gridPos.x, gridPos.y] = cube;
            }
        }
    }
    #endregion

    #region Grid Access & Manipulation
    /// <summary>
    /// Возвращает контроллер куба по координатам сетки.
    /// </summary>
    public CubeController GetCubeAt(Vector2Int position)
    {
        if (!IsInBounds(position)) return null;
        return _grid[position.x, position.y];
    }
    
    /// <summary>
    /// Удаляет куб из указанной ячейки сетки (только из модели данных).
    /// </summary>
    public void RemoveCubeAt(Vector2Int position)
    {
        if (IsInBounds(position)) _grid[position.x, position.y] = null;
    }
    
    /// <summary>
    /// Помещает куб в указанную ячейку сетки.
    /// </summary>
    public void PlaceCube(CubeController cube, Vector2Int position)
    {
        if (IsInBounds(position)) _grid[position.x, position.y] = cube;
    }

    /// <summary>
    /// Полностью очищает логическую сетку.
    /// </summary>
    public void ClearGrid()
    {
        if (_grid == null) return;
        System.Array.Clear(_grid, 0, _grid.Length);
    }
    
    /// <summary>
    /// Проверяет, находятся ли координаты в пределах доски.
    /// </summary>
    public bool IsInBounds(Vector2Int position)
    {
        return position.x >= 0 && position.x < Width && position.y >= 0 && position.y < Height;
    }
    #endregion

    #region Coordinate Conversion
    /// <summary>
    /// Преобразует координаты сетки в МИРОВЫЕ координаты с учетом поворота поля.
    /// </summary>
    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        float localX = -(Width - 1) * _spacing / 2.0f + gridPosition.x * _spacing;
        float localZ = -(Height - 1) * _spacing / 2.0f + gridPosition.y * _spacing;
        Vector3 localPos = new Vector3(localX, 0, localZ);
        return _boardTransform.TransformPoint(localPos);
    }
    
    /// <summary>
    /// Преобразует МИРОВЫЕ координаты в координаты на сетке с учетом поворота поля.
    /// </summary>
    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        Vector3 localPos = _boardTransform.InverseTransformPoint(worldPosition);
        int x = Mathf.RoundToInt((localPos.x / _spacing) + (Width - 1) / 2.0f);
        int y = Mathf.RoundToInt((localPos.z / _spacing) + (Height - 1) / 2.0f);
        return new Vector2Int(x, y);
    }
    #endregion

    #region Movement Logic
    /// <summary>
    /// Пытается переместить указанные кубы на один шаг в заданном направлении ("Sokoban-style").
    /// </summary>
    public List<MoveResult> MoveCubes(IReadOnlyList<CubeController> cubesToMove, Vector2Int direction)
    {
        var validMoves = new List<MoveResult>();
        foreach (var cube in cubesToMove)
        {
            if (cube == null) continue;
            var fromPos = WorldToGridPosition(cube.transform.position);
            var toPos = fromPos + direction;
            if (!IsInBounds(toPos) || GetCubeAt(toPos) != null) continue;
            validMoves.Add(new MoveResult(cube, fromPos, toPos));
        }
        foreach (var move in validMoves)
        {
            _grid[move.From.x, move.From.y] = null;
            _grid[move.To.x, move.To.y] = move.Cube;
        }
        return validMoves;
    }
    
    /// <summary>
    /// Сдвигает все кубы на доске в указанном направлении до упора ("2048-style").
    /// </summary>
    public List<MoveResult> SettleBoard(Vector2Int direction)
    {
        var results = new List<MoveResult>();
        int width = Width;
        int height = Height;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                int x = (direction.y != 0) ? i : (direction.x > 0 ? width - 1 - j : j);
                int y = (direction.x != 0) ? i : (direction.y > 0 ? height - 1 - j : j);
                var cube = GetCubeAt(new Vector2Int(x, y));
                if (cube == null) continue;
                var currentPos = new Vector2Int(x, y);
                var targetPos = currentPos;
                var nextPos = currentPos + direction;
                while (IsInBounds(nextPos) && GetCubeAt(nextPos) == null)
                {
                    targetPos = nextPos;
                    nextPos += direction;
                }
                if (targetPos != currentPos)
                {
                    _grid[targetPos.x, targetPos.y] = cube;
                    _grid[currentPos.x, currentPos.y] = null;
                    results.Add(new MoveResult(cube, currentPos, targetPos));
                }
            }
        }
        return results;
    }

    /// <summary>
    /// Выполняет L-образное перемещение "вверх-вбок" (против гравитации).
    /// </summary>
    public MoveResult? JumpStrafe(CubeController cube, Vector2Int gravityDirection, Vector2Int strafeDirection)
    {
        var fromPos = WorldToGridPosition(cube.transform.position);
        var intermediatePos = fromPos - gravityDirection;
        var finalPos = intermediatePos + strafeDirection;

        if (IsInBounds(intermediatePos) && GetCubeAt(intermediatePos) == null &&
            IsInBounds(finalPos) && GetCubeAt(finalPos) == null)
        {
            _grid[fromPos.x, fromPos.y] = null;
            _grid[finalPos.x, finalPos.y] = cube;
            return new MoveResult(cube, fromPos, finalPos);
        }
        return null;
    }
    #endregion
    
    #region Query Methods
    /// <summary>
    /// Находит все связанные кубы одного типа (одинаковая верхняя грань), используя поиск в ширину.
    /// </summary>
    public List<CubeController> GetConnectedArea(Vector2Int startPosition)
    {
        var area = new List<CubeController>();
        var startCube = GetCubeAt(startPosition);
        if (startCube == null) return area;
        var targetFace = startCube.GetTopFaceCategory();
        var queue = new Queue<Vector2Int>();
        var visited = new HashSet<Vector2Int>();
        queue.Enqueue(startPosition);
        visited.Add(startPosition);
        while (queue.Count > 0)
        {
            var currentPos = queue.Dequeue();
            var currentCube = GetCubeAt(currentPos);
            if (currentCube == null) continue;
            area.Add(currentCube);
            var directions = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var dir in directions)
            {
                var neighborPos = currentPos + dir;
                if (IsInBounds(neighborPos) && !visited.Contains(neighborPos))
                {
                    visited.Add(neighborPos);
                    var neighborCube = GetCubeAt(neighborPos);
                    if (neighborCube != null && neighborCube.GetTopFaceCategory() == targetFace)
                    {
                        queue.Enqueue(neighborPos);
                    }
                }
            }
        }
        return area;
    }
    
    /// <summary>
    /// Возвращает список соседей для указанной ячейки (тороидальный мир).
    /// </summary>
    public List<CubeController> GetNeighbors(Vector2Int position)
    {
        var neighbors = new List<CubeController>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;
                int neighborX = (position.x + x + Width) % Width;
                int neighborY = (position.y + y + Height) % Height;
                var neighborCube = _grid[neighborX, neighborY];
                if (neighborCube != null)
                {
                    neighbors.Add(neighborCube);
                }
            }
        }
        return neighbors;
    }
    #endregion
}