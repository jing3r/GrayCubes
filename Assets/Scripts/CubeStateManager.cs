using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Управляет сохранением и загрузкой состояния всех кубов.
/// </summary>
public class CubeStateManager : MonoBehaviour
{
    private List<CubeController> allCubes;
    private Dictionary<Vector3, CubeState> savedState;
    private CubeRotator cubeRotator; 

    /// <summary>
    /// Инициализирует менеджер состояния.
    /// </summary>
    public void Initialize(List<CubeController> cubes, CubeRotator rotator)
    {
        allCubes = cubes;
        cubeRotator = rotator;
    }

    /// <summary>
    /// Сохраняет текущее вращение всех кубов.
    /// </summary>
    public void SaveState()
    {
        if (cubeRotator == null)
        {
            Debug.LogError("CubeRotator не инициализирован в CubeStateManager! Сохранение будет неточным.");
        }

        savedState = new Dictionary<Vector3, CubeState>();
        foreach (var cube in allCubes)
        {

            Quaternion targetRotation = cubeRotator.GetTargetRotation(cube);

            var state = new CubeState { Rotation = targetRotation };
            savedState[cube.transform.position] = state;
        }
        Debug.Log($"Состояние {savedState.Count} кубов сохранено.");
    }


    /// <summary>
    /// Загружает ранее сохраненное состояние кубов.
    /// </summary>
    public void LoadState()
    {
        if (savedState == null || savedState.Count == 0)
        {
            Debug.LogWarning("Нет сохраненного состояния для загрузки.");
            return;
        }
        if (cubeRotator != null)
        {
            cubeRotator.StopAllRotations();
        }
        foreach (var cube in allCubes)
        {
            if (savedState.TryGetValue(cube.transform.position, out CubeState state))
            {
                // Загружаем только вращение. Позиция остается той же.
                cube.transform.rotation = state.Rotation;
            }
        }
        Debug.Log("Состояние кубов загружено.");
    }
}