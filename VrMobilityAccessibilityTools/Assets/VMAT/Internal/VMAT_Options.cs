using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VMAT_Options : MonoBehaviour
{

    // VMAT_Options is the central connection point of the VMAT, with options for custom control


    #region VARIABLES


    public static VMAT_Options Instance;

    [Tooltip("The input action which will open the VMAT menu")]
    public InputActionReference openMenuAction;
    [Tooltip("The input action which will select an item in the VMAT menu")]
    public InputActionReference selectMenuItemAction;
    [Tooltip("The input action which will be used to navigate the VMAT menu (should be a joystick or similar input)")]
    public InputActionReference joystickNavigationAction;

    [Range(0f, 1f)]
    public float menuAudioVolume = 1f;

    [HideInInspector] public bool reachExtensionEnabled;
    [HideInInspector] public bool normalizationEnabled;


    #endregion


    #region SETUP


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    #endregion


}
