using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HatAttach : MonoBehaviour
{

    // HatAttach handles attaching and releasing a hat from the player's head


    #region VARIABLES


    public Hat attachedHat { get; private set; }
    private List<Hat> overlappedHats = new List<Hat>();

    [SerializeField] private Transform hatAttachPoint;


    #endregion


    #region TRIGGER


    // Enter trigger, update overlapped if it's a hat
    private void OnTriggerEnter(Collider other)
    {
        Hat hat = other.GetComponent<Hat>();

        if (hat != null)
        {
            if (!overlappedHats.Contains(hat))
                overlappedHats.Add(hat);
        }

    } // END OnTriggerEnter


    // Exit trigger, update overlapped if it's a hat
    private void OnTriggerExit(Collider other)
    {
        Hat hat = other.GetComponent<Hat>();

        if (hat != null)
        {
            if (overlappedHats.Contains(hat))
                overlappedHats.Remove(hat);
        }

    } // END OnTriggerExit

    
    // Returns whether a given hat is currently overlapped by the hat attach
    public bool IsHatOverlapped(Hat hat)
    {
        return overlappedHats.Contains(hat);

    } // END IsHatOverlapped


    #endregion


    #region ATTACH HAT


    // Tries to attach the hat
    public void TryAttachHat(Hat hat)
    {
        if (attachedHat != null)
            UnattachHat();

        attachedHat = hat;

        attachedHat.DisableCollision();
        attachedHat.transform.SetParent(hatAttachPoint);
        attachedHat.transform.localPosition = Vector3.zero;
        attachedHat.transform.localRotation = Quaternion.identity;

    } // END TryAttachHat


    // Unattaches the currently attached hat
    public void UnattachHat()
    {
        attachedHat.transform.SetParent(null);
        attachedHat.EnableCollision();
        attachedHat = null;

    } // END UnattachHat


    #endregion


}
