using UnityEngine;

/// <summary>
/// Хранит сериализуемое состояние одного куба для сохранения и загрузки.
/// </summary>
[System.Serializable]
public class CubeState
{
    /// <summary>
    /// Сохраненное вращение куба.
    /// </summary>
    public Quaternion Rotation;
}