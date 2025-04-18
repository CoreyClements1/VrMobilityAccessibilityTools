using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandAnimator : MonoBehaviour
{

    [SerializeField] private InputActionProperty triggerAction;
    [SerializeField] private InputActionProperty gripAction;

    [SerializeField] private Animator anim;

    private void Update()
    {
        float triggerVal = triggerAction.action.ReadValue<float>();
        float gripVal = gripAction.action.ReadValue<float>();

        anim.SetFloat("Trigger", triggerVal);
        anim.SetFloat("Grip", gripVal);
    }
}
