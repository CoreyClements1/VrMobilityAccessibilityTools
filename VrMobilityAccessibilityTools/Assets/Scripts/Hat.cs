using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hat : MonoBehaviour
{

    // WitchHat controls the placement and release of the witch hat on the player's head


    #region VARIABLES


    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider col;


    #endregion


    #region HAT ATTACH


    // When grab ends, see if we are overlapping hat attach; if yes, attach hat.
    public void OnEndGrab()
    {
        HatAttach hatAttach = FindObjectOfType<HatAttach>();
        if (hatAttach.IsHatOverlapped(this))
        {
            hatAttach.TryAttachHat(this);
        }
        else if (hatAttach.attachedHat == this)
        {
            hatAttach.UnattachHat();
        }

    } // END OnEndGrab


    #endregion


    #region COLLISION


    // Enables collision
    public void EnableCollision()
    {
        rb.useGravity = true;
        rb.isKinematic = false;
        col.isTrigger = false;
        
    } // END EnableCollision


    // Disables collision
    public void DisableCollision()
    {
        rb.useGravity = false;
        rb.isKinematic = true;
        col.isTrigger = true;

    } // END DisableCollision


    #endregion


} // END WitchHat.cs
