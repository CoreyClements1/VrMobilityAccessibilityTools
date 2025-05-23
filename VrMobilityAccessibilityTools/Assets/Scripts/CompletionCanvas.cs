using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    [ColorUsage(showAlpha: false, hdr: true)]
    [SerializeField] private Color startEnvironmentColor, endEnvironmentColor;
    [SerializeField] private bool trackEnemies, trackCleanable, trackTrees;

    private int totalObjs, objsLeft;
    private float startAtmosphereVal, endAtmosphereVal;

    private bool updatingAtmosphere = true;


    #endregion


    #region MONOBEHAVIOUR AND SETUP


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        CleanableObject[] cleanableObjects = FindObjectsOfType<CleanableObject>();
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        EvilTree[] evilTrees = FindObjectsOfType<EvilTree>();

        totalObjs = 0;
        if (trackCleanable)
            totalObjs += cleanableObjects.Length;
        if (trackEnemies)
            totalObjs += enemies.Length;
        if (trackTrees)
            totalObjs += evilTrees.Length;

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

        // Lerp the lighting to the correct value based on percentage
        float lerpedAtmosphereVal = Mathf.Lerp(startAtmosphereVal, endAtmosphereVal, percent);
        Color lerpedEnvironmentColor = Color.Lerp(startEnvironmentColor, endEnvironmentColor, percent);

        if (updatingAtmosphere)
        {
            LeanTween.value(gameObject, skyboxMat.GetFloat("_AtmosphereThickness"), lerpedAtmosphereVal, .5f).setOnUpdate((value) =>
            {
                skyboxMat.SetFloat("_AtmosphereThickness", value);
            });
            LeanTween.value(gameObject, RenderSettings.ambientLight, lerpedEnvironmentColor, .5f).setOnUpdate((value) =>
            {
                RenderSettings.ambientLight = value;
            });

            halloweenMusic.volume = 0.1f * (1f - Mathf.Pow(percent, 3f));
            natureSounds.volume = 2f * percent;

            if (percent >= 1f)
            {
                StartCoroutine(WinCo());
            }
        }
    }


    #endregion


    #region RESET ATMOSPHERE AND WIN


    private void OnApplicationQuit()
    {
        ResetAtmostphere();
    }

    public void OnStop()
    {
        StartCoroutine(StopCo());
    }

    private IEnumerator StopCo()
    {
        updatingAtmosphere = false;

        LeanTween.value(halloweenMusic.gameObject, halloweenMusic.volume, 0f, .5f).setOnUpdate((float value) =>
        {
            halloweenMusic.volume = value;
        });
        LeanTween.value(natureSounds.gameObject, natureSounds.volume, 0f, .5f).setOnUpdate((float value) =>
        {
            natureSounds.volume = value;
        });

        yield return new WaitForSeconds(.5f);

        ResetAtmostphere();
    }

    private void ResetAtmostphere()
    {
        updatingAtmosphere = false;

        LeanTween.cancel(gameObject);
        skyboxMat.SetFloat("_AtmosphereThickness", startAtmosphereVal);
        RenderSettings.ambientLight = startEnvironmentColor;
    }

    private IEnumerator WinCo()
    {
        updatingAtmosphere = false;

        WinCanvas.Instance.FadeOut();
        SceneLoader.Instance.ManualSetIsLoading(true);

        yield return new WaitForSeconds(5f);

        WinCanvas.Instance.FadeJustText();
        SceneLoader.Instance.LoadScene("Menu", true);
    }


    #endregion


}
