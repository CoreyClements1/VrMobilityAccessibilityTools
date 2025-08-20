
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class InteractionsLogger : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
        {
            Debug.LogError("XRGrabInteractable component not found on this GameObject.");
            return;
        }

        grabInteractable.selectEntered.AddListener(OnPickedUp);
        grabInteractable.selectExited.AddListener(OnDropped);
        grabInteractable.activated.AddListener(OnUsed);
    }

    private void OnPickedUp(SelectEnterEventArgs args)
    {
        #if VERAFile_Interactions
        VERAFile_Interactions.CreateCsvEntry(0, "Pickup", gameObject.name, transform);
        #endif
    }

    private void OnDropped(SelectExitEventArgs args)
    {
        #if VERAFile_Interactions
        VERAFile_Interactions.CreateCsvEntry(1, "Drop", gameObject.name, transform);
        #endif
    }

    private void OnUsed(ActivateEventArgs args)
    {
        #if VERAFile_Interactions
        VERAFile_Interactions.CreateCsvEntry(2, "Used", gameObject.name, transform);
        #endif
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnPickedUp);
            grabInteractable.selectExited.RemoveListener(OnDropped);
            grabInteractable.activated.RemoveListener(OnUsed);
        }
    }
}
