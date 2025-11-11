using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Управляет отображением статистики для режима Match-3.
/// </summary>
public class Match3UI : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] private Match3Controller match3Controller;
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private Text turnsText;
    [SerializeField] private Text createdText;
    [SerializeField] private Text destroyedText;
    #endregion

    #region Unity Lifecycle
    void Start()
    {
        if (match3Controller == null)
        {
            Debug.LogError("Match3Controller не назначен в Match3UI!", this);
            return;
        }
        match3Controller.OnStatsUpdated += UpdateUI;
        statsPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (match3Controller != null)
        {
            match3Controller.OnStatsUpdated -= UpdateUI;
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Показывает панель статистики.
    /// </summary>
    public void Show()
    {
        statsPanel.SetActive(true);
        UpdateUI();
    }

    /// <summary>
    /// Скрывает панель статистики.
    /// </summary>
    public void Hide()
    {
        statsPanel.SetActive(false);
    }
    #endregion

    #region Private Methods
    private void UpdateUI()
    {
        if (!statsPanel.activeSelf || match3Controller == null) return;

        turnsText.text = $"Ходов: {match3Controller.TurnCount}";
        createdText.text = $"Создано: {match3Controller.CubesCreated}";
        destroyedText.text = $"Уничтожено: {match3Controller.CubesDestroyed}";
    }
    #endregion
}