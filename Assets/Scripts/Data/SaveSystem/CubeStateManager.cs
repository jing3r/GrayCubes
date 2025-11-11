using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Управляет сохранением и загрузкой состояния игрового поля и сессии.
/// </summary>
public class CubeStateManager : MonoBehaviour
{
    #region Serialized Fields
    [Tooltip("Ссылка на контроллер режима Match-3 для сохранения/загрузки статистики.")]
    [SerializeField] private Match3Controller match3Controller;
    #endregion

    #region Private State
    private GameManager _gameManager;
    private GameBoard _gameBoard;
    private CubeRotator _cubeRotator;
    private WorldState _savedWorldState;
    #endregion

    #region Public Methods
    /// <summary>
    /// Инициализирует менеджер состояния необходимыми ссылками на системы.
    /// </summary>
    public void Initialize(GameManager gameManager, GameBoard gameBoard, CubeRotator rotator)
    {
        _gameManager = gameManager;
        _gameBoard = gameBoard;
        _cubeRotator = rotator;
    }

    /// <summary>
    /// Сохраняет текущее состояние игрового мира (поворот поля, кубы, статистика).
    /// </summary>
    public void SaveState()
    {
        if (_gameManager == null || _gameBoard == null)
        {
            Debug.LogError("CubeStateManager не инициализирован!");
            return;
        }

        _savedWorldState = new WorldState();
        _savedWorldState.BoardRotation = _gameManager.GetTargetBoardRotation();
        
        if (match3Controller != null && _gameManager.CurrentMode == GameMode.Match3)
        {
            _savedWorldState.TurnCount = match3Controller.TurnCount;
            _savedWorldState.CubesCreated = match3Controller.CubesCreated;
            _savedWorldState.CubesDestroyed = match3Controller.CubesDestroyed;
        }

        _savedWorldState.Cubes = new List<CubeState>();
        foreach (var cube in _gameManager.AllCubes)
        {
            if (cube == null) continue;
            var state = new CubeState
            {
                GridPosition = _gameBoard.WorldToGridPosition(cube.transform.position),
                Rotation = _cubeRotator.GetTargetRotation(cube)
            };
            _savedWorldState.Cubes.Add(state);
        }
        Debug.Log($"Состояние мира ({_savedWorldState.Cubes.Count} кубов) сохранено.");
    }

    /// <summary>
    /// Полностью воссоздает игровое поле из сохраненного состояния.
    /// </summary>
    public void LoadState()
    {
        if (_savedWorldState == null || _gameManager == null)
        {
            Debug.LogWarning("Нет сохраненного состояния для загрузки.");
            return;
        }

        _gameManager.IsLoading = true;
        _gameManager.SetBoardRotationInstantly(_savedWorldState.BoardRotation);
        _gameManager.ClearBoard();

        foreach (var savedCubeState in _savedWorldState.Cubes)
        {
            var newCube = _gameManager.CreateCubeAt(savedCubeState.GridPosition);
            newCube.transform.rotation = savedCubeState.Rotation;
        }

        _gameManager.ReinitializeSelector();

        if (match3Controller != null && _gameManager.CurrentMode == GameMode.Match3)
        {
            match3Controller.LoadStats(_savedWorldState);
            _gameManager.SyncMatch3State();
        }
        
        StartCoroutine(WaitForLoadingToComplete());
    }
    #endregion
    
    #region Private Methods
    private IEnumerator WaitForLoadingToComplete()
    {
        yield return null; 
        
        while (_cubeRotator != null && _cubeRotator.IsBusy())
        {
            yield return null;
        }

        _gameManager.IsLoading = false;
        Debug.Log("Загрузка полностью завершена.");
    }
    #endregion
}