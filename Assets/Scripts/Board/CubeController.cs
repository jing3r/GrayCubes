using UnityEngine;

/// <summary>
/// Управляет состоянием и поведением одного куба.
/// Хранит информацию о текущем состоянии и отвечает за визуальные эффекты.
/// </summary>
public class CubeController : MonoBehaviour
{
    [Tooltip("Объект-контейнер, содержащий все рамки выделения.")]
    [SerializeField] private GameObject selectionBordersContainer;

    /// <summary>
    /// Флаг, указывающий, выбран ли куб в данный момент.
    /// </summary>
    public bool IsSelected { get; private set; }

    // Допуск для сравнения векторов с плавающей точкой.
    private const float VectorDotTolerance = 0.99f;

    private void Awake()
    {
        SetSelectionVisual(false);
    }

    /// <summary>
    /// Устанавливает состояние выделения для куба.
    /// </summary>
    /// <param name="isSelected">True, чтобы выделить куб, false, чтобы снять выделение.</param>
    public void SetSelected(bool isSelected)
    {
        if (IsSelected == isSelected) return;

        IsSelected = isSelected;
        SetSelectionVisual(IsSelected);
    }

    /// <summary>
    /// Включает или выключает визуальное отображение выделения.
    /// </summary>
    private void SetSelectionVisual(bool isActive)
    {
        if (selectionBordersContainer != null)
        {
            selectionBordersContainer.SetActive(isActive);
        }
    }

    /// <summary>
    /// Определяет, какая грань куба сейчас является "верхней".
    /// </summary>
    /// <returns>Значение из enum CubeFace, представляющее грань.</returns>
    public CubeFace GetTopFaceCategory()
    {
        Vector3 localUp = transform.up;

        if (Vector3.Dot(localUp, Vector3.up) > VectorDotTolerance) return CubeFace.Up;
        if (Vector3.Dot(localUp, Vector3.forward) > VectorDotTolerance) return CubeFace.Forward;
        if (Vector3.Dot(localUp, Vector3.left) > VectorDotTolerance) return CubeFace.Left;
        if (Vector3.Dot(localUp, Vector3.back) > VectorDotTolerance) return CubeFace.Back;
        if (Vector3.Dot(localUp, Vector3.right) > VectorDotTolerance) return CubeFace.Right;
        if (Vector3.Dot(localUp, Vector3.down) > VectorDotTolerance) return CubeFace.Down;

        return CubeFace.Up; // Возвращаем значение по умолчанию в исключительном случае.
    }
}