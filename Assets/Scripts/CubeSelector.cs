using UnityEngine;
using System.Collections.Generic;

public class CubeSelector : MonoBehaviour
{
    public List<GameObject> SelectedCubes { get; private set; } = new List<GameObject>();
    private Dictionary<GameObject, int> cubeCategories = new Dictionary<GameObject, int>();

    public void Initialize()
    {
        GameObject[] allCubes = GameObject.FindGameObjectsWithTag("Cube");

        foreach (var cube in allCubes)
        {
            UpdateCubeCategory(cube);
        }
    }

    public void HandleSelectionInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Cube"))
            {
                ToggleSelection(hit.collider.gameObject);
            }
        }

        if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.Keypad0))
        {
            DeselectAllCubes();
        }

        for (int i = 1; i <= 6; i++)
        {
            if (Input.GetKeyDown(KeyCode.Keypad1 + (i - 1)))
            {
                SelectCubesByCategory(i);
            }
        }
    }

    private void ToggleSelection(GameObject cube)
    {
        if (SelectedCubes.Contains(cube))
        {
            DeselectCube(cube);
        }
        else
        {
            SelectCube(cube);
        }
    }

    private void SelectCube(GameObject cube)
    {
        SelectedCubes.Add(cube);
        SetBordersActive(cube, true);
    }

    private void DeselectCube(GameObject cube)
    {
        SelectedCubes.Remove(cube);
        SetBordersActive(cube, false);
    }

    public void DeselectAllCubes()
    {
        foreach (var cube in SelectedCubes)
        {
            SetBordersActive(cube, false);
        }
        SelectedCubes.Clear();
    }

    private void SetBordersActive(GameObject cube, bool active)
    {
        foreach (Transform child in cube.transform)
        {
            foreach (Transform grandchild in child)
            {
                if (grandchild.CompareTag("Border"))
                {
                    grandchild.gameObject.SetActive(active);
                }
            }
        }
    }

    public void UpdateCubeCategory(GameObject cube)
    {
        Vector3 cubeUp = cube.transform.up;
        int category = 1;

        if (cubeUp == Vector3.up)
        {
            category = 1;
        }
        else if (cubeUp == Vector3.forward)
        {
            category = 2;
        }
        else if (cubeUp == Vector3.left)
        {
            category = 3;
        }
        else if (cubeUp == Vector3.back)
        {
            category = 4;
        }
        else if (cubeUp == Vector3.right)
        {
            category = 5;
        }
        else if (cubeUp == Vector3.down)
        {
            category = 6;
        }

        if (cubeCategories.ContainsKey(cube))
        {
            cubeCategories[cube] = category;
        }
        else
        {
            cubeCategories.Add(cube, category);
        }
    }


    public void SelectCubesByCategory(int category)
    {
        List<GameObject> cubesToToggle = new List<GameObject>();

        foreach (var cube in cubeCategories)
        {
            if (cube.Value == category)
            {
                cubesToToggle.Add(cube.Key);
            }
        }

        foreach (var cube in cubesToToggle)
        {
            if (SelectedCubes.Contains(cube))
            {
                DeselectCube(cube);
            }
            else
            {
                SelectCube(cube);
            }
        }
    }
}
