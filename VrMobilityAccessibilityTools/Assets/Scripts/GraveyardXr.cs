using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraveyardXr : MonoBehaviour
{

    public static GraveyardXr Instance;

    [SerializeField] private SphereCollider headCollider;
    [SerializeField] private LayerMask headCollideLayerMask;

    [SerializeField] private float proxiFadeStart = .5f;
    [SerializeField] private float proxiFadeEnd = .1f;
    [SerializeField] private CanvasGroup proxiFadeCanvGroup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // Get nearby colliders within proxi fade
        Collider[] hits = Physics.OverlapSphere(headCollider.transform.position, proxiFadeStart, headCollideLayerMask, QueryTriggerInteraction.Ignore);

        // Get the closest collider within proxi fade
        float closestDist = proxiFadeStart;
        foreach (var col in hits)
        {
            Vector3 closestPoint = col.ClosestPoint(headCollider.transform.position);
            float d = Vector3.Distance(headCollider.transform.position, closestPoint);
            if (d < closestDist)
                closestDist = d;
        }

        // Map that distance to an alpha for the fade canvas group
        float t = Mathf.InverseLerp(proxiFadeEnd, proxiFadeStart, closestDist);
        proxiFadeCanvGroup.alpha = 1f - t;
    }

    private void FixedUpdate()
    {
        var hits = Physics.OverlapSphere(headCollider.transform.position, headCollider.radius, headCollideLayerMask);

        foreach (var hit in hits)
        {
            if (Physics.ComputePenetration(
                headCollider, headCollider.transform.position, Quaternion.identity,
                hit, hit.transform.position, hit.transform.rotation,
                out Vector3 pushDir, out float pushDist))
            {
                transform.position += pushDir * pushDist;
            }
        }
    }
}
