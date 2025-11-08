using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Отвечает за логику и анимацию перемещения кубов.
/// </summary>
public class CubeMover : MonoBehaviour
{
    [Tooltip("Скорость перемещения кубов.")]
    [SerializeField] private float moveSpeed = 8.0f;

    // Допуск, при котором перемещение считается завершенным.
    private const float PositionDistanceTolerance = 0.01f;

    private readonly Dictionary<CubeController, Vector3> _targetPositions = new Dictionary<CubeController, Vector3>();

    private void Update()
    {
        UpdatePositions();
    }

    /// <summary>
    /// Плавно перемещает кубы к их целевым позициям.
    /// </summary>
    private void UpdatePositions()
    {
        if (_targetPositions.Count == 0) return;

        var cubesToRemove = new List<CubeController>();

        foreach (var entry in _targetPositions)
        {
            var cube = entry.Key;
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
    /// Запускает перемещение для одного куба к целевой позиции.
    /// </summary>
    /// <param name="cube">Куб для перемещения.</param>
    /// <param name="targetPosition">Целевая позиция в мировых координатах.</param>
    public void MoveCubeTo(CubeController cube, Vector3 targetPosition)
    {
        _targetPositions[cube] = targetPosition;
    }
}