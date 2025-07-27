using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Отвечает за логику и анимацию вращения кубов.
/// </summary>
public class CubeRotator : MonoBehaviour
{
    [Tooltip("Скорость вращения кубов.")]
    [SerializeField] private float rotationSpeed = 5.0f;

    // Допуск, при котором вращение считается завершенным.
    private const float RotationAngleTolerance = 0.1f;

    private readonly Dictionary<CubeController, Quaternion> _targetRotations = new Dictionary<CubeController, Quaternion>();

    private void Update()
    {
        UpdateRotations();
    }

    /// <summary>
    /// Плавно поворачивает кубы к их целевым ротациям.
    /// </summary>
    private void UpdateRotations()
    {
        if (_targetRotations.Count == 0) return;

        List<CubeController> cubesToRemove = new List<CubeController>();

        foreach (var entry in _targetRotations)
        {
            var cube = entry.Key;
            var targetRotation = entry.Value;
            
            cube.transform.rotation = Quaternion.Lerp(cube.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            if (Quaternion.Angle(cube.transform.rotation, targetRotation) < RotationAngleTolerance)
            {
                cube.transform.rotation = targetRotation;
                cubesToRemove.Add(cube);
            }
        }

        foreach (var cube in cubesToRemove)
        {
            _targetRotations.Remove(cube);
        }
    }

    /// <summary>
    /// Запускает вращение для списка кубов к определенной грани.
    /// </summary>
    /// <param name="cubes">Список кубов для вращения.</param>
    /// <param name="targetFace">Целевая грань.</param>
    public void RotateCubesToFace(IReadOnlyList<CubeController> cubes, CubeFace targetFace)
    {
        Quaternion targetRotation = GetRotationForFace(targetFace);
        foreach (var cube in cubes)
        {
            StartRotation(cube, targetRotation);
        }
    }

    /// <summary>
    /// Запускает вращение для списка кубов в случайные стороны.
    /// </summary>
    /// <param name="cubes">Список кубов для вращения.</param>
    public void RotateCubesRandomly(IReadOnlyList<CubeController> cubes)
    {
        foreach (var cube in cubes)
        {
            CubeFace randomFace = (CubeFace)Random.Range(1, 7);
            Quaternion targetRotation = GetRotationForFace(randomFace);
            StartRotation(cube, targetRotation);
        }
    }

    /// <summary>
    /// Добавляет или обновляет целевую ротацию для одного куба.
    /// </summary>
    private void StartRotation(CubeController cube, Quaternion targetRotation)
    {
        _targetRotations[cube] = targetRotation;
    }

    /// <summary>
    /// Возвращает кватернион вращения для указанной грани.
    /// </summary>
    private Quaternion GetRotationForFace(CubeFace face)
    {
        switch (face)
        {
            case CubeFace.Up:      return Quaternion.Euler(0f, 0f, 0f);
            case CubeFace.Forward: return Quaternion.Euler(-90f, 0f, 0f);
            case CubeFace.Left:    return Quaternion.Euler(0f, 0f, 90f);
            case CubeFace.Back:    return Quaternion.Euler(90f, 0f, 0f);
            case CubeFace.Right:   return Quaternion.Euler(0f, 0f, -90f);
            case CubeFace.Down:    return Quaternion.Euler(180f, 0f, 0f);
            default:               return Quaternion.identity;
        }
    }
}