using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Обрабатывает все игровые действия, управляет их последовательностью и анимациями.
/// Является "движком" для выполнения команд, полученных от InputController через GameManager.
/// </summary>
public class ActionController : MonoBehaviour
{
    #region Private State
    private GameManager _gameManager;
    private GameBoard _gameBoard;
    private CubeSelector _cubeSelector;
    private CubeRotator _cubeRotator;
    private CubeMover _cubeMover;
    private Match3Controller _match3Controller;
    private Transform _boardContainer;

    private bool _isActionInProgress = false;
    #endregion

    #region Public Properties
    /// <summary>
    /// Указывает, находится ли игра в процессе выполнения действия (ввод заблокирован).
    /// </summary>
    public bool IsActionInProgress => _isActionInProgress;
    #endregion

    #region Public Methods
    /// <summary>
    /// Инициализирует контроллер ссылками на все необходимые системы.
    /// </summary>
    public void Initialize(GameManager gameManager, GameBoard gameBoard, CubeSelector cubeSelector, CubeRotator cubeRotator, CubeMover cubeMover, Match3Controller match3Controller, Transform boardContainer)
    {
        _gameManager = gameManager;
        _gameBoard = gameBoard;
        _cubeSelector = cubeSelector;
        _cubeRotator = cubeRotator;
        _cubeMover = cubeMover;
        _match3Controller = match3Controller;
        _boardContainer = boardContainer;
    }
    
    /// <summary>
    /// Выполняет простое пошаговое перемещение кубов.
    /// </summary>
    public void ProcessMove(Vector2Int inputDir)
    {
        var direction = GetInputDirection(inputDir);
        StartCoroutine(ProcessPlayerActionCoroutine(() => {
            var targetCubes = Input.GetKey(KeyCode.LeftShift) ? _cubeSelector.GetUnselectedCubes() : new List<CubeController>(_cubeSelector.SelectedCubes);
            var moveResults = _gameBoard.MoveCubes(targetCubes, direction);
            if (moveResults.Count > 0)
            {
                foreach (var result in moveResults)
                {
                    _cubeMover.MoveCubeTo(result.Cube, _gameBoard.GetWorldPosition(result.To));
                }
                return true;
            }
            return false;
        }));
    }

    /// <summary>
    /// Выполняет L-образный прыжок для одного выделенного куба.
    /// </summary>
    public void ProcessJumpStrafe()
    {
        StartCoroutine(ProcessJumpActionCoroutine());
    }
    
    /// <summary>
    /// Выполняет относительное вращение кубов на 90 градусов.
    /// </summary>
    public void ProcessRelativeRotation(Vector3 axis, float angle)
    {
        StartCoroutine(ProcessPlayerActionCoroutine(() => {
            var targetCubes = Input.GetKey(KeyCode.LeftShift) ? _cubeSelector.GetUnselectedCubes() : new List<CubeController>(_cubeSelector.SelectedCubes);
            if (targetCubes.Count > 0)
            {
                _cubeRotator.RotateCubesBy(targetCubes, axis, angle);
                return true;
            }
            return false;
        }));
    }
    
    /// <summary>
    /// Выполняет абсолютное вращение кубов к указанной грани.
    /// </summary>
    public void ProcessAbsoluteRotation(CubeFace face)
    {
        StartCoroutine(ProcessPlayerActionCoroutine(() => {
            var targetCubes = Input.GetKey(KeyCode.LeftShift) ? _cubeSelector.GetUnselectedCubes() : new List<CubeController>(_cubeSelector.SelectedCubes);
            if (targetCubes.Count > 0)
            {
                _cubeRotator.RotateCubesToFace(targetCubes, face);
                return true;
            }
            return false;
        }));
    }
    
    /// <summary>
    /// Выполняет обмен состояниями (вращением) двух выделенных кубов.
    /// </summary>
    public void ProcessStateSwap()
    {
        StartCoroutine(ProcessPlayerActionCoroutine(() => {
            var selectedCubes = _cubeSelector.SelectedCubes;
            if (selectedCubes.Count == 2)
            {
                var cubeA = selectedCubes[0]; var cubeB = selectedCubes[1];
                var rotationA = _cubeRotator.GetTargetRotation(cubeA); var rotationB = _cubeRotator.GetTargetRotation(cubeB);
                _cubeRotator.RotateCubeTo(cubeA, rotationB); _cubeRotator.RotateCubeTo(cubeB, rotationA);
                _cubeSelector.DeselectAll();
                return true;
            }
            return false;
        }));
    }
    
    /// <summary>
    /// Выполняет случайное вращение для целевых кубов.
    /// </summary>
    public void ProcessRandomization()
    {
        StartCoroutine(ProcessPlayerActionCoroutine(() => {
            var targetCubes = Input.GetKey(KeyCode.LeftShift) ? _cubeSelector.GetUnselectedCubes() : new List<CubeController>(_cubeSelector.SelectedCubes);
            if (targetCubes.Count > 0)
            {
                _cubeRotator.RotateCubesRandomly(targetCubes);
                return true;
            }
            return false;
        }));
    }

