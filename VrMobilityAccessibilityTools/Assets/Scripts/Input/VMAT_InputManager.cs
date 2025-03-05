using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VMAT_InputManager : MonoBehaviour
{

    // VMAT_InputManager manages the various inputs of the VR Mobility Accessibility Toolkit


    #region VARIABLES


    public static VMAT_InputManager Instance;

    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private VMAT_Menu accessibilityMenu;
    [SerializeField] private VMAT_Highlighter highlighterPrefab;
    private InputActionMap actionMap;

    private InputAction primaryButtonAction;
    private InputAction secondaryButtonAction;
    private InputAction joystickAction;

    private float joystickButtonThreshold = 0.5f;
    private bool joystickUpPressed = false;
    private bool joystickDownPressed = false;
    private bool joystickLeftPressed = false;
    private bool joystickRightPressed = false;

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

        // Set up actions
        actionMap = inputActions.FindActionMap("VMAT");

        primaryButtonAction = actionMap.FindAction("PrimaryButton");
        secondaryButtonAction = actionMap.FindAction("SecondaryButton");
        joystickAction = actionMap.FindAction("Joystick");

        accessibilityMenu.gameObject.SetActive(false);
    }


    // OnEnable, subscribe to input actions
    private void OnEnable()
    {
        actionMap?.Enable();

        if (primaryButtonAction != null)
            primaryButtonAction.performed += OnPrimaryButton;

        if (secondaryButtonAction != null)
            secondaryButtonAction.performed += OnSecondaryButton;

        if (joystickAction != null)
            joystickAction.performed += OnJoystick;
    }


    // OnDisable, unsubscribe from input actions
    private void OnDisable()
    {
        actionMap?.Disable();

        if (primaryButtonAction != null)
            primaryButtonAction.performed -= OnPrimaryButton;

        if (secondaryButtonAction != null)
            secondaryButtonAction.performed -= OnSecondaryButton;

        if (joystickAction != null)
            joystickAction.performed -= OnJoystick;
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
            accessibilityMenu.gameObject.SetActive(false);
            currentlyNavigatedMenu = pendingNavigatedMenu;
        }
        // If main accessibility menu is not shown, show it
        // Save currently navigated menu into pending, in case we need to go back to it
        else
        {
            accessibilityMenuShown = true;
            accessibilityMenu.gameObject.SetActive(true);
            accessibilityMenu.StartNavigatingMenu();
            pendingNavigatedMenu = currentlyNavigatedMenu;
            currentlyNavigatedMenu = accessibilityMenu;
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
