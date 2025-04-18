using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeOutCanvas : MonoBehaviour
{

    public static FadeOutCanvas Instance;

    [SerializeField] private CanvasGroup canvGroup;


    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void FadeOut()
    {
        canvGroup.LeanAlpha(1f, .5f);
    }

    public void FadeIn()
    {
        canvGroup.LeanAlpha(0f, .5f);
    }
}
