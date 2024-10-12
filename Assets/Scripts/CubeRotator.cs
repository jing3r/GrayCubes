using UnityEngine;
using System.Collections.Generic;

public class CubeRotator : MonoBehaviour
{
    private float rotationSpeed = 2.0f;
    private Dictionary<GameObject, Quaternion> targetRotations = new Dictionary<GameObject, Quaternion>();
    private List<GameObject> rotatingCubes = new List<GameObject>();

    private CubeSelector cubeSelector;

    void Start()
    {
        cubeSelector = GetComponent<CubeSelector>();
    }

    public void HandleRotationInput(List<GameObject> selectedCubes)
    {
        if (selectedCubes.Count > 0)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                StartRotationForSelected(selectedCubes, 1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                StartRotationForSelected(selectedCubes, 2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                StartRotationForSelected(selectedCubes, 3);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                StartRotationForSelected(selectedCubes, 4);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                StartRotationForSelected(selectedCubes, 5);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                StartRotationForSelected(selectedCubes, 6);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RotateSelectedCubesRandomly(selectedCubes);
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            RotateUnselectedCubesRandomly();
        }
    }

    public void UpdateRotations()
    {
        List<GameObject> cubesToRemove = new List<GameObject>();

        foreach (var cube in rotatingCubes)
        {
            if (cube != null && targetRotations.ContainsKey(cube))
            {
                cube.transform.rotation = Quaternion.Lerp(cube.transform.rotation, targetRotations[cube], Time.deltaTime * rotationSpeed);

                if (Quaternion.Angle(cube.transform.rotation, targetRotations[cube]) < 0.1f)
                {
                    cube.transform.rotation = targetRotations[cube];
                    cubesToRemove.Add(cube);
                    cubeSelector.UpdateCubeCategory(cube);
                }
            }
        }

        foreach (var cube in cubesToRemove)
        {
            rotatingCubes.Remove(cube);
            targetRotations.Remove(cube);
        }
    }

    public void StartRotation(GameObject cube, int targetFace)
    {
        if (!rotatingCubes.Contains(cube))
        {
            rotatingCubes.Add(cube);
        }

        Quaternion targetRotation = Quaternion.identity;

        switch (targetFace)
        {
            case 1:
                targetRotation = Quaternion.identity;
                break;
            case 2:
                targetRotation = Quaternion.Euler(90f, 0f, 0f);
                break;
            case 3:
                targetRotation = Quaternion.Euler(0f, 0f, -90f);
                break;
            case 4:
                targetRotation = Quaternion.Euler(0f, 0f, 90f);
                break;
            case 5:
                targetRotation = Quaternion.Euler(-90f, 0f, 0f);
                break;
            case 6:
                targetRotation = Quaternion.Euler(180f, 0f, 0f);
                break;
        }

        if (targetRotations.ContainsKey(cube))
        {
            targetRotations[cube] = targetRotation;
        }
        else
        {
            targetRotations.Add(cube, targetRotation);
        }
    }

    public void StartRotationForSelected(List<GameObject> selectedCubes, int targetFace)
    {
        foreach (var cube in selectedCubes)
        {
            StartRotation(cube, targetFace);
        }
    }

    public void RotateSelectedCubesRandomly(List<GameObject> selectedCubes)
    {
        foreach (var cube in selectedCubes)
        {
            int randomFace = Random.Range(1, 7);
            StartRotation(cube, randomFace);
        }
    }

    public void RotateUnselectedCubesRandomly()
    {
        GameObject[] allCubes = GameObject.FindGameObjectsWithTag("Cube");

        foreach (var cube in allCubes)
        {
            if (!rotatingCubes.Contains(cube))
            {
                int randomFace = Random.Range(1, 7);
                StartRotation(cube, randomFace);
            }
        }
    }
}
