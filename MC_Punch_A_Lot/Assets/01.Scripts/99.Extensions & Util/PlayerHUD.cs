using System.Collections;
using UnityEngine;
using UnityEngine.UI;



[DisallowMultipleComponent]
public class PlayerHUD : MonoBehaviour
{
    /*      PlayerHUD        */
    /*
            Created by Jansen Duarte    
    
            Controls the level up indicator and allways points at the camera.
    */



    private void Update() { transform.LookAt(m_mainCamera.transform); }



    #region PUBLIC_METHODS
    public void Show_LevelUp_Indicator(float _timer)
    {
        if (co_levelUpIndicator == null)
            co_levelUpIndicator = StartCoroutine(LevelUp_Indicator(_timer));
    }


    #endregion //PUBLIC_METHODS

    #region PRIVATE_METHODS
    private IEnumerator LevelUp_Indicator(float _timer)
    {
        m_levelUpParent.gameObject.SetActive(true);
        float aux = 0f;
        while (aux <= _timer)
        {
            m_levelUpIndicator.fillAmount = aux / _timer;
            yield return new WaitForEndOfFrame();
            aux += Time.deltaTime;
        }

        m_levelUpIndicator.fillAmount = 0f;
        m_levelUpParent.gameObject.SetActive(false);

        co_levelUpIndicator = null;
        yield break;
    }


    #endregion //PRIVATE_METHODS




    #region VARIABLES
    [SerializeField] Camera m_mainCamera;
    [SerializeField] Image m_levelUpParent;
    [SerializeField] Image m_levelUpIndicator;

    Coroutine co_levelUpIndicator;
    #endregion //VARIABLES
}