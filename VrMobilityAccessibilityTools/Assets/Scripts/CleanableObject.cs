using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CleanableObject : MonoBehaviour
{

    // CleanableObject controls the animations and destruction of a cleanable object


    #region VARIABLES


    private enum CleanableType { Shovel, Broom, Axe }
    [SerializeField] private CleanableType cleanableType;
    [SerializeField] private ParticleSystem deathParticles;
    [SerializeField] private Transform grassMesh;
    private bool dead = false;


    #endregion


    #region TRIGGER


    private void OnTriggerEnter(Collider other)
    {
        if (!dead)
        {
            switch(cleanableType)
            {
                case CleanableType.Shovel:
                    if (other.CompareTag("Scooper"))
                        TestVelocityAndDie(other);
                    break;
                case CleanableType.Broom:
                    if (other.CompareTag("Whacker"))
                        TestVelocityAndDie(other);
                    break;
                case CleanableType.Axe:
                    if (other.CompareTag("Chopper"))
                        TestVelocityAndDie(other);
                    break;
            }
        }

    }


    #endregion


    #region DEATH


    private void TestVelocityAndDie(Collider other)
    {
        VelocityTracker vt = other.GetComponent<VelocityTracker>();
        if (vt == null)
        {
            vt = other.transform.parent.GetComponent<VelocityTracker>();

            if (vt == null)
                return;
        }

        if (vt.velocityMagnitude > .08f)
        {
            Die();
        }
    }


    private void Die()
    {
        dead = true;

        grassMesh.LeanScale(Vector3.zero, .5f).setEaseOutExpo();

        deathParticles.Play();

        Destroy(gameObject, 3f);
        CompletionCanvas.Instance.OnObjDestroyed();

        switch (cleanableType)
        {
            case CleanableType.Shovel:
                SfxManager.Instance.PlaySfx(SfxManager.SoundEffect.Shovel, transform.position, true);
                break;
            case CleanableType.Broom:
                SfxManager.Instance.PlaySfx(SfxManager.SoundEffect.Sweep, transform.position, true);
                break;
            case CleanableType.Axe:
                SfxManager.Instance.PlaySfx(SfxManager.SoundEffect.PumpkinChop, transform.position, true);
                break;
        }
    }


    #endregion


}
