using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{

    // Grass controls the animations and scooping of grass


    #region VARIABLES


    [SerializeField] private ParticleSystem deathParticles;
    [SerializeField] private Transform grassMesh;
    private bool dead = false;


    #endregion


    #region TRIGGER


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Scooper") && !dead)
        {
            Die();
        }
    }


    #endregion


    #region DEATH


    private void Die()
    {
        dead = true;

        grassMesh.LeanScale(Vector3.zero, .5f).setEaseOutExpo();

        deathParticles.Play();

        Destroy(gameObject, 3f);
        CompletionCanvas.Instance.OnObjDestroyed();
    }


    #endregion


}
