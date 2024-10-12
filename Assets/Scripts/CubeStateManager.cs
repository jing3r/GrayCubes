using UnityEngine;
using System.Collections.Generic;

public class CubeStateManager : MonoBehaviour
{
    private List<CubeState> savedCubeStates;

    public void SaveCubeState()
    {
        savedCubeStates = new List<CubeState>();
        GameObject[] allCubes = GameObject.FindGameObjectsWithTag("Cube");

        foreach (var cube in allCubes)
        {
            CubeState state = new CubeState
            {
                Position = cube.transform.position,
                Rotation = cube.transform.rotation
            };
            savedCubeStates.Add(state);
        }

        Debug.Log("Cube state saved.");
    }

    public void LoadCubeState()
    {
        if (savedCubeStates == null || savedCubeStates.Count == 0)
        {
            Debug.LogWarning("There is no saved cube state.");
            return;
        }

        GameObject[] allCubes = GameObject.FindGameObjectsWithTag("Cube");

        if (allCubes.Length != savedCubeStates.Count)
        {
            Debug.LogError("The number of cubes has been changed, it is not possible to load the state of the cubes.");
            return;
        }

        for (int i = 0; i < allCubes.Length; i++)
        {
            allCubes[i].transform.position = savedCubeStates[i].Position;
            allCubes[i].transform.rotation = savedCubeStates[i].Rotation;
        }

        Debug.Log("Cube state is loaded.");
    }
}