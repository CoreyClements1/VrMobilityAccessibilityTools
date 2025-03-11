using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VMAT_AccessibilityMenu : MonoBehaviour
{

    // VMAT_AccessibilityMenu handles the display and control of the main accessibility VMAT menu


    #region VARIABLES


    private List<VMAT_HandController> handControllers = new List<VMAT_HandController>();
    private VMAT_HeightAdjuster heightAdjuster;

    [SerializeField] private Toggle reachExtensionEnabledToggle;
    [SerializeField] private Slider reachExtensionAmtSlider;
    [SerializeField] private TMP_Text reachExtensionAmtText;
    [SerializeField] private Slider heightAdjustSlider;


    #endregion


    #region MONOBEHAVIOUR AND SETUP


    private void Awake()
    {
        // Get all hand controllers
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            handControllers.AddRange(root.GetComponentsInChildren<VMAT_HandController>(true));

        heightAdjuster = FindObjectOfType<VMAT_HeightAdjuster>(true);
    }


    private void Start()
    {
        // Ping all values
        OnReachExtensionEnabledChange();
        OnReachExtensionAmtChange();
        OnResetReachExtensionOrigin();
        OnHeightAdjustChange();
    }


    #endregion


    #region REACH EXTENSION


    // Enables / disables reach extension
    public void OnReachExtensionEnabledChange()
    {
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


    #region OTHER


    // Adjusts height
    public void OnHeightAdjustChange()
    {
        heightAdjuster.SetHeightScale(heightAdjustSlider.value);
    }


    #endregion


}
