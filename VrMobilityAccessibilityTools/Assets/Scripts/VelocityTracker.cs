using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityTracker : MonoBehaviour
{

    private Vector3 lastPos;
    public float velocityMagnitude { get; private set; } = 0f;

    void Awake()
    {
        lastPos = transform.position;
    }

    private void FixedUpdate()
    {
        velocityMagnitude = (transform.position - lastPos).magnitude;
        lastPos = transform.position;
    }
}
