
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class SurfaceControl : MonoBehaviour
{

    public List<GameObject> surfaceObjects;
    public List<Surface> surfaces;
    public SlicePlane slicePlane;
    public int numberOfChildren;
    private bool buttonDown;
    private int selectedFunction;
    private List<int> randomFunctions;
    private Surface centralSurface;


    public void setFunction(int x)
    {
        selectedFunction = x;
    }

    public int getFunction()
    {
        return selectedFunction;
    }

    public bool checkButton()
    {
        return buttonDown;
    }

    public void buttonStatus(bool button)
    {
        buttonDown = button;
    }

    public void SetFunctions(List<int> rf)
    {
        randomFunctions = rf;
    }

    public void InitializeSurfaces()
    {
        numberOfChildren = transform.childCount;

        surfaceObjects = new List<GameObject>(numberOfChildren);
        surfaces = new List<Surface>(numberOfChildren);

        for (int i = 0; i < numberOfChildren; i++)
        {
            surfaceObjects.Add(transform.GetChild(i).gameObject);
            if (surfaceObjects[i].CompareTag("central") == false)
            {
                surfaces.Add(surfaceObjects[i].GetComponent<Surface>());
                surfaces[surfaces.Count - 1].function = randomFunctions[i];
            }
            else
            {
                surfaces.Add(surfaceObjects[i].GetComponent<Surface>());
            }
        }

    }
}