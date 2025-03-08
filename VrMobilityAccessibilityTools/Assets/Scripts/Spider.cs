using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider : MonoBehaviour
{

    // Spider controls the animations and squishing of a spider


    #region VARIABLES


    [SerializeField] private ParticleSystem deathParticles;
    [SerializeField] private Transform spiderMesh;
    [SerializeField] private Animator spiderAnimator;
    [SerializeField] private bool idleMove = true;
    private bool dead = false;

    private Vector3 startPos;


    #endregion


    #region MONOBEHAVIOUR


    private void Awake()
    {
        startPos = transform.position;

        if (idleMove)
            StartCoroutine(IdleMoveCo());
    }


    #endregion


    #region MOVEMENT


    private IEnumerator IdleMoveCo()
    {
        while (!dead)
        {
            Vector3 randPos = startPos + Vector3.right * Random.Range(-.2f, .2f) + Vector3.forward * Random.Range(-.2f, .2f);
            float moveTime = Mathf.Abs((randPos - transform.position).magnitude) / .5f;
            transform.LeanMove(randPos, moveTime).setEaseOutQuad();

            spiderAnimator.SetBool("Walking", true);

            Vector3 lookDir = (randPos - transform.position).normalized;
            Quaternion startRot = transform.rotation;
            Quaternion endRot = Quaternion.LookRotation(lookDir);

            float elapsedTime = 0f;
            while (elapsedTime < moveTime)
            {
                transform.rotation = Quaternion.Slerp(startRot, endRot, elapsedTime / (moveTime / 4f));

                if (dead)
                {
                    LeanTween.cancel(gameObject);
                    break;
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            spiderAnimator.SetBool("Walking", false);

            if (!dead)
                yield return new WaitForSeconds(Random.Range(.5f, 2f));
        }
        
    }


    #endregion


    #region TRIGGER


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Whacker") && !dead)
        {
            StartCoroutine(Die());
        }
    }


    #endregion


    #region DEATH


    private IEnumerator Die()
    {
        dead = true;

        spiderAnimator.SetBool("Dead", true);

        yield return null;
        spiderMesh.LeanScale(Vector3.zero, .5f).setEaseOutExpo();

        deathParticles.Play();

        Destroy(gameObject, 3f);
    }


    #endregion


}
