using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{

    public static SceneLoader Instance;

    public InputActionReference primaryButton;
    public InputActionReference secondaryButton;

    private bool isLoading = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        primaryButton?.action.Enable();
        secondaryButton?.action.Enable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        primaryButton?.action.Disable();
        secondaryButton?.action.Disable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (isLoading) return;

        bool primaryHeld = primaryButton.action.IsPressed();
        bool secondaryHeld = secondaryButton.action.IsPressed();

        if (primaryHeld && secondaryHeld)
        {
            LoadScene("Menu", 0f);
        }
    }

    public void LoadSceneWithDelay(string sceneToLoad)
    {
        LoadScene(sceneToLoad, 2f);
    }

    public void LoadScene(string sceneToLoad, float delay)
    {
        if (isLoading) return;

        isLoading = true;

        StartCoroutine(DelayThenLoad(sceneToLoad, delay));
    }

    private IEnumerator DelayThenLoad(string sceneToLoad, float delay)
    {
        yield return new WaitForSeconds(delay);

        FadeOutCanvas.Instance.FadeOut();
        yield return new WaitForSeconds(.5f);

        Debug.Log("Loading scene " + sceneToLoad);
        SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isLoading = false;
        FadeOutCanvas.Instance.FadeIn();
    }

}
