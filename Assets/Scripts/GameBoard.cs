using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Структура для описания результата операции перемещения.
/// </summary>
public readonly struct MoveResult
{
    public readonly CubeController Cube;
    public readonly Vector2Int From;
    public readonly Vector2Int To;

    public MoveResult(CubeController cube, Vector2Int from, Vector2Int to)
    {
        Cube = cube;
        From = from;
        To = to;
    }
}

/// <summary>
/// Управляет логическим представлением игровой доски.
/// Хранит данные о кубах в двумерной сетке и предоставляет методы для работы с ней.
/// </summary>
public class GameBoard
{
    private CubeController[,] _grid;
    private Vector3 _originPosition;
    private float _spacing;

    /// <summary>
    /// Ширина доски в ячейках.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Высота доски в ячейках.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Инициализирует доску на основе списка созданных кубов.
    /// </summary>
    /// <param name="allCubes">Список всех CubeController на сцене.</param>
    /// <param name="spacing">Расстояние между центрами кубов.</param>
    public void Initialize(IReadOnlyList<CubeController> allCubes, float spacing)
    {
        if (allCubes == null || allCubes.Count == 0)
        {
            Debug.LogError("Невозможно инициализировать доску: список кубов пуст.");
            return;
        }

        _spacing = spacing;

        var minX = allCubes.Min(c => c.transform.position.x);
        var minZ = allCubes.Min(c => c.transform.position.z);
        float planeY = allCubes[0].transform.position.y;
        _originPosition = new Vector3(minX, planeY, minZ);
    
        // Определяем размеры сетки
        var maxX = allCubes.Max(c => c.transform.position.x);
        var maxZ = allCubes.Max(c => c.transform.position.z);
        Width = Mathf.RoundToInt((maxX - minX) / spacing) + 1;
        Height = Mathf.RoundToInt((maxZ - minZ) / spacing) + 1;

        _grid = new CubeController[Width, Height];

        foreach (var cube in allCubes)
        {
            Vector2Int gridPos = WorldToGridPosition(cube.transform.position);
            _grid[gridPos.x, gridPos.y] = cube;
        }
    }

    /// <summary>
    /// Возвращает контроллер куба по координатам сетки.
    /// </summary>
    /// <param name="position">Координаты ячейки.</param>
    /// <returns>CubeController или null, если ячейка пуста или вне границ.</returns>
    public CubeController GetCubeAt(Vector2Int position)
    {
        if (position.x < 0 || position.x >= Width || position.y < 0 || position.y >= Height)
        {
            return null; // За границами поля
        }
        return _grid[position.x, position.y];
    }
    
