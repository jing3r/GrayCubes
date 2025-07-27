using UnityEngine;
using UnityEngine.UI;

public class GameOfLifeUI : MonoBehaviour
{
    [SerializeField] private Text generationText; // или TextMeshProUGUI
    [SerializeField] private GameOfLifeController gameOfLifeController;

    private void Start()
    {
        if (gameOfLifeController == null)
        {
            // Попробуем найти, если не задан
            gameOfLifeController = FindObjectOfType<GameOfLifeController>();
        }

        if (gameOfLifeController != null)
        {
            gameOfLifeController.OnGenerationUpdated += UpdateText;
            UpdateText(gameOfLifeController.GenerationCount); // Установить начальное значение
        }
        else
        {
            gameObject.SetActive(false); // Скрыть UI, если нет игры
        }
    }

    private void OnDestroy()
    {
        if (gameOfLifeController != null)
        {
            gameOfLifeController.OnGenerationUpdated -= UpdateText; // Обязательно отписываемся!
        }
    }

    private void UpdateText(int generation)
    {
        generationText.text = $"Generation: {generation}";
    }
}