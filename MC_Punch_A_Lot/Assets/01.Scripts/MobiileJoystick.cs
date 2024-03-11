using UnityEngine;


[DisallowMultipleComponent, RequireComponent(typeof(RectTransform))]
public class MobiileJoystick : MonoBehaviour
{
    /*      MobileJoystick        */
    /*
            Created by Jansen Duarte    
    
            Controls UI graphics of the joystick and calculations.
    */


    #region PUBLIC_METHODS
    public void Enable_Joystick(Vector2 _position)
    {
        m_joystick.sizeDelta = m_joystickSize;
        m_joystick.anchoredPosition = Clamp_Position(_position);
    }

    public Vector2 Move_Joystick(Vector2 _movement)
    {
        float maxMovement = m_joystickSize.x / 2f;   //It's a square

        if (Vector2.Distance(_movement, m_joystick.anchoredPosition) > maxMovement)
            m_knob.anchoredPosition = (_movement - m_joystick.anchoredPosition).normalized * maxMovement;
        else
            m_knob.anchoredPosition = _movement - m_joystick.anchoredPosition;

        return m_knob.anchoredPosition / maxMovement;
    }

    public void Disable_Joystick()
    {
        m_joystick.anchoredPosition = Vector2.zero;
        gameObject.SetActive(false);
    }
    #endregion //PUBLIC_METHODS

    #region PRIVATE_METHODS
    private Vector2 Clamp_Position(Vector2 _position)
    {
        if (_position.x < m_joystickSize.x / 2f)
            _position.x = m_joystickSize.x / 2f;

        if (_position.y < m_joystickSize.y / 2f)
            _position.y = m_joystickSize.y / 2f;
        else if (_position.y > Screen.height - m_joystickSize.y / 2f)
            _position.y = Screen.height - m_joystickSize.y / 2f;

        return _position;
    }
    #endregion //PRIVATE_METHODS




    #region SERIALIZED_MEMBERS
    [SerializeField] RectTransform m_joystick;
    [SerializeField] RectTransform m_knob;
    [SerializeField] Vector2 m_joystickSize = new Vector2(300, 300);
    #endregion //SERIALIZED_MEMBERS
}
