using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Хранит полное сериализуемое состояние игрового мира для сохранения и загрузки.
/// Включает в себя состояние поля, список всех кубов и статистику сессии.
/// </summary>
[System.Serializable]
public class WorldState
{
    /// <summary>
    /// Сохраненное вращение игрового поля (BoardContainer).
    /// </summary>
    public Quaternion BoardRotation;

    /// <summary>
    /// Список состояний всех кубов, которые были на поле в момент сохранения.
    /// </summary>
    public List<CubeState> Cubes;

    /// <summary>
    /// Сохраненное количество ходов.
    /// </summary>
    public int TurnCount;

    /// <summary>
    /// Сохраненное количество созданных кубов.
    /// </summary>
    public int CubesCreated;

    /// <summary>
    /// Сохраненное количество уничтоженных кубов.
    /// </summary>
    public int CubesDestroyed;

    public WorldState()
    {
        Cubes = new List<CubeState>();
    }
}