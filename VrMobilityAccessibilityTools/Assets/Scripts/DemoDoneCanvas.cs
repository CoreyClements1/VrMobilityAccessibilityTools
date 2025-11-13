using System.Collections;
using TMPro;
using UnityEngine;
using VERA;

public class DemoDoneCanvas : MonoBehaviour
{

    // DemoDoneCanvas displays when the demo is done and allows the user to continue or quit


    #region VARIABLES


    public static DemoDoneCanvas Instance;
    [SerializeField] private CanvasGroup canvGroup;
    [SerializeField] private TMP_Text countdownText;

    private bool triggered = false;
    private int countdownCount = 5;


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
    }


    public IEnumerator WaitThenDemoDone()
    {
        yield return new WaitForSeconds(30f);

        ShowDemoDone();
    }


    #endregion


    #region DISPLAY


    public void ShowDemoDone()
    {
        if (triggered)
            return;

        triggered = true;

#if VERAFile_SpecialEvents
        VERAFile_SpecialEvents.CreateCsvEntry(0, "Progression", "DemoConditionsMet", transform);
#endif
        canvGroup.LeanAlpha(1f, 1f);

        StartCoroutine(CountdownCo());
    }
    
    
    private IEnumerator CountdownCo()
    {
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(1f);
            countdownCount--;
            countdownText.text = countdownCount.ToString();
        }

        yield return new WaitForSeconds(1f);
        countdownText.text = "Quitting...";
        OnConcludeDemo();
    }


    public void OnConcludeDemo()
    {
        #if VERAFile_SpecialEvents
        VERAFile_SpecialEvents.CreateCsvEntry(0, "Progression", "DemoConcluded", transform);
        #endif
        VERASessionManager.FinalizeSession();
    }


    #endregion


}