    /// <summary>
    /// Преобразует мировые координаты в координаты на сетке.
    /// </summary>
    /// <param name="worldPosition">Позиция в мировом пространстве.</param>
    /// <returns>Координаты в сетке (x, y).</returns>
    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt((worldPosition.x - _originPosition.x) / _spacing);
        int y = Mathf.RoundToInt((worldPosition.z - _originPosition.z) / _spacing);
        return new Vector2Int(x, y);
    }
    
    /// <summary>
    /// Преобразует координаты сетки в мировые координаты.
    /// </summary>
    /// <param name="gridPosition">Координаты ячейки.</param>
    /// <returns>Центр ячейки в мировом пространстве.</returns>
    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        float x = _originPosition.x + gridPosition.x * _spacing;
        float z = _originPosition.z + gridPosition.y * _spacing;
        return new Vector3(x, _originPosition.y, z);
    }

    /// <summary>
    /// Возвращает список соседей для указанной ячейки (тороидальный мир).
    /// </summary>
    /// <param name="position">Координаты центральной ячейки.</param>
    /// <returns>Список из 8 соседей.</returns>
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

    /// <summary>
    /// Пытается переместить указанные кубы на один шаг в заданном направлении ("Sokoban-style").
    /// Перемещение для каждого куба происходит только если целевая ячейка свободна и находится в пределах поля.
    /// </summary>
    /// <param name="cubesToMove">Список кубов, которые нужно попытаться переместить.</param>
    /// <param name="direction">Направление смещения (например, (0, 1) для 'вверх').</param>
    /// <returns>Список успешно выполненных перемещений.</returns>
    public List<MoveResult> MoveCubes(IReadOnlyList<CubeController> cubesToMove, Vector2Int direction)
    {
        var validMoves = new List<MoveResult>();

        // Этап 1: Определяем, какие ходы в принципе возможны, не меняя состояние сетки.
        foreach (var cube in cubesToMove)
        {
            var fromPos = WorldToGridPosition(cube.transform.position);
            var toPos = fromPos + direction;

            // Проверка 1: Цель должна быть в пределах игрового поля.
            if (!IsInBounds(toPos))
            {
                continue; // Этот куб упирается в край. Ход невозможен.
            }

            // Проверка 2: Целевая ячейка должна быть свободна.
            if (GetCubeAt(toPos) != null)
            {
                continue; // Этот куб упирается в другой куб. Ход невозможен.
            }

            // Если все проверки пройдены, этот ход считается валидным.
            validMoves.Add(new MoveResult(cube, fromPos, toPos));
        }

        // Этап 2: Применяем к сетке только валидные ходы.
        // Это гарантирует, что мы не изменим состояние сетки до того, как проверим все кубы.
        foreach (var move in validMoves)
        {
            _grid[move.From.x, move.From.y] = null;
            _grid[move.To.x, move.To.y] = move.Cube;
        }

        return validMoves;
    }

    /// <summary>
    /// Удаляет куб из указанной ячейки сетки (только из модели данных).
    /// </summary>
    /// <param name="position">Координаты ячейки для очистки.</param>
    public void RemoveCubeAt(Vector2Int position)
    {
        if (position.x < 0 || position.x >= Width || position.y < 0 || position.y >= Height)
        {
            return;
        }
        _grid[position.x, position.y] = null;
    }
    /// <summary>
    /// Сдвигает все кубы на доске в указанном направлении до упора ("2048-style").
    /// </summary>
    /// <param name="direction">Направление смещения (должно быть единичным вектором).</param>
    /// <returns>Список всех произошедших перемещений для последующей анимации.</returns>
    public List<MoveResult> SettleBoard(Vector2Int direction)
    {
        var results = new List<MoveResult>();
        bool moved;

        // Этот цикл повторяется, пока на доске происходят какие-либо перемещения за один проход.
        // Это гарантирует, что кубы, освободившие место, позволят сдвинуться другим.
        do
        {
            moved = false;

            // Порядок обхода сетки критически важен. Мы должны начинать с того края,
            // куда направлено движение, чтобы кубы "тянули" за собой остальных.
            int startX = (direction.x > 0) ? Width - 1 : 0;
            int endX = (direction.x > 0) ? -1 : Width;
            int stepX = (direction.x != 0) ? -direction.x : 1;

            int startY = (direction.y > 0) ? Height - 1 : 0;
            int endY = (direction.y > 0) ? -1 : Height;
            int stepY = (direction.y != 0) ? -direction.y : 1;

            for (int x = startX; x != endX; x += stepX)
            {
                for (int y = startY; y != endY; y += stepY)
                {
                    var currentPos = new Vector2Int(x, y);
                    var cube = GetCubeAt(currentPos);
                    if (cube == null) continue;

                    var targetPos = currentPos + direction;

                    // Если целевая ячейка свободна и в пределах поля, перемещаем куб.
                    if (GetCubeAt(targetPos) == null && IsInBounds(targetPos))
                    {
                        // Обновляем модель данных
                        _grid[targetPos.x, targetPos.y] = cube;
                        _grid[currentPos.x, currentPos.y] = null;

                        results.Add(new MoveResult(cube, currentPos, targetPos));
                        moved = true;
                    }
                }
            }
        } while (moved);

        // Оптимизируем результаты: если куб двигался несколько раз,
        // нам нужен только итоговый результат его перемещения.
        var finalResults = new List<MoveResult>();
        var cubeFinalDestinations = new Dictionary<CubeController, Vector2Int>();
        var cubeOriginalPositions = new Dictionary<CubeController, Vector2Int>();

        foreach (var result in results)
        {
            // Запоминаем исходную позицию только один раз
            if (!cubeOriginalPositions.ContainsKey(result.Cube))
            {
                cubeOriginalPositions[result.Cube] = result.From;
            }
            // Обновляем конечную позицию
            cubeFinalDestinations[result.Cube] = result.To;
        }

        foreach (var cube in cubeFinalDestinations.Keys)
        {
            finalResults.Add(new MoveResult(cube, cubeOriginalPositions[cube], cubeFinalDestinations[cube]));
        }

        return finalResults;
    }

    /// <summary>
    /// Находит все связанные кубы одного типа (одинаковая верхняя грань),
    /// используя алгоритм поиска в ширину (заливка).
    /// </summary>
    /// <param name="startPosition">Начальная позиция для поиска.</param>
    /// <returns>Список всех кубов в связанной области.</returns>
    public List<CubeController> GetConnectedArea(Vector2Int startPosition)
    {
        var area = new List<CubeController>();
        var startCube = GetCubeAt(startPosition);
        if (startCube == null)
        {
            return area;
        }

        var targetFace = startCube.GetTopFaceCategory();
        var queue = new Queue<Vector2Int>();
        var visited = new HashSet<Vector2Int>();

        queue.Enqueue(startPosition);
        visited.Add(startPosition);

        while (queue.Count > 0)
        {
            var currentPos = queue.Dequeue();
            var currentCube = GetCubeAt(currentPos);

            area.Add(currentCube);

            // Проверяем 4-х соседей (вверх, вниз, влево, вправо)
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
    /// Проверяет, находятся ли координаты в пределах доски.
    /// </summary>
    private bool IsInBounds(Vector2Int position)
    {
        return position.x >= 0 && position.x < Width && position.y >= 0 && position.y < Height;
    }

}