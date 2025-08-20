using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionManager : MonoBehaviour
{

    [SerializeField] private GameObject[] spiders;
    [SerializeField] private GameObject hat;
    [SerializeField] private Renderer leftHandRenderer, rightHandRenderer;
    [SerializeField] private Material standardHandMaterial, ghostHandMaterial;

    // Start is called before the first frame update
    void Start()
    {
#if VERAConditionGroup_Distractions
    if (VERAConditionGroup_Distractions.HandMaterialIndex.value == 0)
        {
            leftHandRenderer.material = ghostHandMaterial;
            rightHandRenderer.material = ghostHandMaterial;
        }
        else
        {
            leftHandRenderer.material = standardHandMaterial;
            rightHandRenderer.material = standardHandMaterial;
        }

        if (VERAConditionGroup_Distractions.HatEnabled.value)
        {
            hat.SetActive(true);
        } 
        else 
        {
            hat.SetActive(false);
        }
#endif

#if VERAConditionGroup_Arachnophobia
        if (VERAConditionGroup_Arachnophobia.SpidersEnabled.value)
        {
            foreach (GameObject spider in spiders)
            {
                spider.SetActive(true);
            }
        } 
        else 
        {
            foreach (GameObject spider in spiders)
            {
                spider.SetActive(false);
            }
        }
#endif
    }
}
