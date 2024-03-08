using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LockPlane : MonoBehaviour
{
    public enum LockState
    {
        Off,
        XY,
        XZ,
        YZ
    };

    public bool lockedRotation;

    private XRGrabInteractable grabInteractable;
    private LockState current = LockState.Off;

    public void Start()
    {
        grabInteractable = this.gameObject.GetComponent<XRGrabInteractable>();
    }

    public void Lock()
    {
        if (lockedRotation)
        { 
            return;
        }
        else
        {
            int state = (int)current;
            state++;
            if (state > 3) state = 0;

            current = (LockState)state;

            if (current == LockState.Off)
            {
                grabInteractable.trackRotation = true;
                return;
            }

            grabInteractable.trackRotation = false;
            Quaternion newRot = this.transform.rotation;

            switch (current)
            {
                case LockState.XY:
                    newRot = Quaternion.AngleAxis(0, Vector3.up);
                    break;
                case LockState.XZ:
                    newRot = Quaternion.AngleAxis(90, Vector3.forward);
                    break;
                case LockState.YZ:
                    newRot = Quaternion.AngleAxis(90, Vector3.left);
                    break;
            }

            // the position setting doesn't work properly but it's good enough
            this.transform.SetPositionAndRotation(grabInteractable.interactorsSelecting[0].transform.position, newRot);
            // todo: lock properly
        }
    }
}