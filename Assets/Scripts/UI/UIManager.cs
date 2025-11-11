using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Управляет кнопками игрового интерфейса и связывает их с командами в GameManager.
/// </summary>
public class UIManager : MonoBehaviour
{
    #region Serialized Fields
    [Header("Кнопки действий")]
    [SerializeField] private Button rotateSelectedButton;
    [SerializeField] private Button rotateUnselectedButton;
    [SerializeField] private Button deselectAllButton;

    [Header("Кнопки выбора по категориям")]
    [SerializeField] private List<Button> selectCategoryButtons;

    [Header("Кнопки вращения к грани")]
    [SerializeField] private List<Button> rotateToFaceButtons;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("UIManager не может найти GameManager.Instance!", this);
            gameObject.SetActive(false);
            return;
        }

        BindButtons();
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Привязывает события OnClick кнопок к публичным методам GameManager.
    /// </summary>
    private void BindButtons()
    {
        rotateSelectedButton.onClick.AddListener(GameManager.Instance.RandomizeSelectedCubes);
        rotateUnselectedButton.onClick.AddListener(GameManager.Instance.RandomizeUnselectedCubes);
        deselectAllButton.onClick.AddListener(GameManager.Instance.DeselectAllCubes);

        for (int i = 0; i < selectCategoryButtons.Count; i++)
        {
            CubeFace category = (CubeFace)(i + 1);
            selectCategoryButtons[i].onClick.AddListener(() => GameManager.Instance.SelectCubesByCategory(category));
        }

        for (int i = 0; i < rotateToFaceButtons.Count; i++)
        {
            CubeFace face = (CubeFace)(i + 1);
            rotateToFaceButtons[i].onClick.AddListener(() => GameManager.Instance.RotateSelectedCubesToFace(face));
        }
    }
    #endregion
}