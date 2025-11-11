using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Управляет логикой выделения и снятия выделения кубов.
/// Поддерживает одиночные и двойные клики, а также клавиши-модификаторы.
/// </summary>
public class CubeSelector : MonoBehaviour
{
    #region Private State
    private List<CubeController> _allCubes;
    private readonly List<CubeController> _selectedCubes = new List<CubeController>();
    private Camera _mainCamera;
    private float _lastClickTime = -1f;
    private CubeController _lastClickedCube;
    #endregion

    #region Constants
    private const float DoubleClickTimeThreshold = 0.25f;
    #endregion

    #region Public Properties
    /// <summary>
    /// Публичное свойство для доступа к списку выделенных кубов (только для чтения).
    /// </summary>
    public IReadOnlyList<CubeController> SelectedCubes => _selectedCubes;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        _mainCamera = Camera.main;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Инициализирует селектор списком всех кубов на поле.
    /// </summary>
    public void Initialize(List<CubeController> cubes)
    {
        _allCubes = cubes;
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
        
        if (Time.time - _lastClickTime < DoubleClickTimeThreshold && _lastClickedCube == clickedCube)
        {
            _lastClickTime = -1f;
            _lastClickedCube = null;

            var area = gameBoard.GetConnectedArea(gameBoard.WorldToGridPosition(clickedCube.transform.position));
            if (isShiftPressed) Select(area);
            else if (isCtrlPressed) ToggleSelection(area);
            else SetSelection(area);
        }
        else
        {
            _lastClickTime = Time.time;
            _lastClickedCube = clickedCube;

            if (isShiftPressed) Select(clickedCube);
            else if (isCtrlPressed) ToggleSelection(clickedCube);
            else SetSelection(new List<CubeController> { clickedCube });
        }
    }
    
    /// <summary>
    /// Снимает выделение со всех кубов.
    /// </summary>
    public void DeselectAll()
    {
        foreach (var cube in _selectedCubes.ToList()) 
        {
            Deselect(cube);
        }
    }

    /// <summary>
    /// Снимает выделение с одного конкретного куба, если он был выбран.
    /// </summary>
    public void Deselect(CubeController cube)
    {
        if (cube == null || !cube.IsSelected) return;
        cube.SetSelected(false);
        _selectedCubes.Remove(cube);
    }

    /// <summary>
    /// Возвращает список невыделенных кубов.
    /// </summary>
    public List<CubeController> GetUnselectedCubes()
    {
        if (_allCubes == null) return new List<CubeController>();
        return _allCubes.Where(c => c != null && !c.IsSelected).ToList();
    }

    /// <summary>
    /// Устанавливает выделение для всех кубов с указанной верхней гранью, сбрасывая предыдущее выделение.
    /// </summary>
    public void SelectByCategory(CubeFace category)
    {
        if (_allCubes == null) return;
        var cubesToSelect = _allCubes.Where(c => c != null && c.GetTopFaceCategory() == category).ToList();
        SetSelection(cubesToSelect);
    }
    #endregion

    #region Private Selection Methods
    private void SetSelection(IReadOnlyList<CubeController> cubesToSelect)
    {
        DeselectAll();
        Select(cubesToSelect);
    }
    
    private void Select(IReadOnlyList<CubeController> cubes)
    {
        foreach (var cube in cubes)
        {
            Select(cube);
        }
    }
    
    private void ToggleSelection(IReadOnlyList<CubeController> cubes)
    {
        foreach (var cube in cubes)
        {
            ToggleSelection(cube);
        }
    }
    
    private void ToggleSelection(CubeController cube)
    {
        if (cube == null) return;
        if (cube.IsSelected) Deselect(cube);
        else Select(cube);
    }
    
    private void Select(CubeController cube)
    {
        if (cube == null || cube.IsSelected) return;
        cube.SetSelected(true);
        _selectedCubes.Add(cube);
    }
    #endregion
}