using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CompletionCanvas : MonoBehaviour
{

    // CompletionCanvas displays the completion amount of the graveyard cleaning


    #region VARIABLES


    public static CompletionCanvas Instance;

    [SerializeField] private TMP_Text percentDisplay;
    [SerializeField] private Slider completionSlider;
    [SerializeField] private Material skyboxMat;
    [SerializeField] private AudioSource halloweenMusic, natureSounds;

    private int totalObjs, objsLeft;
    private float startAtmosphereVal, endAtmosphereVal;


    #endregion


    #region MONOBEHAVIOUR AND SETUP


    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        Instance = this;

        CleanableObject[] cleanableObjects = FindObjectsOfType<CleanableObject>();
        Spider[] spiders = FindObjectsOfType<Spider>();
        EvilTree[] evilTrees = FindObjectsOfType<EvilTree>();

        totalObjs = spiders.Length + cleanableObjects.Length;
        objsLeft = totalObjs;

        startAtmosphereVal = 4.5f;
        endAtmosphereVal = 1f;

        UpdateCanvas();
    }


    #endregion


    #region COMPLETION


    // Called when a cleanable object is destroyed
    public void OnObjDestroyed()
    {
        objsLeft--;
        UpdateCanvas();
    }


    // Updates canvas value to current completion
    private void UpdateCanvas()
    {
        float percent = (float)((float)totalObjs - objsLeft) / totalObjs;
        completionSlider.value = percent;
        int percentInt = Mathf.RoundToInt(percent * 100f);
        percentDisplay.text = "<b>" + percentInt + "%</b> UN-SPOOKIFIED";
        float lerpedVal = Mathf.Lerp(startAtmosphereVal, endAtmosphereVal, percent);

        LeanTween.value(gameObject, skyboxMat.GetFloat("_AtmosphereThickness"), lerpedVal, .5f).setOnUpdate((value) =>
        {
            skyboxMat.SetFloat("_AtmosphereThickness", value);
        });

        halloweenMusic.volume = 0.1f * (1f - Mathf.Pow(percent, 3f));
        natureSounds.volume = 2f * percent;
    }


    private void OnApplicationQuit()
    {
        LeanTween.cancel(gameObject);
        skyboxMat.SetFloat("_AtmosphereThickness", startAtmosphereVal);
    }


    #endregion


}
