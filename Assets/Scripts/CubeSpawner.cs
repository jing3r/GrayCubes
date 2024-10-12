using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    public GameObject cubePrefab;
    public int gridSize = 32;
    public float cubeSpacing = 1.1f;
    public Vector3 anchorPosition;

    void Start()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector3 position = anchorPosition + new Vector3(x * cubeSpacing, 0, y * cubeSpacing);
                Instantiate(cubePrefab, position, Quaternion.identity);
            }
        }
    }
}