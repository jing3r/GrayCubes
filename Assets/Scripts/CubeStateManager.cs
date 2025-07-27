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

    /// <summary>
    /// Инициализирует менеджер состояния.
    /// </summary>
    public void Initialize(List<CubeController> cubes)
    {
        allCubes = cubes;
    }

    /// <summary>
    /// Сохраняет текущее вращение всех кубов.
    /// </summary>
    public void SaveState()
    {
        savedState = new Dictionary<Vector3, CubeState>();
        foreach (var cube in allCubes)
        {
            var state = new CubeState { Rotation = cube.transform.rotation };
            // Используем позицию как уникальный идентификатор
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