    /// <summary>
    /// Выполняет сдвиг всех кубов на поле в указанном направлении.
    /// </summary>
    public void ProcessSettle(Vector2Int inputDir)
    {
        var direction = GetInputDirection(inputDir);
        StartCoroutine(ProcessPlayerActionCoroutine(() => {
            var moveResults = _gameBoard.SettleBoard(direction);
            if (moveResults.Count > 0)
            {
                foreach (var result in moveResults)
                {
                    _cubeMover.MoveCubeTo(result.Cube, _gameBoard.GetWorldPosition(result.To));
                }
                return true;
            }
            return false;
        }));
    }
    
    /// <summary>
    /// Запускает последовательность вращения всего игрового поля.
    /// </summary>
    public void ProcessBoardRotation(float angle)
    {
        _gameManager.UpdateBoardRotationState(angle);
        StartCoroutine(ProcessBoardRotationSequenceCoroutine());
    }
    #endregion
    
    #region Coroutines & Helpers
    
    private IEnumerator ProcessPlayerActionCoroutine(System.Func<bool> action)
    {
        _isActionInProgress = true;
        bool actionWasSuccessful = action.Invoke();
        if (actionWasSuccessful)
        {
            while (_cubeMover.IsBusy() || _cubeRotator.IsBusy())
            {
                yield return null;
            }
            if (_gameManager.CurrentMode == GameMode.Match3)
            {
                _match3Controller.OnPlayerAction();
                yield return _match3Controller.ActiveCoroutine;
            }
        }
        _isActionInProgress = false;
    }
    
    private IEnumerator ProcessBoardRotationSequenceCoroutine()
    {
        _isActionInProgress = true;
        while (Quaternion.Angle(_boardContainer.rotation, _gameManager.GetTargetBoardRotation()) > 0.01f)
        {
            yield return null;
        }
        _boardContainer.rotation = _gameManager.GetTargetBoardRotation();
        
        if (_gameManager.CurrentMode == GameMode.Match3)
        {
            _match3Controller.UpdateGravityFromState(_gameManager.BoardRotationState);
            _match3Controller.OnPlayerAction(); 
            yield return _match3Controller.ActiveCoroutine;
        }
        _isActionInProgress = false;
    }

    private IEnumerator ProcessJumpActionCoroutine()
    {
        _isActionInProgress = true;
        var cube = _cubeSelector.SelectedCubes[0];
        var gravity = _match3Controller.GetCurrentGravityDirection();
        var jumpUpDir = GetInputDirection(Vector2Int.up);
        var fromPos = _gameBoard.WorldToGridPosition(cube.transform.position);
        var upPos = fromPos + jumpUpDir;

        if (!_gameBoard.IsInBounds(upPos) || _gameBoard.GetCubeAt(upPos) != null)
        {
            _isActionInProgress = false;
            yield break;
        }

        _gameBoard.RemoveCubeAt(fromPos);
        _gameBoard.PlaceCube(cube, upPos);
        _cubeMover.MoveCubeTo(cube, _gameBoard.GetWorldPosition(upPos));

        while (_cubeMover.IsBusy())
        {
            Vector2Int inputStrafeDirection = Vector2Int.zero;
            if (Input.GetKeyDown(KeyCode.A)) inputStrafeDirection = Vector2Int.left;
            if (Input.GetKeyDown(KeyCode.D)) inputStrafeDirection = Vector2Int.right;
            if (inputStrafeDirection != Vector2Int.zero)
            {
                var strafeDir = GetInputDirection(inputStrafeDirection);
                var finalPos = upPos + strafeDir;
                if (_gameBoard.IsInBounds(finalPos) && _gameBoard.GetCubeAt(finalPos) == null)
                {
                    _gameBoard.RemoveCubeAt(upPos);
                    _gameBoard.PlaceCube(cube, finalPos);
                    _cubeMover.MoveCubeTo(cube, _gameBoard.GetWorldPosition(finalPos));
                    break;
                }
            }
            yield return null;
        }
        
        while (_cubeMover.IsBusy())
        {
            yield return null;
        }

        if (_gameManager.CurrentMode == GameMode.Match3)
        {
            _match3Controller.OnPlayerAction();
            yield return _match3Controller.ActiveCoroutine;
        }
        _isActionInProgress = false;
    }
    
    private Vector2Int GetInputDirection(Vector2Int inputDir)
    {
        int rotationSteps = (_gameManager.BoardRotationState + 4) % 4;
        for (int i = 0; i < rotationSteps; i++)
        {
            inputDir = new Vector2Int(-inputDir.y, inputDir.x);
        }
        return inputDir;
    }
    
    #endregion
}