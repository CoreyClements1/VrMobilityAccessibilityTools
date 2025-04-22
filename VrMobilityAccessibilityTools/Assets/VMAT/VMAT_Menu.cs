using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VMAT_Menu : MonoBehaviour
{

    // VMAT_Menu handles the navigation of a UI menu via accessible controls


    #region VARIABLES


    private Selectable currentSelectable;

    private VMAT_Highlighter activeHighlighter = null;


    #endregion


    #region START / STOP NAVIGATION


    // Starts navigating this menu via accessible controls
    public void StartNavigatingMenu()
    {
        // Get first selectable in children
        if (currentSelectable == null)
            currentSelectable = GetComponentInChildren<Selectable>();

        // If selectable is still null, we cannot navigate this menu (no selectables found)
        if (currentSelectable == null)
            return;

        currentSelectable.Select();
        HighlightSelectable(currentSelectable);
    }


    // Stops navigating this menu via accessible controls
    public void StopNavigatingMenu()
    {
        if (activeHighlighter != null)
            Destroy(activeHighlighter.gameObject);
    }


    #endregion


    #region NAVIGATION CONTROL


    // Navigates one menu item in given direction
    public void NavigateInDir(Vector2 dir)
    {
        Selectable oldSelectable = currentSelectable;

        if (dir == Vector2.up)
        {
            currentSelectable = currentSelectable.FindSelectableOnUp();

            // If we can't go up, find the "last" selectable and loop
            if (currentSelectable == null)
            {
                Selectable[] selectables = GetComponentsInChildren<Selectable>();
                currentSelectable = selectables[selectables.Length - 1];
            }
        }
        else if (dir == Vector2.down)
        {
            currentSelectable = currentSelectable.FindSelectableOnDown();

            // If we can't go down, find the "first" selectable and loop
            if (currentSelectable == null)
            {
                Selectable[] selectables = GetComponentsInChildren<Selectable>();
                currentSelectable = selectables[0];
            }
        }
        else if (dir == Vector2.right)
        {
            currentSelectable = currentSelectable.FindSelectableOnRight();
        }
        else if (dir == Vector2.left)
        {
            currentSelectable = currentSelectable.FindSelectableOnLeft();
        }

        if (currentSelectable == null)
            currentSelectable = oldSelectable;

        if (currentSelectable != null)
        {
            currentSelectable.Select();
            HighlightSelectable(currentSelectable);
        }
    }


    #endregion


    #region HIGHLIGHT


    private void HighlightSelectable(Selectable selectable)
    {
        if (selectable == null)
            return;

        // Destroy old highlighter (if it exists)
        if (activeHighlighter != null)
            Destroy(activeHighlighter.gameObject);

        // Find the visible portion (toggle, slider, button...) and attach highlighter
        VMAT_InputManager inpManager = FindObjectOfType<VMAT_InputManager>();
        activeHighlighter = Instantiate(inpManager.GetHighlighterPrefab(), selectable.transform);
        activeHighlighter.transform.SetAsFirstSibling();
    }


    #endregion


    #region SELECT


    // Tries to select the currently highlighted selectable item
    // (i.e., toggle a toggle, move a slider, press a button...)
    public void TrySelectHighlighted()
    {
        if (currentSelectable == null)
            return;

        switch (currentSelectable)
        {
            case Button button:
                // Button, press the button
                button.onClick.Invoke();
                break;
            case Toggle toggle:
                // Toggle, swap the toggle state
                toggle.isOn = !toggle.isOn;
                break;
            case Slider slider:
                // Slider, adjust the value
                if (slider.value > .9f)
                    slider.value = 0f;
                else
                    slider.value += .16667f;
                break;
            default:
                Debug.LogWarning("VMAT - unsupported UI selectable type. Current types supported are toggles, sliders, and buttons." +
                    " Users can highlight this selectable, but will be unable to select it until support is added.");
                break;
        }

    }


    #endregion


    #region OTHER


    public virtual void ShowMenu()
    {
        gameObject.SetActive(true);
    }

    public virtual void HideMenu()
    {
        gameObject.SetActive(false);
    }


    #endregion


}
