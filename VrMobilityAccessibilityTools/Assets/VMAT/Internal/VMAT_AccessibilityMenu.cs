using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Windows;

public class VMAT_AccessibilityMenu : VMAT_Menu
{

    // VMAT_AccessibilityMenu handles the display and control of the main accessibility VMAT menu


    #region VARIABLES


    private List<VMAT_HandController> handControllers = new List<VMAT_HandController>();
    private VMAT_HeightAdjuster heightAdjuster;

    [SerializeField] private Toggle reachExtensionEnabledToggle;
    [SerializeField] private Slider reachExtensionAmtSlider;
    [SerializeField] private TMP_Text reachExtensionAmtText;
    [SerializeField] private Toggle normalizationEnabledToggle;
    [SerializeField] private Slider normalizationAmtSlider;
    [SerializeField] private TMP_Text normalizationAmtText;
    [SerializeField] private Slider heightAdjustSlider;
    [SerializeField] private CanvasGroup canvGroup;


    #endregion


    #region MONOBEHAVIOUR AND SETUP


    private void Awake()
    {
        // Get all hand controllers
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            handControllers.AddRange(root.GetComponentsInChildren<VMAT_HandController>(true));

        heightAdjuster = FindObjectOfType<VMAT_HeightAdjuster>(true);

        HideMenu();
    }


    private void Start()
    {
        // Load player prefs
        if (PlayerPrefs.GetInt("VMAT_ReachExtensionEnabled", 0) == 1)
            reachExtensionEnabledToggle.isOn = true;
        if (PlayerPrefs.GetFloat("VMAT_ReachExtensionAmt", -1f) != -1f)
            reachExtensionAmtSlider.value = PlayerPrefs.GetFloat("VMAT_ReachExtensionAmt");
        if (PlayerPrefs.GetInt("VMAT_NormalizationEnabled", 0) == 1)
            normalizationEnabledToggle.isOn = true;
        if (PlayerPrefs.GetFloat("VMAT_NormalizationAmt", -1f) != -1f)
            normalizationAmtSlider.value = PlayerPrefs.GetFloat("VMAT_NormalizationAmt");
        if (PlayerPrefs.GetFloat("VMAT_HeightAdjustment", -1f) != -1f)
            heightAdjustSlider.value = PlayerPrefs.GetFloat("VMAT_HeightAdjustment");

        // Ping all values
        OnReachExtensionEnabledChange();
        OnReachExtensionAmtChange();
        OnResetReachExtensionOrigin();
        OnHeightAdjustChange();
        OnNormalizationEnabledChange();
        OnNormalizationAmtChange();

        StartCoroutine(WaitThenResetHandOrigins());
    }

    private IEnumerator WaitThenResetHandOrigins()
    {
        yield return null;
        foreach (VMAT_HandController handController in handControllers)
        {
            handController.ResetReachExtensionOrigin();
        }
    }


    #endregion


    #region REACH EXTENSION


    // Enables / disables reach extension
    public void OnReachExtensionEnabledChange()
    {
        VMAT_Options.Instance.reachExtensionEnabled = reachExtensionEnabledToggle.isOn;
        PlayerPrefs.SetInt("VMAT_ReachExtensionEnabled", (reachExtensionEnabledToggle.isOn ? 1 : 0));

        foreach (VMAT_HandController handController in handControllers)
        {
            handController.SetEnabledReachExtension(reachExtensionEnabledToggle.isOn);
        }
    }


    // Adjusts reach extension scale
    public void OnReachExtensionAmtChange()
    {
        float newScale = 1f + (reachExtensionAmtSlider.value * 3f);
        reachExtensionAmtText.text = "Reach Extension Scale: " + newScale.ToString("F2");
        PlayerPrefs.SetFloat("VMAT_ReachExtensionAmt", reachExtensionAmtSlider.value);

        foreach (VMAT_HandController handController in handControllers)
        {
            handController.SetReachExtensionScale(newScale);
        }
    }


    // Resets reach extension origin to current positions
    public void OnResetReachExtensionOrigin()
    {
        foreach (VMAT_HandController handController in handControllers)
        {
            handController.ResetReachExtensionOrigin();
        }
    }


    #endregion


    #region NORMALIZATION


    // Enables / disables normalization
    public void OnNormalizationEnabledChange()
    {
        VMAT_Options.Instance.normalizationEnabled = normalizationEnabledToggle.isOn;
        PlayerPrefs.SetInt("VMAT_NormalizationEnabled", (normalizationEnabledToggle.isOn ? 1 : 0));

        foreach (VMAT_HandController handController in handControllers)
        {
            handController.SetEnabledNormalization(normalizationEnabledToggle.isOn);
        }
    }


    // Adjusts normalization scale
    public void OnNormalizationAmtChange()
    {
        float newScale = 0.2f + (normalizationAmtSlider.value * 2f);
        float t = (newScale - 0.2f) / (2.2f - 0.2f);
        float percentage = 10 + t * (100 - 10);
        normalizationAmtText.text = "Normalization: " + Mathf.FloorToInt(percentage).ToString() + "%";
        PlayerPrefs.SetFloat("VMAT_NormalizationAmt", normalizationAmtSlider.value);

        foreach (VMAT_HandController handController in handControllers)
        {
            handController.SetNormalizationScale(newScale);
        }
    }


    #endregion


    #region OTHER


    // Adjusts height
    public void OnHeightAdjustChange()
    {
        PlayerPrefs.SetFloat("VMAT_HeightAdjustment", heightAdjustSlider.value);
        heightAdjuster.SetHeightScale(heightAdjustSlider.value);
    }


    // Shows menu
    public override void ShowMenu()
    {
        canvGroup.alpha = 1f;
    }

    // Hides menu
    public override void HideMenu()
    {
        canvGroup.alpha = 0f;
    }


    #endregion


}
