using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    // Enemy controls the animations and squishing of an enemy (bat or spider)


    #region VARIABLES


    public enum EnemyType { Spider, Bat }
    [SerializeField] private EnemyType enemyType;
    [SerializeField] private ParticleSystem deathParticles;
    [SerializeField] private Transform enemyMesh;
    [SerializeField] private Animator enemyAnimator;
    [SerializeField] private bool lookAtPlayer = false;
    [SerializeField] private bool idleMove = true;
    [SerializeField] private bool moveVertically = false;
    [SerializeField] private float moveDurationMultiplier = 1f;
    [SerializeField] private float idleDurationMultiplier = 1f;
    [SerializeField] private float moveDist = .2f;
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

    private void Update()
    {
        if (lookAtPlayer)
        {
            transform.LookAt(Camera.main.transform);
        }
    }


    #endregion


    #region MOVEMENT


    private IEnumerator IdleMoveCo()
    {
        while (!dead)
        {
            Vector3 randPos = startPos + Vector3.right * Random.Range(-moveDist, moveDist) + Vector3.forward * Random.Range(-moveDist, moveDist);
            if (moveVertically)
                randPos += Vector3.up * Random.Range(-moveDist, moveDist);

            float moveTime = (Mathf.Abs((randPos - transform.position).magnitude) / .5f) * moveDurationMultiplier;
            transform.LeanMove(randPos, moveTime).setEaseOutQuad();

            enemyAnimator.SetBool("Moving", true);

            Vector3 lookDir = (randPos - transform.position).normalized;
            Quaternion startRot = transform.rotation;
            Quaternion endRot = Quaternion.LookRotation(lookDir);

            float elapsedTime = 0f;
            while (elapsedTime < moveTime)
            {
                if (!lookAtPlayer)
                    transform.rotation = Quaternion.Slerp(startRot, endRot, elapsedTime / (moveTime / 4f));

                if (dead)
                {
                    LeanTween.cancel(gameObject);
                    break;
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            enemyAnimator.SetBool("Moving", false);

            if (!dead)
                yield return new WaitForSeconds(Random.Range(.5f * idleDurationMultiplier, 2f * idleDurationMultiplier));
        }
        
    }


    #endregion


    #region TRIGGER


    private void OnTriggerEnter(Collider other)
    {
        if (!dead)
        {
            if (other.CompareTag("Projectile"))
            {
                StartCoroutine(Die());
            }
            else if (other.CompareTag("Whacker") || other.CompareTag("Scooper") || other.CompareTag("Chopper") || other.CompareTag("Breaker"))
            {
                TestVelocityAndDie(other);
            }
        }
    }


    private void TestVelocityAndDie(Collider other)
    {
        VelocityTracker vt = other.GetComponent<VelocityTracker>();
        if (vt == null)
        {
            if (other.transform.parent == null)
                return;

            vt = other.transform.parent.GetComponent<VelocityTracker>();

            if (vt == null)
                return;
        }

        if (vt.velocityMagnitude > .08f)
        {
            StartCoroutine(Die());
        }
    }


    #endregion


    #region DEATH


    private IEnumerator Die()
    {
        dead = true;

        enemyAnimator.SetBool("Dead", true);
        SfxManager.Instance.PlaySfx(SfxManager.SoundEffect.Ouch, transform.position, false);

        if (enemyType == EnemyType.Bat)
            yield return new WaitForSeconds(.5f);
        else
            yield return null;

        enemyMesh.LeanScale(Vector3.zero, .5f).setEaseOutExpo();
        deathParticles.Play();

        Destroy(gameObject, 3f);
        CompletionCanvas.Instance.OnObjDestroyed();
    }


    #endregion


}
