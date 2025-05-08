using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class GrabLogHandler : MonoBehaviour
{

    [SerializeField] private string itemName;
    XRGrabInteractable _grabInteractable;

    void Awake()
    {
        // Cache reference
        _grabInteractable = GetComponent<XRGrabInteractable>();
    }

    void OnEnable()
    {
        // Subscribe to the grab and release events
        _grabInteractable.selectEntered.AddListener(OnGrabbed);
        _grabInteractable.selectExited.AddListener(OnReleased);
    }

    void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        _grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        _grabInteractable.selectExited.RemoveListener(OnReleased);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        VERAFile_GrabTimes.CreateCsvEntry(0, itemName, "Grab");
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        VERAFile_GrabTimes.CreateCsvEntry(0, itemName, "Release");
    }
}
