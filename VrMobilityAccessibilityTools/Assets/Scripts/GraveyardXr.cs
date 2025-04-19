using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraveyardXr : MonoBehaviour
{

    public static GraveyardXr Instance;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
