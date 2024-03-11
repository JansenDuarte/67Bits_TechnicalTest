using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region SIMPLE_SINGLETON
    public static GameManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
            return;
        }
        else
        {
            Destroy(this);
        }
    }
    #endregion


    private void Start()
    {
        Application.targetFrameRate = 60;
    }
}
