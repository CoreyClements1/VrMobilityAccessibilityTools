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
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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

    public void ManualSetIsLoading(bool loading)
    {
        isLoading = loading;
    }

    private void Update()
    {
        if (isLoading) return;

        bool primaryHeld = primaryButton.action.IsPressed();
        bool secondaryHeld = secondaryButton.action.IsPressed();

        if (primaryHeld && secondaryHeld)
        {
            LoadScene("Menu");
        }
    }

    public void LoadScene(string sceneToLoad)
    {
        LoadScene(sceneToLoad, false);
    }

    public void LoadScene(string sceneToLoad, bool overrideIsLoading)
    {
        if (isLoading && !overrideIsLoading) return;

        isLoading = true;

        if (sceneToLoad != "Menu")
        {
            SfxManager.Instance.PlaySfx(SfxManager.SoundEffect.EvilLaugh, transform.position, false);
            StartCoroutine(WaitThenLoadScene(sceneToLoad, 2.5f));
        }
        else
        {
            StartCoroutine(LoadSceneCo(sceneToLoad));
        }
    }

    private IEnumerator WaitThenLoadScene(string sceneToLoad, float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(LoadSceneCo(sceneToLoad));
    }

    private IEnumerator LoadSceneCo(string sceneToLoad)
    {
        FadeOutCanvas.Instance.FadeOut();
        CompletionCanvas completionCanvas = FindObjectOfType<CompletionCanvas>();
        if (completionCanvas != null)
        {
            completionCanvas.OnStop();
        }

        yield return new WaitForSeconds(.7f);

        SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isLoading = false;
        FadeOutCanvas.Instance.FadeIn();
        WinCanvas.Instance.HideImmediate();

        if (scene.name == "Menu")
        {
            GraveyardXr.Instance.transform.position = new Vector3(0.062f, 0.72f, 6.077f);
        }
        else
        {
            GraveyardXr.Instance.transform.position = new Vector3(0.587f, 0.344f, 5.532f);
        }

        SceneManager.MoveGameObjectToScene(GraveyardXr.Instance.gameObject, scene);
    }

}
