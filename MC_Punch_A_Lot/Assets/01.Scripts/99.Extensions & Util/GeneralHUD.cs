using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


[DisallowMultipleComponent]
public class GeneralHUD : MonoBehaviour
{
    /*      GeneralHUD        */
    /*
            Created by Jansen Duarte    
    
            Controls inputs, rigidbody movement, enemy pile behaviour,
            camera positioning, money and level.
    */




    #region PUBLIC_METHODS
    public void Update_Money(int _value)
    {
        StartCoroutine(Animate_Money(_value));
        m_lastMoneyValue = _value;
    }

    public void Update_Level(int _level)
    {
        StartCoroutine(Animate_Level(_level));
    }

    public void Update_PileSize(int _size, int _capacity)
    {
        m_pile.text = _size.ToString();
        m_pile.color = Color.Lerp(m_normalColor, m_fullPileColor, _size / (float)_capacity);
    }
    #endregion //PUBLIC_METHODS



    #region PRIVATE_METHODS
    private IEnumerator Animate_Money(int _value)
    {
        Color targetColor = (_value > m_lastMoneyValue) ? Color.green : Color.red;
        float timer = 0f;
        m_moneyText.text = _value.ToString();

        while (timer < MONEY_ANIMATION_TIMER / 2f)
        {
            m_moneyText.color = Color.Lerp(m_normalColor, targetColor, timer * 2f);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        while (timer < MONEY_ANIMATION_TIMER)
        {
            m_moneyText.color = Color.Lerp(targetColor, m_normalColor, timer * 2f);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        yield break;
    }

    private IEnumerator Animate_Level(int _value)
    {
        m_levelText.color = Color.yellow;
        m_levelText.text = "LEVEL: " + _value;
        float timer = 0f;
        yield return new WaitForSeconds(LEVEL_ANIMATION_TIMER / 2f);

        while (timer < LEVEL_ANIMATION_TIMER / 2f)
        {
            m_levelText.color = Color.Lerp(Color.yellow, m_normalColor, timer * 2f);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        yield break;
    }
    #endregion //PRIVATE_METHODS



    #region VARIABLES
    [SerializeField] TextMeshProUGUI m_levelText;
    [SerializeField] TextMeshProUGUI m_moneyText;
    [SerializeField] TextMeshProUGUI m_pile;
    [SerializeField] Color m_normalColor;
    [SerializeField] Color m_fullPileColor;

    int m_lastMoneyValue = 0;

    private const float MONEY_ANIMATION_TIMER = 1f;
    private const float LEVEL_ANIMATION_TIMER = 1f;

    #endregion //VARIABLES
}
