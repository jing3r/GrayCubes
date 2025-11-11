using UnityEngine;

/// <summary>
/// Структура для описания результата операции перемещения одного куба.
/// Хранит информацию о том, какой куб и откуда куда переместился.
/// </summary>
public readonly struct MoveResult
{
    public readonly CubeController Cube;
    public readonly Vector2Int From;
    public readonly Vector2Int To;

    public MoveResult(CubeController cube, Vector2Int from, Vector2Int to)
    {
        Cube = cube;
        From = from;
        To = to;
    }
}