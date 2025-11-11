using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Отображает счетчик поколений для режима "Игра Жизнь".
/// </summary>
public class GameOfLifeUI : MonoBehaviour
{
    #region Serialized Fields
    [Tooltip("Текстовое поле для отображения номера поколения.")]
    [SerializeField] private Text generationText;
    [Tooltip("Ссылка на контроллер 'Игры Жизнь'.")]
    [SerializeField] private GameOfLifeController gameOfLifeController;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        if (gameOfLifeController == null)
        {
            gameOfLifeController = FindObjectOfType<GameOfLifeController>();
        }

        if (gameOfLifeController != null)
        {
            gameOfLifeController.OnGenerationUpdated += UpdateText;
            UpdateText(gameOfLifeController.GenerationCount);
            
            gameObject.SetActive(GameManager.Instance.CurrentMode == GameMode.Sandbox);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (gameOfLifeController != null)
        {
            gameOfLifeController.OnGenerationUpdated -= UpdateText;
        }
    }
    #endregion

    #region Private Methods
    private void UpdateText(int generation)
    {
        if (generationText != null)
        {
            generationText.text = $"Generation: {generation}";
        }
    }
    #endregion
}