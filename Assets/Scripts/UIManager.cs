using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public Button rotateSelectedButton;
    public Button rotateUnselectedButton;
    public Button deselectAllButton;
    public List<Button> selectCategoryButtons; 
    public List<Button> rotateToFaceButtons;

    private CubeSelector cubeSelector;
    private CubeRotator cubeRotator;

    void Start()
    {
        cubeSelector = FindObjectOfType<CubeSelector>();
        cubeRotator = FindObjectOfType<CubeRotator>();

        for (int i = 0; i < selectCategoryButtons.Count; i++)
        {
            int category = i + 1;
            selectCategoryButtons[i].onClick.AddListener(() => cubeSelector.SelectCubesByCategory(category));
        }

        for (int i = 0; i < rotateToFaceButtons.Count; i++)
        {
            int face = i + 1;
            rotateToFaceButtons[i].onClick.AddListener(() => cubeRotator.StartRotationForSelected(cubeSelector.SelectedCubes, face));
        }

        rotateSelectedButton.onClick.AddListener(() => cubeRotator.RotateSelectedCubesRandomly(cubeSelector.SelectedCubes));
        rotateUnselectedButton.onClick.AddListener(() => cubeRotator.RotateUnselectedCubesRandomly());
        deselectAllButton.onClick.AddListener(() => cubeSelector.DeselectAllCubes());
    }
}
