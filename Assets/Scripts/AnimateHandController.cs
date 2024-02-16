using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimateHandController : MonoBehaviour
{
    public InputActionReference gripInputActionReference;
    public InputActionReference triggerInputActionReference;

    private Animator _handAnimator;
    private float _gripValue;
    private float _triggerValue;
    private static readonly int Grip = Animator.StringToHash("Grip");
    private static readonly int Trigger = Animator.StringToHash("Trigger");
    
    void Start()
    {
        _handAnimator = GetComponent<Animator>();
    }
    
    void Update()
    {
        AnimateGrip();
        AnimateTrigger();
    }

    private void AnimateGrip()
    {
        _gripValue = gripInputActionReference.action.ReadValue<float>();
        _handAnimator.SetFloat(Grip, _gripValue);
    }

    private void AnimateTrigger()
    {
        _triggerValue = triggerInputActionReference.action.ReadValue<float>();
        _handAnimator.SetFloat(Trigger, _triggerValue);
    }
}
