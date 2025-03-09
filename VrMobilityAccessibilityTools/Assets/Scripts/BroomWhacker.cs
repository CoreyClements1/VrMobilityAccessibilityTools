using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BroomWhacker : MonoBehaviour
{

    // BroomWhacker controls the particle showing and sweeping of the broom


    #region VARIABLES


    [SerializeField] private Rigidbody broomRb;
    [SerializeField] private ParticleSystem sweepParticles;
    private Vector3 prevPos, velocity;

    private List<GameObject> overlapped = new List<GameObject>();


    #endregion


    #region MONOBEHAVIOUR


    private void Start()
    {
        prevPos = transform.position;
    }


    private void FixedUpdate()
    {
        CalculateVelocity();
    }


    private void Update()
    {
        CheckForSweep();
    }


    #endregion


    #region SWEEPING


    private void CalculateVelocity()
    {
        velocity = (transform.position - prevPos) / Time.fixedDeltaTime;
        prevPos = transform.position;
    }


    private void CheckForSweep()
    {
        if (overlapped.Count > 0)
        {
            if (Mathf.Abs(velocity.magnitude) > .1f)
            {
                if (!sweepParticles.isPlaying)
                    sweepParticles.Play();
            }
            else if (sweepParticles.isPlaying)
            {
                sweepParticles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }
        }
        else if (sweepParticles.isPlaying)
        {
            sweepParticles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        }
    }


    #endregion


    #region TRIGGER


    private void OnTriggerEnter(Collider other)
    {
        if (!overlapped.Contains(other.gameObject))
        {
            overlapped.Add(other.gameObject);
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (overlapped.Contains(other.gameObject))
        {
            overlapped.Remove(other.gameObject);
        }
    }


    #endregion


}
