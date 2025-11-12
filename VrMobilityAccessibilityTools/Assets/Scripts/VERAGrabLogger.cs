using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using VERA;

public class VERAGrabLogger : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
            grabInteractable.activated.AddListener(OnUsed);
        }
        else
        {
            Debug.LogWarning($"No XRGrabInteractable found on {gameObject.name}");
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        #if VERAFile_SpecialEvents
        VERAFile_SpecialEvents.CreateCsvEntry(2, "ToolGrabbed", gameObject.name, transform);
        #endif
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        #if VERAFile_SpecialEvents
        VERAFile_SpecialEvents.CreateCsvEntry(3, "ToolReleased", gameObject.name, transform);
        #endif
    }

    private void OnUsed(ActivateEventArgs args)
    {
        #if VERAFile_SpecialEvents
        VERAFile_SpecialEvents.CreateCsvEntry(4, "ToolUsed", gameObject.name, transform);
        #endif
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
            grabInteractable.activated.RemoveListener(OnUsed);
        }
    }
}
