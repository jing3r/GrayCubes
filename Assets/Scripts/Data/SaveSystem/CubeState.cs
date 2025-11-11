using UnityEngine;

/// <summary>
/// Хранит сериализуемое состояние одного куба для сохранения и загрузки.
/// Содержит только чистые данные, независимые от GameObject.
/// </summary>
[System.Serializable]
public class CubeState
{
    /// <summary>
    /// Логическая позиция куба на сетке GameBoard.
    /// </summary>
    public Vector2Int GridPosition;

    /// <summary>
    /// Сохраненное вращение куба.
    /// </summary>
    public Quaternion Rotation;
}