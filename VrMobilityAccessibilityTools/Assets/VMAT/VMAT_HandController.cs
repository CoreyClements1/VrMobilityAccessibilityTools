using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VMAT_HandController : MonoBehaviour
{

    // VMAT_HandController is to be placed on a VR hand controller, and provides additional VMAT controls for it


    #region VARIABLES


    private Transform offsetTransform;

    private bool useReachExtension = false;
    private float reachExtensionScale = 2f;
    private Vector3 restingControllerOffset;
    private float restingYawOffset;

    private bool useNormalization = false;
    private float normalizationScale = 0.2f;
    private Queue<KeyValuePair<Vector3, float>> lastPositions = new Queue<KeyValuePair<Vector3, float>>();
    private Queue<KeyValuePair<Quaternion, float>> lastRotations = new Queue<KeyValuePair<Quaternion, float>>();


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

        if (useNormalization)
            HandleNormalization();
    }

    private void OnEnable()
    {
        ResetReachExtensionOrigin();
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
            children[i].SetParent(offsetTransform, false);

        ResetReachExtensionOrigin();
    }


    // Handles reach extension by using offsets
    private void HandleReachExtension()
    {
        if (transform == null)
            return;

        Vector3 offsetFromCamera = transform.position - Camera.main.transform.position;
        Vector3 offsetFromResting = offsetFromCamera - RotateWithRig(restingControllerOffset);
        offsetTransform.position = Camera.main.transform.position + 
            RotateWithRig(restingControllerOffset) + 
            reachExtensionScale * offsetFromResting;

        offsetTransform.localRotation = transform.localRotation;
    }

    private Vector3 RotateWithRig(Vector3 vectorToRotate)
    {
        float currentYaw = Camera.main.transform.eulerAngles.y;
        float relativeYaw = Mathf.DeltaAngle(restingYawOffset, currentYaw);

        Quaternion rot = Quaternion.Euler(0f, relativeYaw, 0f);
        return rot * vectorToRotate;
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
        restingControllerOffset = transform.position - Camera.main.transform.position;
        restingYawOffset = Camera.main.transform.eulerAngles.y;
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


    #region NORMALIZATION


    // Normalizes inputs (averages inputs from last second or so)
    private void HandleNormalization()
    {
        // Positions
        lastPositions.Enqueue(new KeyValuePair<Vector3, float>(offsetTransform.position, Time.time));
        while (lastPositions.Count > 0 && Time.time - lastPositions.Peek().Value > normalizationScale)
        {
            lastPositions.Dequeue();
        }

        Vector3 posSum = Vector3.zero;
        foreach (var v in lastPositions)
        {
            posSum += v.Key;
        }
        Vector3 posAverage = posSum / lastPositions.Count;

        offsetTransform.position = posAverage;

        // Rotations
        lastRotations.Enqueue(new KeyValuePair<Quaternion, float>(offsetTransform.rotation, Time.time));
        while (lastRotations.Count > 0 && Time.time - lastRotations.Peek().Value > normalizationScale)
        {
            lastRotations.Dequeue();
        }

        Vector4 rotSum = Vector4.zero;
        foreach (var q in lastRotations)
        {
            Quaternion q2 = q.Key;
            if (Quaternion.Dot(q.Key, lastRotations.Peek().Key) < 0f)
                q2 = new Quaternion(-q.Key.x, -q.Key.y, -q.Key.z, -q.Key.w);

            rotSum += new Vector4(q2.x, q2.y, q2.z, q2.w);
        }

        // Normalize the result back to unit‑length
        float invMag = 1.0f / rotSum.magnitude;
        Quaternion averageRot = new Quaternion(rotSum.x * invMag, rotSum.y * invMag, rotSum.z * invMag, rotSum.w * invMag);
        offsetTransform.rotation = averageRot;
    }


    // Sets whether normalization is enabled or not
    public void SetEnabledNormalization(bool enabled)
    {
        useNormalization = enabled;
    }


    // Sets the scale of normalization
    public void SetNormalizationScale(float scale)
    {
        normalizationScale = scale;
    }


    #endregion


} // END VMAT_HandController.cs
