using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.XR.OpenXR.Features.Interactions.DPadInteraction;

public class PumpkinCannonball : MonoBehaviour
{

    // PumpkinCannonball handles a single pumpkin cannonball (fired from the pumpkin cannon)


    #region VARIABLES


    [SerializeField] private MeshFilter cannonballMeshFilter;
    [SerializeField] private List<Mesh> cannonballMeshes;
    [SerializeField] private ParticleSystem hitParticles;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider col;

    private float lifeTime = 10f;
    private bool dead = false;


    #endregion


    #region MONOBEHAVIOUR


    void Awake()
    {
        int rand = Random.Range(0, cannonballMeshes.Count);
        cannonballMeshFilter.mesh = cannonballMeshes[rand];
        Destroy(gameObject, lifeTime);
    }


    #endregion


    #region TRIGGER


    private void OnTriggerEnter(Collider other)
    {
        if (!dead && !other.gameObject.CompareTag("Cannon"))
            Die();
    }


    #endregion


    #region DEATH


    private void Die()
    {
        dead = true;

        cannonballMeshFilter.transform.LeanScale(Vector3.zero, .2f).setEaseOutExpo();
        hitParticles.Play();
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        col.enabled = false;

        Destroy(gameObject, 3f);
    }


    #endregion


}
