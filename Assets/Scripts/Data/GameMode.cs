/// <summary>
/// Определяет текущий активный игровой режим.
/// Используется в GameManager для переключения логики.
/// </summary>
public enum GameMode
{
    /// <summary>
    /// Режим песочницы со свободным управлением.
    /// </summary>
    Sandbox,

    /// <summary>
    /// Режим "Три в ряд".
    /// </summary>
    Match3,
    
    /// <summary>
    /// Режим симуляции "Игра Жизнь".
    /// </summary>
    GameOfLife
}