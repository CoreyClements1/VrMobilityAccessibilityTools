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
        VERAConditionGroup_Distractions.HandMaterialIndex.GetValueAsync((int value) =>
        {
            if (value == 0)
            {
                leftHandRenderer.material = ghostHandMaterial;
                rightHandRenderer.material = ghostHandMaterial;
            }
            else
            {
                leftHandRenderer.material = standardHandMaterial;
                rightHandRenderer.material = standardHandMaterial;
            }
        });
        
        VERAConditionGroup_Distractions.HatEnabled.GetValueAsync((bool value) =>
        {
            if (value)
            {
                hat.SetActive(true);
            }
            else
            {
                hat.SetActive(false);
            }
        });
#endif

#if VERAConditionGroup_Arachnophobia
        VERAConditionGroup_Arachnophobia.SpidersEnabled.GetValueAsync((bool value) =>
        {
            if (value)
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
        });
        #endif
    }
}
