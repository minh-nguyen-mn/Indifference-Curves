using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class testbutton : MonoBehaviour
{
    [SerializeField]
    InputActionProperty _mButtonInput = new InputActionProperty(new InputAction("Test Button", expectedControlType: "Button"));
    private InputActionProperty buttonInput
    {
        get => _mButtonInput;
        set => SetInputActionProperty(ref _mButtonInput, value);
    }
    
    void SetInputActionProperty(ref InputActionProperty property, InputActionProperty value)
    {
        if (Application.isPlaying)
            property.DisableDirectAction();

        property = value;

        if (Application.isPlaying && isActiveAndEnabled)
            property.EnableDirectAction();
    }

    private void Update()
    {
        var test = buttonInput.action.ReadValue<InputHelpers.Button>();
        
    }
}
