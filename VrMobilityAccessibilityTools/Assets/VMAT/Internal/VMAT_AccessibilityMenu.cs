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


    public static VMAT_AccessibilityMenu Instance;

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

    public bool reachExtensionEnabled { get; private set; } = false;
    public bool normalizationEnabled { get; private set; } = false;


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

        // Get all hand controllers
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            handControllers.AddRange(root.GetComponentsInChildren<VMAT_HandController>(true));

        heightAdjuster = FindObjectOfType<VMAT_HeightAdjuster>(true);

        HideMenu();
    }


    private void Start()
    {
        // Ping all values
        OnReachExtensionEnabledChange();
        OnReachExtensionAmtChange();
        OnResetReachExtensionOrigin();
        OnHeightAdjustChange();
        OnNormalizationEnabledChange();
        OnNormalizationAmtChange();
    }


    #endregion


    #region REACH EXTENSION


    // Enables / disables reach extension
    public void OnReachExtensionEnabledChange()
    {
        reachExtensionEnabled = reachExtensionEnabledToggle.isOn;

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
        normalizationEnabled = normalizationEnabledToggle.isOn;

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
        normalizationAmtText.text = "Normalization: " + percentage.ToString("F2") + "%";

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
