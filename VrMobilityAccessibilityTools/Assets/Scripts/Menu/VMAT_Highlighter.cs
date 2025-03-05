using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VMAT_Highlighter : MonoBehaviour
{

    // VMAT_Highlighter handles the highlight animation of a highlighted object


    #region VARIABLES


    [SerializeField] private CanvasGroup canvGroup;


    #endregion


    #region MONOBEHAVIOUR AND SETUP


    // Awake, bounce the alpha back and forth
    private void Awake()
    {
        canvGroup.LeanAlpha(.5f, .3f).setLoopPingPong();
    }


    #endregion


}
