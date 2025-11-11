using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Отвечает за логику и анимацию перемещения кубов в мировом пространстве.
/// </summary>
public class CubeMover : MonoBehaviour
{
    [Tooltip("Скорость перемещения кубов.")]
    [SerializeField] private float moveSpeed = 8.0f;

    // Допуск, при котором перемещение считается завершенным.
    private const float PositionDistanceTolerance = 0.01f;

    // Теперь снова храним мировые цели.
    private readonly Dictionary<CubeController, Vector3> _targetPositions = new Dictionary<CubeController, Vector3>();
    
    /// <summary>
    /// Возвращает true, если в данный момент есть активные анимации перемещения.
    /// </summary>
    public bool IsBusy()
    {
        return _targetPositions.Count > 0;
    }

    private void Update()
    {
        UpdatePositions();
    }

    /// <summary>
    /// Плавно перемещает кубы к их целевым мировым позициям.
    /// </summary>
    private void UpdatePositions()
    {
        if (_targetPositions.Count == 0) return;

        var cubesToRemove = new List<CubeController>();

        foreach (var entry in _targetPositions)
        {
            var cube = entry.Key;
            // Проверяем, не был ли куб уничтожен в процессе
            if (cube == null)
            {
                cubesToRemove.Add(entry.Key); // Добавляем ключ, так как value уже null
                continue;
            }

            var targetPosition = entry.Value;
            
            cube.transform.position = Vector3.Lerp(cube.transform.position, targetPosition, Time.deltaTime * moveSpeed);

            if (Vector3.Distance(cube.transform.position, targetPosition) < PositionDistanceTolerance)
            {
                cube.transform.position = targetPosition;
                cubesToRemove.Add(cube);
            }
        }

        foreach (var cube in cubesToRemove)
        {
            _targetPositions.Remove(cube);
        }
    }

    /// <summary>
    /// Запускает анимацию перемещения куба к целевой МИРОВОЙ позиции.
    /// </summary>
    public void MoveCubeTo(CubeController cube, Vector3 targetPosition)
    {
        if (cube == null) return;
        _targetPositions[cube] = targetPosition;
    }
    /// <summary>
    /// Запускает корутину для L-образной анимации.
    /// </summary>
    /// <returns>Возвращает запущенную корутину, чтобы можно было дождаться ее завершения.</returns>
    public Coroutine AnimateLShapedMove(CubeController cube, Vector3 intermediateWorldPos, Vector3 finalWorldPos)
    {
        return StartCoroutine(AnimateLShapedMoveCoroutine(cube, intermediateWorldPos, finalWorldPos));
    }

    private IEnumerator AnimateLShapedMoveCoroutine(CubeController cube, Vector3 intermediateWorldPos, Vector3 finalWorldPos)
    {
        // Шаг 1: Движение "вверх"
        MoveCubeTo(cube, intermediateWorldPos);
        while (_targetPositions.ContainsKey(cube))
        {
            yield return null;
        }

        // Шаг 2: Движение "вбок"
        MoveCubeTo(cube, finalWorldPos);
        while (_targetPositions.ContainsKey(cube))
        {
            yield return null;
        }
    }
    /// <summary>
    /// Немедленно останавливает все текущие и запланированные анимации перемещения.
    /// </summary>
    public void StopAllMovements()
    {
        _targetPositions.Clear();
    }
}