using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Отвечает за логику и анимацию вращения кубов.
/// </summary>
public class CubeRotator : MonoBehaviour
{
    #region Serialized Fields
    [Tooltip("Скорость вращения кубов.")]
    [SerializeField] private float rotationSpeed = 5.0f;
    #endregion

    #region Constants
    private const float RotationAngleTolerance = 0.1f;
    #endregion

    #region Private State
    private readonly Dictionary<CubeController, Quaternion> _targetRotations = new Dictionary<CubeController, Quaternion>();
    #endregion

    #region Unity Lifecycle
    private void Update()
    {
        UpdateRotations();
    }
    #endregion
    
    #region Public Methods
    /// <summary>
    /// Возвращает true, если в данный момент есть активные анимации вращения.
    /// </summary>
    public bool IsBusy()
    {
        return _targetRotations.Count > 0;
    }
    
    /// <summary>
    /// Немедленно останавливает все текущие и запланированные анимации вращения.
    /// </summary>
    public void StopAllRotations()
    {
        _targetRotations.Clear();
    }

    /// <summary>
    /// Запускает анимацию вращения куба к определенному целевому вращению.
    /// </summary>
    public void RotateCubeTo(CubeController cube, Quaternion targetRotation)
    {
        if(cube == null) return;
        _targetRotations[cube] = targetRotation;
    }
    
    /// <summary>
    /// Запускает анимацию вращения для списка кубов к определенной грани.
    /// </summary>
    public void RotateCubesToFace(IReadOnlyList<CubeController> cubes, CubeFace targetFace)
    {
        Quaternion targetRotation = GetRotationForFace(targetFace);
        foreach (var cube in cubes)
        {
            RotateCubeTo(cube, targetRotation);
        }
    }

    /// <summary>
    /// Запускает анимацию вращения для списка кубов в случайные стороны.
    /// </summary>
    public void RotateCubesRandomly(IReadOnlyList<CubeController> cubes)
    {
        foreach (var cube in cubes)
        {
            CubeFace randomFace = (CubeFace)Random.Range(1, 7);
            Quaternion targetRotation = GetRotationForFace(randomFace);
            RotateCubeTo(cube, targetRotation);
        }
    }

    /// <summary>
    /// Запускает относительное вращение для списка кубов.
    /// </summary>
    public void RotateCubesBy(IReadOnlyList<CubeController> cubes, Vector3 rotationAxis, float angle)
    {
        Quaternion rotation = Quaternion.AngleAxis(angle, rotationAxis);
        foreach (var cube in cubes)
        {
            Quaternion currentTarget = GetTargetRotation(cube);
            Quaternion newTargetRotation = rotation * currentTarget;
            RotateCubeTo(cube, newTargetRotation);
        }
    }

    /// <summary>
    /// Возвращает целевое вращение куба, если он анимируется, или его текущее вращение.
    /// </summary>
    public Quaternion GetTargetRotation(CubeController cube)
    {
        if (cube != null && _targetRotations.TryGetValue(cube, out Quaternion targetRotation))
        {
            return targetRotation;
        }
        return cube != null ? cube.transform.rotation : Quaternion.identity;
    }

    /// <summary>
    /// Возвращает кватернион вращения, соответствующий указанной грани, обращенной вверх.
    /// </summary>
    public Quaternion GetRotationForFace(CubeFace face)
    {
        switch (face)
        {
            case CubeFace.Up:      return Quaternion.identity;
            case CubeFace.Forward: return Quaternion.Euler(-90f, 0f, 0f);
            case CubeFace.Left:    return Quaternion.Euler(0f, 0f, 90f);
            case CubeFace.Back:    return Quaternion.Euler(90f, 0f, 0f);
            case CubeFace.Right:   return Quaternion.Euler(0f, 0f, -90f);
            case CubeFace.Down:    return Quaternion.Euler(180f, 0f, 0f);
            default:               return Quaternion.identity;
        }
    }
    #endregion
    
    #region Private Methods
    private void UpdateRotations()
    {
        if (_targetRotations.Count == 0) return;

        var cubesToRemove = new List<CubeController>();
        foreach (var entry in _targetRotations)
        {
            var cube = entry.Key;
            if (cube == null)
            {
                cubesToRemove.Add(entry.Key);
                continue;
            }
            
            var targetRotation = entry.Value;
            cube.transform.rotation = Quaternion.Lerp(cube.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            if (Quaternion.Angle(cube.transform.rotation, targetRotation) < RotationAngleTolerance)
            {
                cube.transform.rotation = targetRotation;
                cubesToRemove.Add(cube);
            }
        }

        foreach (var cubeKey in cubesToRemove)
        {
            _targetRotations.Remove(cubeKey);
        }
    }
    #endregion
}