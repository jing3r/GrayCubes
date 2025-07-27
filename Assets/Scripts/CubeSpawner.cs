using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Отвечает за создание сетки кубов при старте сцены.
/// </summary>
public class CubeSpawner : MonoBehaviour
{
    [Tooltip("Префаб куба для создания.")]
    [SerializeField] private GameObject cubePrefab;
    [Tooltip("Размер сетки (gridSize x gridSize).")]
    [SerializeField] private int gridSize = 32;
    [Tooltip("Расстояние между центрами кубов.")]
    [SerializeField] private float cubeSpacing = 1.1f;
    public float CubeSpacing => cubeSpacing;
    [Tooltip("Якорная точка для начала построения сетки.")]
    [SerializeField] private Vector3 anchorPosition;

    /// <summary>
    /// Создает сетку кубов и возвращает список их контроллеров.
    /// </summary>
    /// <returns>Список созданных CubeController.</returns>
    public List<CubeController> SpawnCubes()
    {
        var spawnedCubes = new List<CubeController>();
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector3 position = anchorPosition + new Vector3(x * cubeSpacing, 0, y * cubeSpacing);
                GameObject cubeInstance = Instantiate(cubePrefab, position, Quaternion.identity, transform);
                
                if (cubeInstance.TryGetComponent<CubeController>(out var controller))
                {
                    spawnedCubes.Add(controller);
                }
                else
                {
                    Debug.LogError($"Префаб '{cubePrefab.name}' не содержит компонент CubeController!");
                }
            }
        }
        return spawnedCubes;
    }
}