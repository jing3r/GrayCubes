using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Отвечает за создание префабов кубов.
/// </summary>
public class CubeSpawner : MonoBehaviour
{
    #region Serialized Fields
    [Tooltip("Префаб куба для создания.")]
    [SerializeField] private GameObject cubePrefab;
    [Tooltip("Размер сетки по умолчанию для режима Песочницы.")]
    [SerializeField] private int gridSize = 32;
    [Tooltip("Расстояние между центрами кубов.")]
    [SerializeField] private float cubeSpacing = 1.1f;
    #endregion

    #region Public Properties
    /// <summary>
    /// Расстояние между центрами кубов.
    /// </summary>
    public float CubeSpacing => cubeSpacing;
    
    /// <summary>
    /// Размер сетки по умолчанию для режима песочницы.
    /// </summary>
    public int GridSize => gridSize;
    #endregion

    #region Public Methods
    /// <summary>
    /// Создает сетку кубов, используя размер по умолчанию.
    /// </summary>
    public List<CubeController> SpawnCubes()
    {
        return SpawnCubes(gridSize, gridSize);
    }

    /// <summary>
    /// Создает сетку кубов заданного размера, центрируя ее относительно родителя.
    /// </summary>
    public List<CubeController> SpawnCubes(int width, int height)
    {
        float offsetX = -(width - 1) * cubeSpacing / 2.0f;
        float offsetZ = -(height - 1) * cubeSpacing / 2.0f;
        Vector3 calculatedAnchor = new Vector3(offsetX, 0, offsetZ);

        var spawnedCubes = new List<CubeController>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 localPosition = calculatedAnchor + new Vector3(x * cubeSpacing, 0, y * cubeSpacing);
                GameObject cubeInstance = Instantiate(cubePrefab, transform);
                cubeInstance.transform.localPosition = localPosition;
                
                if (cubeInstance.TryGetComponent<CubeController>(out var controller))
                {
                    spawnedCubes.Add(controller);
                }
                else
                {
                    Debug.LogError($"Префаб '{cubePrefab.name}' не содержит компонент CubeController!");
                    Destroy(cubeInstance);
                }
            }
        }
        return spawnedCubes;
    }

    /// <summary>
    /// Создает один куб в заданной мировой позиции.
    /// </summary>
    public CubeController SpawnSingleCube(Vector3 worldPosition)
    {
        GameObject cubeInstance = Instantiate(cubePrefab, worldPosition, Quaternion.identity, transform);
        return cubeInstance.GetComponent<CubeController>();
    }
    #endregion
}