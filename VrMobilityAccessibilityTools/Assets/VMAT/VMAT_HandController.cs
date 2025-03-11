using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

public class VMAT_HandController : MonoBehaviour
{

    // VMAT_HandController is to be placed on a VR hand controller, and provides additional VMAT controls for it


    #region VARIABLES


    private Transform offsetTransform;
    private Vector3 originPositionLocal;

    private bool useReachExtension = false;
    private float reachExtensionScale = 2f;


    #endregion


    #region MONOBEHAVIOUR


    private void Awake()
    {
        SetupOffset();
    }


    private void Update()
    {
        if (useReachExtension)
            HandleReachExtension();
        else
            HandleTrackController();
    }


    #endregion


    #region REACH EXTENSION


    // Spawn offset object and set all child objects to be parented to it
    private void SetupOffset()
    {
        offsetTransform = new GameObject("VMAT Controller Offset").transform;
        offsetTransform.SetParent(transform.parent);
        offsetTransform.localPosition = transform.localPosition;

        List<Transform> children = new List<Transform>();
        foreach (Transform child in transform)
            children.Add(child);

        for (int i = children.Count - 1; i >= 0; i--)
            children[i].SetParent(offsetTransform);
    }


    // Handles reach extension by using offsets
    private void HandleReachExtension()
    {
        if (transform == null)
            return;

        Vector3 offset = transform.localPosition - originPositionLocal;
        offsetTransform.localPosition = originPositionLocal + reachExtensionScale * offset;

        offsetTransform.localRotation = transform.localRotation;
        offsetTransform.localScale = transform.localScale;
    }


    // Handles directly tracking the controller (no reach extension)
    private void HandleTrackController()
    {
        offsetTransform.localPosition = transform.localPosition;
        offsetTransform.localRotation = transform.localRotation;
        offsetTransform.localScale = transform.localScale;
    }


    // Resets the reach extension "origin" to the current controller position
    public void ResetReachExtensionOrigin()
    {
        originPositionLocal = transform.localPosition;
    }


    // Sets whether reach extension is enabled or not (and resets the origin)
    public void SetEnabledReachExtension(bool enabled)
    {
        useReachExtension = enabled;

        ResetReachExtensionOrigin();
    }

    
    // Sets the scale of reach extension
    public void SetReachExtensionScale(float scale)
    {
        reachExtensionScale = scale;
    }


    #endregion


} // END VMAT_HandController.cs
