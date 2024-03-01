using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LockPlane;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SceneOneController : GameController
{

    public int numFunctions = 8;
    public SurfaceControl surfaceControl;
    public Surface centralSurface;
    public SlicePlane slicePlane;
    private string step;
    private float time;
    private bool timer;
    private bool played;
    private Renderer scren;
    private List<int> randomFunctions;
    private int function;
    private int selectedFunction;
    private int function1;
    private int function2;
    private bool buttonDown;
    private float wait = 2.0f;
    private bool pause=false;
    public GameObject player;
    public InputActionProperty rightTrigger;
    public InputActionProperty leftTrigger;
    public InputActionProperty rightToggle;
    public InputActionProperty leftToggle;
    private float timeMoving =  0.0f;
    private float timeTurning = 0.0f;

    public void Awake()
    {
        messageControl.Initialize("01");
        RandomFunctions(); //Selects functions for outer surfaces
        surfaceControl.SetFunctions(randomFunctions); //Sends information to SurfaceControl
        surfaceControl.InitializeSurfaces(); //SurfaceControl gets surfaces and sends information to surfaces
        function = Random.Range(0, numFunctions);  //choose the function to apply to the central stand
        function1 = function;
        centralSurface.function = function;
        scren = sceneChanger.GetComponent<Renderer>();
        scren.enabled = false;
    }

    public void Start()
    {
        step = "Start";
        time = Time.fixedTime;
        slicePlane.SetOrientation(1);
    }
    public void Update()
    {

        ToggleCheck();

        TriggerForOlderMessages();

        if (pause)
        {
            return;
        }

        switch (step)
        {
            case "Start":
                if (Elapsed())
                {
                    wait = messageControl.Play(1);
                    step = "wait for movement";
                }
                break;

            case "wait for movement":
                if (Elapsed())
                {
                    timeMoving = 0.0f;
                    step = "wait for movement 2";
                }
                break;

            case "wait for movement 2":
                if (timeMoving > 1.5f)
                {
                    wait = messageControl.Play(2);
                    step = "wait for turn";
                }
                break;

            case "wait for turn":
                if (Elapsed())
                {
                    timeTurning = 0.0f;
                    step = "review messages";
                }
                break;

            case "review messages":
                if (timeTurning > 1.5f)
                {
                    wait = messageControl.Play(3);
                    wait += 3.0f;
                    step = "move to central stand";
                }
                break;

            case "move to central stand":
                if (Elapsed())
                {
                    wait = messageControl.Play(4);
                    step = "wait for proximity";
                }
                break;

            case "wait for proximity":
                if (Elapsed())
                {
                    step = "viewing lens";
                }
                break;

            case "viewing lens":
                if (ProximityCheck())
                {
                    wait = messageControl.Play(5);
                    step = "Grab the lens";
                }
                break;

            case "Grab the lens":
                if (Elapsed())
                {
                    wait = messageControl.Play(6);
                    step = "wait for grab";
                }
                break;

            case "find visible surface":

                if (Elapsed())
                {
                    wait = messageControl.Play(8);
                    step = "first exercise";
                }
                break;

            case "first exercise":

                buttonDown = surfaceControl.checkButton();

                if (buttonDown)
                {
                    selectedFunction = surfaceControl.getFunction();

                    if (selectedFunction == function)
                    {
                        wait = messageControl.Play(10);
                        step = "second exercise 1";
                    }
                    else
                    {
                        wait = messageControl.Play(9);
                    }
                }
                break;

            case "second exercise 1":

                if (Elapsed())
                {
                    wait = messageControl.Play(11);
                    slicePlane.transform.SetPositionAndRotation(new Vector3(0, 1.5f, 0), Quaternion.AngleAxis(90, Vector3.forward));
                    slicePlane.SetOrientation(2);

                    while (function == function1)
                    {
                        function = Random.Range(0, 8);  //choose the function to apply to the central stand
                    }
                    function2 = function;
                    centralSurface.function = function;
                    centralSurface.Render();
                    step = "second exercise 2";
                }
                break;

            case "second exercise 2":

                if (Elapsed() && played==false)
                {
                    wait = messageControl.Play(12);
                    played = true;
                }
    
                buttonDown = surfaceControl.checkButton();

                if (buttonDown)
                {
                    selectedFunction = surfaceControl.getFunction();

                    if (selectedFunction == function)
                    {
                        wait = messageControl.Play(10);
                        step = "third exercise 1";
                        played = false;
                        timer = false;
                    }
                    else
                    {
                        wait = messageControl.Play(9);
                    }
                }
                break;

            case "third exercise 1":

                if (Elapsed())
                {
                    wait = messageControl.Play(13);
                    slicePlane.transform.SetPositionAndRotation(new Vector3(0, 1.5f, 0), Quaternion.AngleAxis(90, Vector3.left));
                    slicePlane.SetOrientation(3);
                    while (function == function1 || function == function2)
                    {
                        function = Random.Range(0, 8);  //choose the function to apply to the central stand
                    }
                    centralSurface.function = function;
                    centralSurface.Render();
                    step = "third exercise 2";
                }
                break;

            case "third exercise 2":

                if (Elapsed() && played==false)
                {
                    wait = messageControl.Play(12);
                    played = true;
                }

                buttonDown = surfaceControl.checkButton();

                if (buttonDown)
                {
                    selectedFunction = surfaceControl.getFunction();

                    if (selectedFunction == function)
                    {
                        wait = messageControl.Play(14);
                        played = false;
                        timer = false;
                        step = "time to investigate";
                    }
                    else
                    {
                        wait = messageControl.Play(9);
                    }
                }
                break;

            case "time to investigate":

                if (Elapsed())
                {
                    wait = messageControl.Play(15);
                    slicePlane.Release();
//                    slicePlane.gameObject.GetComponent<XRGrabInteractable>().trackRotation = true;
                    slicePlane.transform.SetPositionAndRotation(new Vector3(0, 1.5f, 0), Quaternion.AngleAxis(71, new Vector3(1, 5, 11)));
                    slicePlane.SetOrientation(0);
                    centralSurface.Render();
                    step = "prepare for next scene";
                }
                break;

            case "prepare for next scene":

                if (Elapsed())
                {
                    wait = messageControl.Play(16);
                    step = "next scene";
                }
                break;

            case "next scene":

                scren.enabled = true;
                this.Ready("Second Scene");
                step = "finished";
                break;
        }

        return;
    }

    public void AdvanceToSecond()
    {
        if (step == "wait for grab")
        {
            wait = messageControl.Play(7);
            step = "find visible surface";
        }
    }


    public int GetFunction()
    {
        return function;
    }

    public void SetFunction(int x)
    {
        function = x;
    }

    private void RandomFunctions()
    {
        randomFunctions = new List<int>();
        int j;
        bool newInt;
        for (int i = 0; i < numFunctions; i++)
        {
            j = Random.Range(0, numFunctions);
            newInt = false;
            while (newInt == false)
            {
                newInt = true;
                for (int k = 0; k < i; k++)
                {
                    if (randomFunctions[k] == j)
                    {
                        newInt = false;
                        j = Random.Range(0, numFunctions);
                    }
                }
            }
            randomFunctions.Add(j);
        }
    }




    public bool Elapsed()
    {
        if (timer == false)
        {
            timer = true;
            time = Time.fixedTime;
            return false;
        }
        else
        {
            if (Time.fixedTime > time + wait)
            {
                timer = false;
                return true;
            }
            else
            {
                return false;
            }
        }
    }


    private void TriggerForOlderMessages()
    {
        if (rightTrigger.action.triggered)
        { 
            pause=messageControl.ShowNewer();
        }
        if (leftTrigger.action.triggered)
        {
            pause=messageControl.ShowOlder(pause);
        }
        return;
    }

    private void ToggleCheck()
    {
        if (leftToggle.action.ReadValue<Vector2>().magnitude > .01)
        {
            timeMoving += Time.deltaTime;
        }

        if (rightToggle.action.ReadValue<Vector2>().magnitude > .01)
        {
            timeTurning += Time.deltaTime;
        }
    }

    private bool ProximityCheck()
    {
        if (player.transform.position.x * player.transform.position.x + player.transform.position.z * player.transform.position.z < 2.25f)
        {
            return true;
        }
        else
        {
            return false;
        }

    }
    
}