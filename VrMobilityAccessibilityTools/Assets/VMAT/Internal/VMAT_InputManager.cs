using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

public class VMAT_InputManager : MonoBehaviour
{

    // VMAT_InputManager manages the various inputs of the VR Mobility Accessibility Toolkit


    #region VARIABLES


    public static VMAT_InputManager Instance;

    [SerializeField] private VMAT_Menu accessibilityMenu;
    [SerializeField] private VMAT_Highlighter highlighterPrefab;
    private InputActionMap actionMap;

    [SerializeField] private InputActionReference openMenuAction;
    [SerializeField] private InputActionReference selectMenuItemAction;
    [SerializeField] private InputActionReference joystickNavigationAction;

    private List<MonoBehaviour> disabledWithMenu = new List<MonoBehaviour>();
    private List<bool> disabledWithMenuLastState = new List<bool>();

    private float joystickButtonThreshold = 0.5f;
    private bool joystickUpPressed = false;
    private bool joystickDownPressed = false;

    private bool accessibilityMenuShown = false;

    private VMAT_Menu pendingNavigatedMenu = null;
    private VMAT_Menu currentlyNavigatedMenu = null;


    #endregion


    #region MONOBEHAVIOUR / SETUP


    // Awake, setup input actions
    private void Awake()
    {
        // Setup singleton
        if (Instance != null)
            Destroy(gameObject);
        Instance = this;

        // Set up objects which must be deactivated when the menu is opened
        MonoBehaviour[] locoProviders = FindObjectsOfType<LocomotionProvider>();
        foreach (MonoBehaviour provider in locoProviders)
        {
            disabledWithMenu.Add(provider);
            disabledWithMenuLastState.Add(provider.enabled);
        }
    }


    // OnEnable, subscribe to input actions
    private void OnEnable()
    {
        actionMap?.Enable();

        if (openMenuAction != null)
        {
            openMenuAction.action.performed += OnSecondaryButton;
            openMenuAction.action.Enable();
        }
        
        if (selectMenuItemAction != null)
        {
            selectMenuItemAction.action.performed += OnPrimaryButton;
            selectMenuItemAction.action.Enable();
        }

        if (joystickNavigationAction != null)
        {
            joystickNavigationAction.action.performed += OnJoystick;
            joystickNavigationAction.action.Enable();
        }
    }


    // OnDisable, unsubscribe from input actions
    private void OnDisable()
    {
        actionMap?.Disable();

        if (openMenuAction != null)
        {
            openMenuAction.action.performed -= OnSecondaryButton;
            openMenuAction.action.Disable();
        }

        if (selectMenuItemAction != null)
        {
            selectMenuItemAction.action.performed -= OnPrimaryButton;
            selectMenuItemAction.action.Disable();
        }

        if (joystickNavigationAction != null)
        {
            joystickNavigationAction.action.performed -= OnJoystick;
            joystickNavigationAction.action.Disable();
        }
    }


    #endregion


    #region INPUT: BUTTONS


    // OnPrimaryButton (used for select/confirm)
    private void OnPrimaryButton(InputAction.CallbackContext ctx)
    {
        // If a menu is being navigated, try to select highlighted item
        if (currentlyNavigatedMenu != null)
        {
            currentlyNavigatedMenu.TrySelectHighlighted();
        }
    }


    // OnSecondaryButton (used for open/close menu)
    private void OnSecondaryButton(InputAction.CallbackContext ctx)
    {
        // If main accessibility menu is shown, hide it
        // Return navigation to previously navigated menu (which can be null, meaning no navigation)
        if (accessibilityMenuShown)
        {
            accessibilityMenuShown = false;
            accessibilityMenu.StopNavigatingMenu();
            accessibilityMenu.HideMenu();
            currentlyNavigatedMenu = pendingNavigatedMenu;

            if (currentlyNavigatedMenu == null)
            {
                // Re-enable locomoters, if pertinent (set to their last state before menu opening)
                for (int i = 0; i < disabledWithMenu.Count; i++)
                {
                    disabledWithMenu[i].enabled = disabledWithMenuLastState[i];
                }
            }
        }
        // If main accessibility menu is not shown, show it
        // Save currently navigated menu into pending, in case we need to go back to it
        else
        {
            accessibilityMenuShown = true;
            accessibilityMenu.ShowMenu();
            accessibilityMenu.StartNavigatingMenu();
            pendingNavigatedMenu = currentlyNavigatedMenu;
            currentlyNavigatedMenu = accessibilityMenu;

            for (int i = 0; i < disabledWithMenu.Count; i++) {
                disabledWithMenuLastState[i] = disabledWithMenu[i].enabled;
                disabledWithMenu[i].enabled = false;
            }
        }
    }


    #endregion


    #region INPUT: JOYSTICK


    // OnJoystick (used for adjustments and navigation)
    private void OnJoystick(InputAction.CallbackContext ctx)
    {
        Vector2 axes = ctx.ReadValue<Vector2>();

        float vertical = axes.y;

        // Pressed up
        if (vertical > joystickButtonThreshold && !joystickUpPressed)
        {
            joystickUpPressed = true;
            OnJoystickButtonPressed(Vector2.up);
        }
        // Pressed down
        else if (vertical < -joystickButtonThreshold && !joystickDownPressed)
        {
            joystickDownPressed = true;
            OnJoystickButtonPressed(Vector2.down);
        }
        // Vertically neutral
        else if (vertical < joystickButtonThreshold && vertical > -joystickButtonThreshold)
        {
            joystickUpPressed = false;
            joystickDownPressed = false;
        }
    }

    
    // Called by OnJoystick when a joystick direction is "pressed" as if it were a button
    // Used primarily for navigating a menu
    private void OnJoystickButtonPressed(Vector2 dir)
    {
        // If a menu is being navigated, navigate that menu accordingly
        if (currentlyNavigatedMenu != null)
        {
            currentlyNavigatedMenu.NavigateInDir(dir);
        }
    }


    #endregion


    #region OTHER


    public VMAT_Highlighter GetHighlighterPrefab()
    {
        return highlighterPrefab;
    }


    #endregion


}
