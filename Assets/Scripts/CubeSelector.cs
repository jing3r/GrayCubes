using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Управляет логикой выделения и снятия выделения кубов.
/// Поддерживает одиночные и двойные клики, а также клавиши-модификаторы.
/// </summary>
public class CubeSelector : MonoBehaviour
{
    private List<CubeController> _allCubes;
    private readonly List<CubeController> _selectedCubes = new List<CubeController>();

    /// <summary>
    /// Публичное свойство для доступа к списку выделенных кубов (только для чтения).
    /// </summary>
    public IReadOnlyList<CubeController> SelectedCubes => _selectedCubes;
    
    private Camera _mainCamera;

    // Поля для обработки двойного клика
    private float _lastClickTime = -1f;
    private const float DoubleClickTimeThreshold = 0.25f;
    private CubeController _lastClickedCube;
    
    /// <summary>
    /// Инициализирует селектор списком всех кубов на поле.
    /// </summary>
    public void Initialize(List<CubeController> cubes)
    {
        _allCubes = cubes;
        _mainCamera = Camera.main;
    }
    
    /// <summary>
    /// Обрабатывает клик мыши, используя модификаторы для управления выделением.
    /// </summary>
    public void HandleSelectionClick(Vector3 screenPosition, GameBoard gameBoard)
    {
        Ray ray = _mainCamera.ScreenPointToRay(screenPosition);
        if (!Physics.Raycast(ray, out RaycastHit hit) || !hit.collider.TryGetComponent<CubeController>(out var clickedCube))
        {
            return;
        }
            
        bool isShiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool isCtrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        // --- Логика двойного клика ---
        if (Time.time - _lastClickTime < DoubleClickTimeThreshold && _lastClickedCube == clickedCube)
        {
            _lastClickTime = -1f; // Сбрасываем таймер, чтобы избежать тройного клика
            _lastClickedCube = null;

            var area = gameBoard.GetConnectedArea(gameBoard.WorldToGridPosition(clickedCube.transform.position));
            
            if (isShiftPressed)
                Select(area); // Shift всегда добавляет
            else if (isCtrlPressed)
                ToggleSelection(area); // Ctrl всегда переключает
            else
                SetSelection(area); // Обычный двойной клик устанавливает новое выделение
        }
        else // --- Логика одиночного клика ---
        {
            _lastClickTime = Time.time;
            _lastClickedCube = clickedCube;

            if (isShiftPressed)
                Select(clickedCube);
            else if (isCtrlPressed)
                ToggleSelection(clickedCube);
            else
                SetSelection(new List<CubeController> { clickedCube });
        }
    }
    
    /// <summary>
    /// Снимает выделение со всех кубов.
    /// </summary>
    public void DeselectAll()
    {
        // Используем ToList() для создания копии, чтобы безопасно изменять коллекцию _selectedCubes
        foreach (var cube in _selectedCubes.ToList()) 
        {
            Deselect(cube);
        }
    }

    /// <summary>
    /// Возвращает список невыделенных кубов.
    /// </summary>
    public List<CubeController> GetUnselectedCubes()
    {
        return _allCubes.Where(c => !c.IsSelected).ToList();
    }

    /// <summary>
    /// Устанавливает выделение для всех кубов с указанной верхней гранью, сбрасывая предыдущее выделение.
    /// </summary>
    /// <param name="category">Целевая грань.</param>
    public void SelectByCategory(CubeFace category)
    {
        var cubesToSelect = _allCubes.Where(c => c.GetTopFaceCategory() == category).ToList();
        SetSelection(cubesToSelect);
    }
    
    #region Private Selection Methods

    // --- Методы для работы с группами кубов ---
    
    /// <summary>
    /// Устанавливает новое выделение, сбрасывая предыдущее.
    /// </summary>
    private void SetSelection(IReadOnlyList<CubeController> cubesToSelect)
    {
        DeselectAll();
        Select(cubesToSelect);
    }

    /// <summary>
    /// Добавляет кубы к существующему выделению.
    /// </summary>
    private void Select(IReadOnlyList<CubeController> cubes)
    {
        foreach (var cube in cubes)
        {
            Select(cube);
        }
    }

    /// <summary>
    /// Снимает выделение с указанных кубов.
    /// </summary>
    private void Deselect(IReadOnlyList<CubeController> cubes)
    {
        foreach (var cube in cubes)
        {
            Deselect(cube);
        }
    }
    
    /// <summary>
    /// Инвертирует состояние выделения для указанных кубов.
    /// </summary>
    private void ToggleSelection(IReadOnlyList<CubeController> cubes)
    {
        foreach (var cube in cubes)
        {
            ToggleSelection(cube);
        }
    }
    
    // --- Базовые методы для работы с одним кубом ---

    private void ToggleSelection(CubeController cube)
    {
        if (cube.IsSelected) Deselect(cube);
        else Select(cube);
    }
    
    private void Select(CubeController cube)
    {
        if (cube.IsSelected) return;
        cube.SetSelected(true);
        _selectedCubes.Add(cube);
    }
    
    private void Deselect(CubeController cube)
    {
        if (!cube.IsSelected) return;
        cube.SetSelected(false);
        _selectedCubes.Remove(cube);
    }
    
    #endregion
}