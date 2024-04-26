using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class SceneOneController : MonoBehaviour
{
    public GameObject surfacePrefab;
    private Surface[] surfaces;
    private int currentSurfaceIndex = 0;
    private XRController xrController;
    private InputAction gripAction;

    void Start()
    {
        surfaces = new Surface[3];
        for (int i = 0; i < 3; i++)
        {
            surfaces[i] = Instantiate(surfacePrefab).GetComponent<Surface>();
            surfaces[i].function = i; // Set function parameter
            surfaces[i].gameObject.SetActive(false);
        }

        // Get the XR controller
        xrController = GetComponent<XRController>();


        // Subscribe to the grip input action
        gripAction = new InputAction("grip", binding: "<XRController>{LeftHand}/grip");
        gripAction.performed += OnGripPerformed;
        gripAction.Enable();
        
        ShowNextSurface();
    }

    void OnGripPerformed(InputAction.CallbackContext context)
    {
        ShowNextSurface();
    }

    void ShowNextSurface()
    {
        surfaces[currentSurfaceIndex].gameObject.SetActive(false);
        currentSurfaceIndex = (currentSurfaceIndex + 1) % 3;
        surfaces[currentSurfaceIndex].gameObject.SetActive(true);
    }

    // Clean up the input system
    void OnDestroy()
    {
        gripAction.Disable();
    }
}