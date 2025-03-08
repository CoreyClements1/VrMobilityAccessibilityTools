using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

public class Cannon : MonoBehaviour
{

    // Cannon handles the pumpkin cannon and its firing


    #region VARIABLES


    [SerializeField] private GameObject pumpkinCannonballPrefab;
    [SerializeField] private Transform pumpkinSpawnPos;
    [SerializeField] private float fireForce = 20f;
    [SerializeField] private ParticleSystem firingParticleSystem;

    private XRGrabInteractable grabInteractable;
    private float fireParticleDuration = .1f;


    #endregion


    #region MONOBEHAVIOUR


    private void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.activated.AddListener(Shoot);
    }


    #endregion


    #region FIRING


    void Shoot(ActivateEventArgs args)
    {
        GameObject cannonball = Instantiate(pumpkinCannonballPrefab, pumpkinSpawnPos.position, pumpkinSpawnPos.rotation);
        Rigidbody rb = cannonball.GetComponent<Rigidbody>();
        rb.velocity = pumpkinSpawnPos.forward * fireForce;
        firingParticleSystem.Play();
        StopCoroutine(WaitThenStopParticles());
        StartCoroutine(WaitThenStopParticles());
    }


    private IEnumerator WaitThenStopParticles()
    {
        yield return new WaitForSeconds(fireParticleDuration);
        firingParticleSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);
    }


    #endregion


}
