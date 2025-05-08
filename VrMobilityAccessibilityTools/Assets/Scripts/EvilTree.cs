using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvilTree : MonoBehaviour
{

    // CleanableObject controls the animations and destruction of a cleanable object


    #region VARIABLES


    [SerializeField] private ParticleSystem deathParticles;
    [SerializeField] private Transform evilMesh;
    [SerializeField] private Transform goodMesh;
    private bool dead = false;

    private int numHits = 0;
    private int maxHits = 3;


    #endregion


    #region TRIGGER


    private void OnTriggerEnter(Collider other)
    {
        if (!dead)
        {
            if (other.CompareTag("Chopper"))
                TestVelocityAndDamage(other);
            else if (other.CompareTag("Projectile"))
                Damage(false);
        }

    }


    #endregion


    #region DEATH


    private void TestVelocityAndDamage(Collider other)
    {
        if (VMAT_Options.Instance != null && VMAT_Options.Instance.normalizationEnabled)
        {
            Damage(true);
            return;
        }

        VelocityTracker vt = other.GetComponent<VelocityTracker>();
        if (vt == null)
        {
            vt = other.transform.parent.GetComponent<VelocityTracker>();

            if (vt == null)
                return;
        }

        if (vt.velocityMagnitude > .08f)
        {
            Damage(true);
        }
    }


    private void Damage(bool isChop)
    {
        numHits++;
        SfxManager.Instance.PlaySfx(SfxManager.SoundEffect.Ouch, transform.position, false);
        if (isChop)
            SfxManager.Instance.PlaySfx(SfxManager.SoundEffect.WoodChop, transform.position, false);

        if (numHits >= maxHits)
            StartCoroutine(Die());
        else
        {
            Renderer treeRend = evilMesh.GetComponent<Renderer>();
            LeanTween.cancel(treeRend.gameObject);
            LeanTween.value(treeRend.gameObject, Color.black, Color.red, .1f).setLoopPingPong(1).setOnUpdate((Color value) =>
            {
                treeRend.material.SetColor("_EmissionColor", value);
            });
        }
    }


    private IEnumerator Die()
    {
        dead = true;
        GetComponent<Collider>().enabled = false;

        deathParticles.Play();
        CompletionCanvas.Instance.OnObjDestroyed();

        VERAFile_CleanTimes.CreateCsvEntry(0, "Tree", transform);

        evilMesh.LeanScale(Vector3.zero, .5f).setEaseOutExpo();
        yield return new WaitForSeconds(.1f);
        goodMesh.LeanScale(Vector3.one, .5f).setEaseOutExpo();
    }


    #endregion


}
