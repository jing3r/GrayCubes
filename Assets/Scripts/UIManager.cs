using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Управляет кнопками игрового интерфейса и связывает их с игровой логикой.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Кнопки действий")]
    [SerializeField] private Button rotateSelectedButton;
    [SerializeField] private Button rotateUnselectedButton;
    [SerializeField] private Button deselectAllButton;

    [Header("Кнопки выбора по категориям")]
    [SerializeField] private List<Button> selectCategoryButtons;

    [Header("Кнопки вращения к грани")]
    [SerializeField] private List<Button> rotateToFaceButtons;
    
    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("UIManager не может найти GameManager.Instance!");
            return;
        }

        var selector = GameManager.Instance.CubeSelector;
        var rotator = GameManager.Instance.CubeRotator;

        rotateSelectedButton.onClick.AddListener(() => rotator.RotateCubesRandomly(selector.SelectedCubes));
        rotateUnselectedButton.onClick.AddListener(() => rotator.RotateCubesRandomly(selector.GetUnselectedCubes()));
        deselectAllButton.onClick.AddListener(selector.DeselectAll);

        for (int i = 0; i < selectCategoryButtons.Count; i++)
        {
            CubeFace category = (CubeFace)(i + 1);
            selectCategoryButtons[i].onClick.AddListener(() => selector.SelectByCategory(category));
        }

        for (int i = 0; i < rotateToFaceButtons.Count; i++)
        {
            CubeFace face = (CubeFace)(i + 1);
            rotateToFaceButtons[i].onClick.AddListener(() => rotator.RotateCubesToFace(selector.SelectedCubes, face));
        }
    }
}