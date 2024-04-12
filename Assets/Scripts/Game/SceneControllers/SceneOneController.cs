using UnityEngine;

public class SceneOneController : MonoBehaviour
{
    public GameObject surfacePrefab;
    private Surface[] surfaces;
    private int currentSurfaceIndex = 0;

    void Start()
    {
        surfaces = new Surface[3];
        for (int i = 0; i < 3; i++)
        {
            surfaces[i] = Instantiate(surfacePrefab).GetComponent<Surface>();
            surfaces[i].function = i; // Set function parameter
            surfaces[i].gameObject.SetActive(false);
        }
        ShowNextSurface();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShowNextSurface();
        }
    }

    void ShowNextSurface()
    {
        surfaces[currentSurfaceIndex].gameObject.SetActive(false);
        currentSurfaceIndex = (currentSurfaceIndex + 1) % 3;
        surfaces[currentSurfaceIndex].gameObject.SetActive(true);
    }
}