using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = GetComponent<Camera>();
    }

    /// <summary>
    /// Подстраивает позицию и размер ортографической камеры, чтобы идеально
    /// вписать в экран игровое поле заданного размера.
    /// </summary>
    /// <param name="boardWidth">Ширина поля в кубах.</param>
    /// <param name="boardHeight">Высота поля в кубах.</param>
    /// <param name="spacing">Расстояние между кубами.</param>
    /// <param name="padding">Дополнительный отступ по краям (в кубах).</param>
    public void FitCameraToBoard(int boardWidth, int boardHeight, float spacing, float padding = 1.0f)
    {
        float totalWidth = boardWidth * spacing;
        float totalHeight = boardHeight * spacing;

        float requiredSizeX = (totalWidth / _mainCamera.aspect) / 2.0f + padding;
        float requiredSizeY = totalHeight / 2.0f + padding;

        _mainCamera.orthographicSize = Mathf.Max(requiredSizeX, requiredSizeY);
        
    }
}