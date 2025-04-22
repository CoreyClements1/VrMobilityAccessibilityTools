using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

public class VMAT_HeightAdjuster : MonoBehaviour
{

    // VMAT_HeightAdjuster provides functionality for adjusting the height of the XR player


    #region VARIABLES


    private float baseHeight;
    private float heightScale = 1f;


    #endregion


    #region HEIGHT


    private void OnEnable()
    {
        StartCoroutine(WaitThenReset());
    }

    private IEnumerator WaitThenReset()
    {
        yield return null;
        yield return null;

        if (PlayerPrefs.GetFloat("VMAT_HeightAdjustment", -1f) != -1f)
            SetHeightScale(PlayerPrefs.GetFloat("VMAT_HeightAdjustment"));
    }


    // Sets the height scale
    public void SetHeightScale(float heightScale)
    {
        this.heightScale = heightScale;
        transform.localPosition = Vector3.up * (heightScale - 0.5f);
    }


    #endregion


}
