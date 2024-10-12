using UnityEngine;
using System.Collections.Generic;

public class CubeManager : MonoBehaviour
{
    private CubeSelector cubeSelector;
    private CubeRotator cubeRotator;

    void Start()
    {
        cubeSelector = GetComponent<CubeSelector>();
        cubeRotator = GetComponent<CubeRotator>();

        cubeSelector.Initialize();
    }

    void Update()
    {
        cubeSelector.HandleSelectionInput();
        cubeRotator.HandleRotationInput(cubeSelector.SelectedCubes);

        cubeRotator.UpdateRotations();
    }
}
