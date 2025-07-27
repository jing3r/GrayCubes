using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Управляет логикой выделения и снятия выделения кубов.
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

    /// <summary>
    /// Инициализирует селектор списком всех кубов на поле.
    /// </summary>
    public void Initialize(List<CubeController> cubes)
    {
        _allCubes = cubes;
        _mainCamera = Camera.main;
    }
    
    /// <summary>
    /// Обрабатывает клик мыши для выделения/снятия выделения куба.
    /// </summary>
    public void HandleSelectionClick(Vector3 screenPosition)
    {
        Ray ray = _mainCamera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.TryGetComponent<CubeController>(out var cube))
        {
            ToggleSelection(cube);
        }
    }

    /// <summary>
    /// Выделяет/снимает выделение для всех кубов указанной категории.
    /// </summary>
    public void ToggleSelectionByCategory(CubeFace category)
    {
        foreach (var cube in _allCubes)
        {
            if (cube.GetTopFaceCategory() == category)
            {
                ToggleSelection(cube);
            }
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
    /// Возвращает список невыделенных кубов.
    /// </summary>
    public List<CubeController> GetUnselectedCubes()
    {
        return _allCubes.Where(c => !c.IsSelected).ToList();
    }
    
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
}