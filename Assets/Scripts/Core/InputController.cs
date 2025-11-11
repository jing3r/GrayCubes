using UnityEngine;

/// <summary>
/// Отвечает за обработку всего пользовательского ввода (клавиатура, мышь)
/// и вызов соответствующих команд в GameManager.
/// </summary>
public class InputController : MonoBehaviour
{
    // Update вызывается каждый кадр
    void Update()
    {
        // GameManager может быть еще не инициализирован, поэтому проверяем Instance
        var gm = GameManager.Instance;
        
        // Не обрабатываем ввод, если GameManager не готов или действие уже в процессе
        if (gm == null || gm.IsActionInProgress)
        {
            return;
        }

        HandleMouseInput(gm);
        HandleKeyboardInput(gm);
    }

    private void HandleMouseInput(GameManager gm)
    {
        if (gm.GameBoard != null && Input.GetMouseButtonDown(0))
        {
            gm.HandleSelectionClick(Input.mousePosition);
        }
    }

    private void HandleKeyboardInput(GameManager gm)
    {
        // Системные
        if (Input.GetKeyDown(KeyCode.Escape)) { /* Здесь может быть gm.ToggleMenu() в будущем */ }
        if (Input.GetKeyDown(KeyCode.F5)) gm.SaveState();
        if (Input.GetKeyDown(KeyCode.F9)) gm.LoadState();

        // Выделение
        if (Input.GetKeyDown(KeyCode.C)) gm.DeselectAllCubes();
        
        // Манипуляции
        HandleMovementInput(gm);
        HandleRotateInput(gm);
        HandleAbsoluteFaceInput(gm);
        HandleStateSwapInput(gm);
        HandleRandomizeInput(gm);
        HandleSettleInput(gm);
        HandleBoardRotationInput(gm);
    }
    
    // Все методы Handle... теперь живут здесь. Они получают данные от Input
    // и вызывают высокоуровневые методы GameManager.

    private void HandleMovementInput(GameManager gm)
    {
        if (!Input.GetKey(KeyCode.V)) return;

        // "Прыжок" имеет приоритет
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (gm.CanProcessJumpStrafe())
            {
                gm.ProcessJumpStrafe();
                // Возвращаемся, чтобы не обработать S/A/D в том же кадре
                return; 
            }
        }
        
        // Обычное движение
        Vector2Int inputDir = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.S)) inputDir = Vector2Int.down;
        if (Input.GetKeyDown(KeyCode.A)) inputDir = Vector2Int.left;
        if (Input.GetKeyDown(KeyCode.D)) inputDir = Vector2Int.right;
        
        if(inputDir != Vector2Int.zero)
        {
            gm.ProcessMove(inputDir);
        }
    }

    private void HandleRotateInput(GameManager gm)
    {
        if (!Input.GetKey(KeyCode.R)) return;
        Vector3 axis = Vector3.zero;
        if (Input.GetKeyDown(KeyCode.W)) axis = Vector3.right;
        if (Input.GetKeyDown(KeyCode.S)) axis = Vector3.left;
        if (Input.GetKeyDown(KeyCode.A)) axis = Vector3.forward;
        if (Input.GetKeyDown(KeyCode.D)) axis = Vector3.back;

        if (axis != Vector3.zero)
        {
            gm.ProcessRelativeRotation(axis, 90f);
        }
    }

    private void HandleAbsoluteFaceInput(GameManager gm)
    {
        if (!Input.GetKey(KeyCode.F)) return;
        for (int i = 1; i <= 6; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
            {
                gm.ProcessAbsoluteRotation((CubeFace)i);
            }
        }
    }

    private void HandleStateSwapInput(GameManager gm)
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            gm.ProcessStateSwap();
        }
    }

    private void HandleRandomizeInput(GameManager gm)
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            gm.ProcessRandomization();
        }
    }

    private void HandleSettleInput(GameManager gm)
    {
        if (!Input.GetKey(KeyCode.T)) return;
        Vector2Int inputDir = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.W)) inputDir = Vector2Int.up;
        if (Input.GetKeyDown(KeyCode.S)) inputDir = Vector2Int.down;
        if (Input.GetKeyDown(KeyCode.A)) inputDir = Vector2Int.left;
        if (Input.GetKeyDown(KeyCode.D)) inputDir = Vector2Int.right;
        
        if (inputDir != Vector2Int.zero)
        {
            gm.ProcessSettle(inputDir);
        }
    }

    private void HandleBoardRotationInput(GameManager gm)
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            gm.ProcessBoardRotation(90f);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            gm.ProcessBoardRotation(-90f);
        }
    }
}