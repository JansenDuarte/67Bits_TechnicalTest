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

    public void Play_PunchSFX()
    {
        SFXs[0].clip = punchSounds[Random.Range(0, punchSounds.Length)];
        SFXs[0].Play();
    }

    public void Play_MoneySFX()
    {
        SFXs[1].clip = moneySounds[Random.Range(0, moneySounds.Length)];
        SFXs[1].Play();
    }

    public void Play_HurtSFX()
    {
        SFXs[2].clip = hurtSounds[Random.Range(0, hurtSounds.Length)];
        SFXs[2].Play();
    }



    #region VARIABLES

    [SerializeField] AudioSource[] SFXs;
    [SerializeField] AudioSource BGM;

    [SerializeField] AudioClip[] punchSounds;
    [SerializeField] AudioClip[] moneySounds;
    [SerializeField] AudioClip[] hurtSounds;

    #endregion //VARIABLES

}
