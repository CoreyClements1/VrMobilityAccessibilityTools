using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CleanableObject : MonoBehaviour
{

    // CleanableObject controls the animations and destruction of a cleanable object


    #region VARIABLES


    private enum CleanableType { Shovel, Broom, Axe, Pickaxe }
    [SerializeField] private CleanableType cleanableType;
    [SerializeField] private ParticleSystem deathParticles;
    [SerializeField] private Transform grassMesh;
    [SerializeField] private Transform replacementMesh;
    [SerializeField] private float necessaryVelocity = 0.08f;
    [SerializeField] private UnityEvent onClean;
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
                case CleanableType.Pickaxe:
                    if (other.CompareTag("Breaker"))
                        TestVelocityAndDie(other, true);
                    break;
            }
        }

    }


    #endregion


    #region DEATH


    private void TestVelocityAndDie(Collider other, bool replaceOnDie = false)
    {
        VelocityTracker vt = other.GetComponent<VelocityTracker>();
        if (vt == null)
        {
            vt = other.transform.parent.GetComponent<VelocityTracker>();

            if (vt == null)
                return;
        }

        if (vt.velocityMagnitude > necessaryVelocity)
        {
            Die(replaceOnDie);
        }
    }


    private void Die(bool replaceOnDie)
    {
        dead = true;

        grassMesh.LeanScale(Vector3.zero, .5f).setEaseOutExpo();

        deathParticles.Play();

        if (replaceOnDie)
        {
            replacementMesh.LeanScale(Vector3.one, .5f).setEaseOutExpo();
        }
        else
        {
            Destroy(gameObject, 3f);
        }
        
        if (CompletionCanvas.Instance != null)
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
            case CleanableType.Pickaxe:
                SfxManager.Instance.PlaySfx(SfxManager.SoundEffect.Pickaxe, transform.position, true);
                break;
        }

        onClean?.Invoke();
    }


    #endregion


}
